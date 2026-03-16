using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Assets.Core.Code.AssetBundles;
using Core.BI;
using Core.Code.AssetBundles;
using Core.Code.AssetBundles.Manifest;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Mtga.Configuration;
using Wizards.Mtga.Diagnostics;
using Wizards.Mtga.IO;
using Wizards.Mtga.Logging;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Storage;
using Wotc.Mtga.Extensions;

namespace Wizards.Mtga.Assets;

public class AssetBundleProvisioner
{
	private enum FileCheckMode
	{
		Safe,
		Normal
	}

	private class AssetFileExceptionLogger
	{
		private ConcurrentDictionary<Type, HashSet<string>> _exceptions;

		public AssetFileExceptionLogger()
		{
			_exceptions = new ConcurrentDictionary<Type, HashSet<string>>();
		}

		public void AppendException(Exception exception, FileInfo fileInfo)
		{
			Type type = exception.GetType();
			if (!_exceptions.TryGetValue(type, out HashSet<string> value))
			{
				value = (_exceptions[type] = new HashSet<string>());
			}
			value.Add(fileInfo.FullName);
		}
	}

	private readonly Dictionary<AssetPriority, IAssetBundleDownloadQueue> _queuesByPriority = new Dictionary<AssetPriority, IAssetBundleDownloadQueue>();

	private long _downloadedBytes;

	private Stopwatch _downloadTime = new Stopwatch();

	private const uint MAX_FAILURES = 3u;

	public static IAssetBundleSource Source { get; private set; }

	public bool KeepUnusedBundles { get; }

	private IStorageContext StorageContext { get; }

	private UnityLogger Logger { get; }

	public List<AssetFileInfo> AvailableBundles { get; } = new List<AssetFileInfo>(10000);

	public AssetPriority AssetPriorityLimit { get; set; }

	public bool CanStartDownload
	{
		get
		{
			if (CompletedStage == AssetBundleProvisionStage.CollectDownloadList)
			{
				return _queuesByPriority.Count > 0;
			}
			return false;
		}
	}

	private AssetBundleProvisionStage CompletedStage { get; set; }

	public List<AssetFileManifest>? ActiveManifests { get; private set; }

	private ManifestProvider ManifestProvider { get; }

	private static int MaxParallelism => Environment.ProcessorCount;

	public static AssetBundleProvisioner Create()
	{
		IAssetBundleSource currentSource = Pantry.Get<AssetBundleSourcesModel>().CurrentSource;
		UnityLogger logger = new UnityLogger("AssetBundles", LoggerLevel.Debug);
		LoggerManager.Register(logger);
		return new AssetBundleProvisioner(currentSource, logger);
	}

	public AssetBundleProvisioner(IAssetBundleSource source, UnityLogger logger)
	{
		Source = source;
		StorageContext = PlatformContext.GetStorageContext();
		Logger = logger;
		bool flag = source.GetBundleUrl("").AbsoluteUri.Equals(BundleSource.DefaultSourceLocation.AbsoluteUri);
		KeepUnusedBundles = !flag && MDNPlayerPrefs.KeepUnusedBundles;
		ExcludeDownloadsFromBackup();
		ManifestProvider = Pantry.Get<ManifestProvider>();
	}

	public bool CheckHasMissingBundlesInQueue(AssetPriority maxPriority)
	{
		return _queuesByPriority.Exists<KeyValuePair<AssetPriority, IAssetBundleDownloadQueue>>((KeyValuePair<AssetPriority, IAssetBundleDownloadQueue> x) => x.Key <= maxPriority && x.Value.Count > 0);
	}

	private long GetAvailableStorageBytes()
	{
		return StorageContext.GetAvailableBytes(StorageContext.GetAssetBundleStoragePath());
	}

	private static bool PriorityFilter(AssetFileManifest manifest, AssetPriority maxPriority)
	{
		return manifest.Priority <= maxPriority;
	}

	public void ResetTimers()
	{
		_downloadedBytes = 0L;
		_downloadTime.Reset();
	}

	public void LogTimerResults(AssetPriority priority)
	{
		AssetFileCheckLogger.GenerateTimingsLog(priority.ToString(), _downloadTime.Elapsed, _downloadedBytes, MaxParallelism);
	}

	public virtual async Task PrepareDownload(IAssetPathResolver? embeddedAssetPathResolver = null, IProgress<AssetBundleProvisionProgress>? provisionProgress = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		AssetPriority maxPriority = AssetPriority.Future;
		if (StorageContext.EnsureDownloadDirectoryExists())
		{
			BIEventType.AssetDownloadDirectoryCreated.SendWithDefaults();
		}
		if (ActiveManifests == null || CompletedStage != AssetBundleProvisionStage.CollectCompletedBundles)
		{
			Promise<List<AssetFileManifest>> getManifests = GetAvailableManifests(provisionProgress);
			await getManifests.AsTask;
			if (!getManifests.Successful)
			{
				if (getManifests.Error.Exception != null)
				{
					throw getManifests.Error.Exception;
				}
				throw new AssetException(getManifests.Error.Message ?? "Unknown error fetching asset manifests");
			}
			ActiveManifests = getManifests.Result.Where((AssetFileManifest m) => PriorityFilter(m, maxPriority)).ToList();
			cancellationToken.ThrowIfCancellationRequested();
		}
		AssetPriorityLimit = maxPriority;
		CompletedStage = AssetBundleProvisionStage.GetManifest;
		AvailableBundles.Clear();
		IAssetFileValidationDetector validationDetector;
		FileCheckMode checkMode = SetUpFileValidation(out validationDetector);
		(ConcurrentDictionary<string, AssetFileInfo>, ConcurrentDictionary<string, string>) tuple = GatherExpectedAssets(ActiveManifests, provisionProgress);
		ConcurrentDictionary<string, AssetFileInfo> expectedAssetsByName = tuple.Item1;
		ConcurrentDictionary<string, string> item = tuple.Item2;
		ConcurrentDictionary<string, FileInfo> existingAssetsByName = new ConcurrentDictionary<string, FileInfo>();
		ConcurrentDictionary<string, FileInfo> assetsToDeleteByName = new ConcurrentDictionary<string, FileInfo>();
		ConcurrentDictionary<string, FileInfo> corruptAssetsByName = new ConcurrentDictionary<string, FileInfo>();
		AssetFileExceptionLogger exceptionLogger = new AssetFileExceptionLogger();
		BlockingCollection<FileInfo> deleteQueue = new BlockingCollection<FileInfo>(500);
		Task deleteTask = Task.Run(delegate
		{
			FileInfo item2;
			while (deleteQueue.TryTake(out item2, -1, cancellationToken))
			{
				try
				{
					item2.SafeDelete();
					WindowsSafePath.DeleteFile(item2.FullName + ".dat");
				}
				catch (IOException exception)
				{
					exceptionLogger.AppendException(exception, item2);
				}
			}
		});
		using (AssetBundleValidator bundleValidator = new AssetBundleValidator())
		{
			await new DownloadDataAssetFileChecker(bundleValidator).CheckExistingAssetFiles((string name) => (!expectedAssetsByName.TryGetValue(name, out AssetFileInfo value2)) ? null : value2, item, resultCallback, StorageContext, validationDetector, provisionProgress);
			deleteQueue.CompleteAdding();
			AssetFileCheckLogger.LogResultsToFile(ActiveManifests.Select((AssetFileManifest x) => x.Hash), expectedAssetsByName, existingAssetsByName, assetsToDeleteByName, corruptAssetsByName);
			int num = expectedAssetsByName.Values.Count((AssetFileInfo b) => b.Priority == AssetPriority.Future);
			int num2 = expectedAssetsByName.Count - num;
			int count = assetsToDeleteByName.Count;
			int count2 = corruptAssetsByName.Count;
			BIEventType.FileCleanupCheckEnd.SendWithDefaults(("Mode", checkMode.ToString()), ("CurrentDownloadsNeeded", num2.ToString()), ("FutureDownloadsNeeded", num.ToString()), ("StaleBundles", count.ToString()), ("CorruptAssets", count2.ToString()));
			cancellationToken.ThrowIfCancellationRequested();
			if (embeddedAssetPathResolver != null)
			{
				List<string> list = new List<string>();
				foreach (KeyValuePair<string, AssetFileInfo> item3 in expectedAssetsByName)
				{
					if (embeddedAssetPathResolver.GetAssetPath(item3.Value) != null)
					{
						list.Add(item3.Key);
						AvailableBundles.Add(item3.Value);
					}
				}
				foreach (string item4 in list)
				{
					expectedAssetsByName.TryRemove(item4, out AssetFileInfo _);
				}
			}
			AvailableBundles.AddRange(from bundle in ActiveManifests.SelectMany((AssetFileManifest m) => m.AssetFileInfoByName.Values)
				where bundle.Priority < AssetPriority.Future && existingAssetsByName.ContainsKey(bundle.Name)
				select bundle);
			cancellationToken.ThrowIfCancellationRequested();
			CompletedStage = AssetBundleProvisionStage.CollectCompletedBundles;
			MDNPlayerPrefs.FileToHashOnStartup = string.Empty;
			MDNPlayerPrefs.HashAllFilesOnStartup = false;
			PopulateDownloadQueues(expectedAssetsByName.Values);
			using (deleteQueue)
			{
				await deleteTask;
			}
			await RemoveExistingManifestsAsync(ActiveManifests.Select((AssetFileManifest x) => x.ManifestName), StorageContext, exceptionLogger);
			long requiredBytes = _queuesByPriority.Sum<KeyValuePair<AssetPriority, IAssetBundleDownloadQueue>>((KeyValuePair<AssetPriority, IAssetBundleDownloadQueue> x) => x.Value.RemainingBytes);
			ThrowIfNotEnoughStorage(requiredBytes);
			CompletedStage = AssetBundleProvisionStage.CollectDownloadList;
		}
		void resultCallback(FileInfo file, AssetFileCheckResult result)
		{
			FileInfo value2;
			switch (result)
			{
			case AssetFileCheckResult.Remove:
				assetsToDeleteByName.TryAdd(file.Name, file);
				existingAssetsByName.TryRemove(file.Name, out value2);
				if (!KeepUnusedBundles)
				{
					deleteQueue.Add(file);
				}
				break;
			case AssetFileCheckResult.Keep:
			{
				existingAssetsByName.TryAdd(file.Name, file);
				assetsToDeleteByName.TryRemove(file.Name, out value2);
				expectedAssetsByName.TryRemove(file.Name, out AssetFileInfo _);
				break;
			}
			case AssetFileCheckResult.Corrupt:
				existingAssetsByName.TryRemove(file.Name, out value2);
				corruptAssetsByName.TryAdd(file.Name, file);
				try
				{
					file.SafeDelete();
					WindowsSafePath.DeleteFile(file.FullName + ".dat");
					break;
				}
				catch (IOException exception)
				{
					exceptionLogger.AppendException(exception, file);
					break;
				}
			case AssetFileCheckResult.Missing:
				break;
			}
		}
	}

	private Promise<List<AssetFileManifest>> GetAvailableManifests(IProgress<AssetBundleProvisionProgress>? provisionProgress = null)
	{
		provisionProgress?.Report(AssetBundleProvisionStage.GetManifest.ToProgress(0L, 0L));
		return ManifestProvider.GetManifests().IfSuccess(delegate
		{
			provisionProgress?.Report(AssetBundleProvisionStage.GetManifest.ToCompletedProgress(1L));
		});
	}

	private void PopulateDownloadQueues(ICollection<AssetFileInfo> assetsToEnqueue)
	{
		List<AssetFileInfo> list = new List<AssetFileInfo>(assetsToEnqueue.Where((AssetFileInfo x) => x.Priority <= AssetPriority.Boot));
		List<AssetFileInfo> list2 = new List<AssetFileInfo>(assetsToEnqueue.Where((AssetFileInfo x) => x.Priority == AssetPriority.NPE));
		List<AssetFileInfo> list3 = new List<AssetFileInfo>(assetsToEnqueue.Where((AssetFileInfo x) => x.Priority == AssetPriority.General));
		List<AssetFileInfo> list4 = new List<AssetFileInfo>(assetsToEnqueue.Where((AssetFileInfo x) => x.Priority == AssetPriority.Future));
		if (list.Count > 0)
		{
			_queuesByPriority[AssetPriority.Boot] = new ConcurrentDownloadQueue(list);
		}
		if (list2.Count > 0)
		{
			_queuesByPriority[AssetPriority.NPE] = new ConcurrentDownloadQueue(list2);
		}
		if (list3.Count > 0)
		{
			_queuesByPriority[AssetPriority.General] = new ConcurrentDownloadQueue(list3);
		}
		if (list4.Count > 0)
		{
			_queuesByPriority[AssetPriority.Future] = new ConcurrentDownloadQueue(list4);
		}
	}

	private static FileCheckMode SetUpFileValidation(out IAssetFileValidationDetector validationDetector)
	{
		if (!MDNPlayerPrefs.HashFilesSkippedOnce)
		{
			MDNPlayerPrefs.HashAllFilesOnStartup = false;
			MDNPlayerPrefs.FileToHashOnStartup = string.Empty;
			MDNPlayerPrefs.HashFilesSkippedOnce = true;
		}
		string fileToHashOnStartup = MDNPlayerPrefs.FileToHashOnStartup;
		FileCheckMode result;
		if (MDNPlayerPrefs.HashAllFilesOnStartup || (fileToHashOnStartup != string.Empty && MDNPlayerPrefs.HashedFilesLastStartup))
		{
			MDNPlayerPrefs.HashedFilesLastStartup = true;
			result = FileCheckMode.Safe;
			validationDetector = new AssetFileValidationDetector(validateFiles: true);
		}
		else if (fileToHashOnStartup != string.Empty)
		{
			MDNPlayerPrefs.HashedFilesLastStartup = true;
			result = FileCheckMode.Safe;
			validationDetector = new SpecificAssetFileValidationDetector(MDNPlayerPrefs.FileToHashOnStartup);
		}
		else
		{
			MDNPlayerPrefs.HashedFilesLastStartup = false;
			result = FileCheckMode.Normal;
			validationDetector = new AssetFileValidationDetector(validateFiles: false);
		}
		BIEventType.FileCleanupCheckStart.SendWithDefaults(("Mode", result.ToString()));
		return result;
	}

	public async Task<AssetBundleDownloadResult> DownloadBundlesAsync(AssetPriority currentPriority, IAssetBundleDownloadQueue downloadQueue, Action<AssetFileInfo> onBundleComplete, DownloadProgressReporter? downloadProgressReporter, CancellationToken cancellationToken, string destinationDirectory, IErrorReporter errorReporter)
	{
		int num = Math.Min(MaxParallelism, downloadQueue.Count);
		if (currentPriority == AssetPriority.Future)
		{
			num = Math.Min(num, Application.isMobilePlatform ? 1 : 2);
		}
		List<Task<AssetBundleDownloadResult>> list = new List<Task<AssetBundleDownloadResult>>(num);
		_downloadTime.Start();
		for (int i = 0; i < num; i++)
		{
			list.Add(Task.Run(() => BundleDownloadWorkerAsync(downloadQueue, destinationDirectory, onBundleComplete, downloadProgressReporter, cancellationToken, errorReporter), cancellationToken));
		}
		Task<AssetBundleDownloadResult[]> waitForAll = Task.WhenAll(list);
		await waitForAll;
		_downloadTime.Stop();
		if (waitForAll.Status != TaskStatus.RanToCompletion)
		{
			Logger.LogException(waitForAll.Exception);
		}
		AssetBundleDownloadResult assetBundleDownloadResult = new AssetBundleDownloadResult();
		AssetBundleDownloadResult[] result = waitForAll.Result;
		foreach (AssetBundleDownloadResult otherResult in result)
		{
			assetBundleDownloadResult.Add(otherResult);
		}
		return assetBundleDownloadResult;
	}

	private async Task<AssetBundleDownloadResult> BundleDownloadWorkerAsync(IAssetBundleDownloadQueue downloadQueue, string destinationDirectory, Action<AssetFileInfo> onBundleComplete, DownloadProgressReporter? downloadProgressReporter, CancellationToken cancellationToken, IErrorReporter errorReporter)
	{
		AssetBundleDownloadResult result = new AssetBundleDownloadResult();
		Stopwatch latency = new Stopwatch();
		long workerDownloadedBytes = 0L;
		AssetFileInfo bundleInfo;
		while (downloadQueue.TryDequeuePendingDownload(out bundleInfo))
		{
			if ((long)result.Exceptions.Count >= 3L)
			{
				downloadQueue.RequeuePendingDownload(bundleInfo);
				result.IsFailure = true;
				break;
			}
			FileInfo targetFile = new FileInfo(Path.Combine(destinationDirectory, bundleInfo.AssetType, bundleInfo.Name));
			targetFile.Directory.SafeCreate();
			Stream sourceStream;
			try
			{
				latency.Restart();
				sourceStream = await AssetBundleDownloadUtils.GetWrappedBundleStream(bundleInfo, downloadProgressReporter, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				latency.Stop();
				result.RecordLatency(latency.Elapsed.TotalMilliseconds);
				workerDownloadedBytes += ((bundleInfo.CompressedLength > 0) ? bundleInfo.CompressedLength : bundleInfo.Length);
			}
			catch (AssetDownloadError assetDownloadError)
			{
				latency.Stop();
				result.RecordLatency(latency.Elapsed.TotalMilliseconds);
				result.Exceptions.Add(assetDownloadError);
				BIEventType.AssetBundleDownloadError.SendWithDefaults(("Filename", bundleInfo.Name), ("Error", $"{assetDownloadError.StatusCode}"));
				switch (assetDownloadError.StatusCode)
				{
				case HttpStatusCode.InternalServerError:
				case HttpStatusCode.BadGateway:
				case HttpStatusCode.ServiceUnavailable:
				case HttpStatusCode.GatewayTimeout:
					await Task.Delay(TimeSpan.FromSeconds(5.0));
					break;
				}
				downloadQueue.RequeuePendingDownload(bundleInfo);
				continue;
			}
			using (sourceStream)
			{
				using FileStream file = targetFile.SafeOpen(FileMode.Create, FileAccess.Write);
				byte[] hash = await AssetBundleDownloadUtils.CopyAndHashAsync(sourceStream, file, SHA256.Create(), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				file.Close();
				AssetSignatureMismatch assetSignatureMismatch = CheckAndReportHash(errorReporter, bundleInfo, hash);
				if (assetSignatureMismatch == null)
				{
					onBundleComplete(bundleInfo);
					result.TotalCompleted++;
				}
				else
				{
					result.Exceptions.Add(assetSignatureMismatch);
					downloadQueue.RequeuePendingDownload(bundleInfo);
				}
			}
		}
		lock (this)
		{
			_downloadedBytes += workerDownloadedBytes;
			return result;
		}
	}

	public virtual async Task<AssetBundleDownloadResult> DoDownload(IProgress<AssetBundleProvisionProgress>? provisionProgress = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!CanStartDownload)
		{
			throw new InvalidOperationException($"Cannot resume download from state '{CompletedStage}'");
		}
		long num = 0L;
		int num2 = 0;
		foreach (KeyValuePair<AssetPriority, IAssetBundleDownloadQueue> item in _queuesByPriority.Where<KeyValuePair<AssetPriority, IAssetBundleDownloadQueue>>((KeyValuePair<AssetPriority, IAssetBundleDownloadQueue> x) => x.Key <= AssetPriorityLimit))
		{
			num += item.Value.RemainingBytes;
			num2 += item.Value.Count;
		}
		DownloadProgressReporter progressReporter = ((provisionProgress != null) ? new DownloadProgressReporter(provisionProgress, num) : null);
		BIEventType.AssetBundleDownloadStart.SendWithDefaults(("Count", $"{num2}"));
		SimpleLog.LogForRelease($"Starting asset bundle download with priority limit {AssetPriorityLimit}...");
		AssetBundleDownloadResult result = new AssetBundleDownloadResult();
		using CancellationTokenSource progressPingCancellationSource = BeginBiProgressReporting(cancellationToken, progressReporter);
		foreach (KeyValuePair<AssetPriority, IAssetBundleDownloadQueue> item2 in _queuesByPriority.OrderBy<KeyValuePair<AssetPriority, IAssetBundleDownloadQueue>, AssetPriority>((KeyValuePair<AssetPriority, IAssetBundleDownloadQueue> kvp) => kvp.Key))
		{
			if (item2.Key <= AssetPriorityLimit)
			{
				Report(item2.Key);
				AssetBundleDownloadResult assetBundleDownloadResult = result;
				assetBundleDownloadResult.Add(await DownloadBundlesAsync(item2.Key, item2.Value, delegate(AssetFileInfo b)
				{
					AvailableBundles.Add(b);
				}, progressReporter, cancellationToken, StorageContext.GetAssetBundleStoragePath(), Pantry.Get<IErrorReporter>()).ConfigureAwait(continueOnCapturedContext: false));
				continue;
			}
			break;
		}
		progressPingCancellationSource.Cancel();
		SimpleLog.LogForRelease($"{AssetPriorityLimit} priority download latency (ms): Min {result.LatencyMin:N2}, Max {result.LatencyMax:N2}, Total {result.LatencyTotal:N2}, Avg {result.LatencyAvg:N2}, Request count {result.TotalCompleted}");
		BIEventType.AssetBundleDownloadEnd.SendWithDefaults();
		return result;
	}

	private static (ConcurrentDictionary<string, AssetFileInfo> expected, ConcurrentDictionary<string, string> renamable) GatherExpectedAssets(List<AssetFileManifest> manifests, IProgress<AssetBundleProvisionProgress>? progress = null)
	{
		ConcurrentDictionary<string, AssetFileInfo> concurrentDictionary = new ConcurrentDictionary<string, AssetFileInfo>();
		ConcurrentDictionary<string, string> concurrentDictionary2 = new ConcurrentDictionary<string, string>();
		progress?.Report(AssetBundleProvisionStage.CollectDownloadList.ToProgress(0L, 0L));
		for (int i = 0; i < manifests.Count; i++)
		{
			AssetFileManifest assetFileManifest = manifests[i];
			foreach (KeyValuePair<string, AssetFileInfo> item in assetFileManifest.AssetFileInfoByName)
			{
				AssetFileInfo value = item.Value;
				if (assetFileManifest.Priority >= AssetPriority.Future)
				{
					value.Priority = AssetPriority.Future;
				}
				concurrentDictionary.GetOrAdd(item.Key, value);
				if (!string.IsNullOrEmpty(value.OldName))
				{
					concurrentDictionary2.TryAdd(value.OldName, value.Name);
				}
			}
			progress?.Report(AssetBundleProvisionStage.CollectDownloadList.ToProgress(i / manifests.Count, 0L));
		}
		progress?.Report(AssetBundleProvisionStage.CollectDownloadList.ToCompletedProgress(1L));
		return (expected: concurrentDictionary, renamable: concurrentDictionary2);
	}

	private AssetSignatureMismatch? CheckAndReportHash(IErrorReporter errorReporter, AssetFileInfo bundleInfo, byte[] hash)
	{
		if (bundleInfo.Sha256Hash == null || bundleInfo.Crc32.HasValue)
		{
			return null;
		}
		if (AssetBundleDownloadUtils.DoesHashMatchBundle(bundleInfo, hash))
		{
			return null;
		}
		AssetSignatureMismatch assetSignatureMismatch = new AssetSignatureMismatch(bundleInfo.Name, hash, bundleInfo.Sha256Hash);
		Logger.LogWarningForRelease(assetSignatureMismatch.Message);
		errorReporter.ReportError(assetSignatureMismatch);
		return assetSignatureMismatch;
	}

	private void ExcludeDownloadsFromBackup()
	{
		if (StorageContext is IStorageWithCloudBackup storageWithCloudBackup)
		{
			string assetBundleStoragePath = StorageContext.GetAssetBundleStoragePath();
			Directory.CreateDirectory(assetBundleStoragePath);
			storageWithCloudBackup.ExcludeFromBackup(assetBundleStoragePath);
		}
	}

	public void ThrowIfNotEnoughStorage(long requiredBytes)
	{
		long availableStorageBytes = GetAvailableStorageBytes();
		if (requiredBytes > availableStorageBytes)
		{
			BIEventType.DiskSpaceCheckFailed.SendWithDefaults(("Needed", $"{requiredBytes}"), ("Available", $"{availableStorageBytes}"));
			throw new InsufficientStorageException(requiredBytes, availableStorageBytes);
		}
	}

	public virtual void ErrorOccured()
	{
	}

	public int BundleCountRequired(AssetPriority priorityLimit)
	{
		int num = 0;
		foreach (KeyValuePair<AssetPriority, IAssetBundleDownloadQueue> item in _queuesByPriority)
		{
			if (item.Key <= priorityLimit)
			{
				num += item.Value.Count;
			}
		}
		return num;
	}

	private static CancellationTokenSource BeginBiProgressReporting(CancellationToken cancellationToken, DownloadProgressReporter? progressReporter)
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		if (progressReporter != null)
		{
			using CancellationTokenSource cancellationTokenSource2 = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
			CancellationToken cancelPingToken = cancellationTokenSource2.Token;
			Task.Run(async delegate
			{
				while (!cancelPingToken.IsCancellationRequested)
				{
					await Task.Delay(TimeSpan.FromMinutes(1.0), cancelPingToken).ConfigureAwait(continueOnCapturedContext: false);
					if (cancelPingToken.IsCancellationRequested)
					{
						break;
					}
					BIEventType.DownloadPing.SendWithDefaults(("Downloaded", $"{progressReporter.completedBytes}"), ("TotalNeeded", $"{progressReporter.expectedBytes}"));
				}
			}, cancelPingToken);
		}
		return cancellationTokenSource;
	}

	public long GetAdditionalRequiredDownloadBytes(AssetPriority maxPriority)
	{
		long num = 0L;
		foreach (KeyValuePair<AssetPriority, IAssetBundleDownloadQueue> item in _queuesByPriority)
		{
			if (item.Key <= maxPriority)
			{
				num += item.Value.RemainingBytes;
			}
		}
		return num;
	}

	public void Report(AssetPriority value)
	{
		BIEventType.AssetBundleDownloadSectionStart.SendWithDefaults(("Section", $"{value}"));
	}

	private static Task RemoveExistingManifestsAsync(IEnumerable<string> requiredManifests, IStorageContext storageContext, AssetFileExceptionLogger exceptionLogger)
	{
		return Task.Run(delegate
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(storageContext.GetAssetBundleStoragePath());
			if (!directoryInfo.Exists)
			{
				return;
			}
			foreach (string item in WindowsSafePath.EnumerateFiles(directoryInfo.FullName, "Manifest*.mtga", SearchOption.TopDirectoryOnly))
			{
				FileInfo fileInfo = new FileInfo(item);
				if (!requiredManifests.Contains<string>(fileInfo.Name))
				{
					try
					{
						if (fileInfo.SafeExists())
						{
							fileInfo.SafeDelete();
						}
					}
					catch (Exception exception)
					{
						exceptionLogger.AppendException(exception, fileInfo);
					}
				}
			}
		});
	}
}
