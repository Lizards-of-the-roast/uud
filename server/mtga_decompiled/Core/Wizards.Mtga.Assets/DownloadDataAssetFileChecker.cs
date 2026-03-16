using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Core.BI;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga.IO;
using Wizards.Mtga.Storage;
using Wotc.Mtga;

namespace Wizards.Mtga.Assets;

public class DownloadDataAssetFileChecker : IAssetFileChecker
{
	private IAssetFileCrcChecker? _assetCrcValidator;

	private readonly IBILogger _biLogger;

	private const string ZIP_EXTENSION = ".gz";

	public DownloadDataAssetFileChecker(IAssetFileCrcChecker? validatorGetter = null, IBILogger? biLogger = null)
	{
		_assetCrcValidator = validatorGetter;
		_biLogger = biLogger;
	}

	public Task CheckExistingAssetFiles(Func<string, IAssetFileSignature> getAssetFileSignature, ConcurrentDictionary<string, string> renamableAssets, Action<FileInfo, AssetFileCheckResult> resultCallback, IStorageContext storageContext, IAssetFileValidationDetector validationDetector, IProgress<AssetBundleProvisionProgress>? progress = null)
	{
		return Task.Run(delegate
		{
			DirectoryInfo downloadDirectory = new DirectoryInfo(storageContext.GetAssetBundleStoragePath());
			ValidateExistingBundles(downloadDirectory, getAssetFileSignature, renamableAssets, validationDetector, resultCallback, progress);
		});
	}

	private void ValidateExistingBundles(DirectoryInfo downloadDirectory, Func<string, IAssetFileSignature> getAssetFileSignature, ConcurrentDictionary<string, string> renamableAssets, IAssetFileValidationDetector validationDetector, Action<FileInfo, AssetFileCheckResult> resultCallback, IProgress<AssetBundleProvisionProgress>? progress = null)
	{
		if (!downloadDirectory.Exists)
		{
			return;
		}
		UnzipFiles(downloadDirectory.FullName);
		List<string> list = WindowsSafePath.EnumerateFiles(downloadDirectory.FullName, "*", SearchOption.AllDirectories).ToList();
		int currentFile = 0;
		float totalFiles = list.Count;
		AssetBundleProvisionStage stage = AssetBundleProvisionStage.CollectCompletedBundles;
		Parallel.ForEach(list, delegate(string bundlePath)
		{
			bool flag = validationDetector.RequiresValidation(bundlePath);
			progress?.Report(stage.ToProgress((float)currentFile / totalFiles));
			Interlocked.Increment(ref currentFile);
			FileInfo fileInfo = new FileInfo(bundlePath);
			string text = fileInfo.Name;
			bool flag2 = !text.Contains("Manifest");
			if (IsZipFile(fileInfo))
			{
				resultCallback(fileInfo, AssetFileCheckResult.Keep);
			}
			else if (flag2)
			{
				IAssetFileSignature assetFileSignature = getAssetFileSignature(text);
				if (assetFileSignature == null)
				{
					if (!renamableAssets.TryGetValue(text, out string value))
					{
						resultCallback(fileInfo, AssetFileCheckResult.Remove);
						return;
					}
					string text2 = Path.Join(fileInfo.DirectoryName, value);
					fileInfo.MoveTo(text2);
					fileInfo = new FileInfo(text2);
					text = value;
					assetFileSignature = getAssetFileSignature(text);
				}
				if (flag)
				{
					if (assetFileSignature.Sha256Hash == null && !assetFileSignature.Crc32.HasValue)
					{
						SendFileCleanupError(text, "Missing file signature in manifest");
						resultCallback(fileInfo, AssetFileCheckResult.Keep);
						return;
					}
					if (assetFileSignature.Crc32.HasValue && _assetCrcValidator != null)
					{
						if (!_assetCrcValidator.CheckAssetCrc(bundlePath, assetFileSignature.Crc32.Value).Result)
						{
							SendFileCleanupError(text, "Deleting bundle with invalid CRC");
							resultCallback(fileInfo, AssetFileCheckResult.Corrupt);
						}
						else
						{
							resultCallback(fileInfo, AssetFileCheckResult.Keep);
						}
						return;
					}
					if (assetFileSignature.Length != fileInfo.SafeGetLength())
					{
						SendFileCleanupError(text, $"Deleting bundle with unexpected length {fileInfo.SafeGetLength()}, expected length {assetFileSignature.Length}");
						resultCallback(fileInfo, AssetFileCheckResult.Corrupt);
						return;
					}
					SHA256 sHA = SHA256.Create();
					byte[] array;
					using (FileStream inputStream = fileInfo.SafeOpenRead())
					{
						array = sHA.ComputeHash(inputStream);
					}
					if (!((IStructuralEquatable)array).Equals((object)assetFileSignature.Sha256Hash, (IEqualityComparer)EqualityComparer<byte>.Default))
					{
						SendFileCleanupError(text, "Deleting bundle with invalid hash");
						resultCallback(fileInfo, AssetFileCheckResult.Corrupt);
						return;
					}
				}
				resultCallback(fileInfo, AssetFileCheckResult.Keep);
			}
		});
		progress?.Report(stage.ToCompletedProgress(1L));
	}

	private void UnzipFiles(string downloadDirectory)
	{
		IReadOnlyList<string> readOnlyList = WindowsSafePath.EnumerateFiles(downloadDirectory, "*.gz", SearchOption.AllDirectories).ToArray();
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			return;
		}
		HashSet<string> corruptedFiles = new HashSet<string>();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Parallel.ForEach(readOnlyList, new ParallelOptions
		{
			MaxDegreeOfParallelism = 4
		}, delegate(string filePath)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			try
			{
				CompressionUtilities.DecompressFile(fileInfo);
			}
			catch (Exception ex)
			{
				SimpleLog.LogError(ex.ToString());
				corruptedFiles.Add(fileInfo.Name);
				fileInfo.IsReadOnly = false;
				File.Delete(filePath);
			}
		});
		stopwatch.Stop();
		string text = $"Decompressed {readOnlyList.Count} gzip files ({stopwatch.ElapsedMilliseconds}ms) ";
		string text2 = ((corruptedFiles.Count == 0) ? string.Empty : string.Format("Found {0} corrupted files: {1}", corruptedFiles.Count, string.Join(", ", corruptedFiles)));
		if (!string.IsNullOrEmpty(text2))
		{
			SimpleLog.LogError(text2);
		}
		_biLogger?.Send(ClientBusinessEventType.ClientLogAlert, new ClientLogAlert
		{
			Message = text + text2
		});
	}

	private bool IsZipFile(FileInfo file)
	{
		return file.Extension == ".gz";
	}

	private void SendFileCleanupError(string filename, string error)
	{
		SimpleLog.LogError("[DownloadDataAssetFileChecker] Error " + error + " : Filename : " + filename);
		BIEventType.FileCleanupCheckError.SendWithDefaults(("Filename", filename), ("Error", error));
	}
}
