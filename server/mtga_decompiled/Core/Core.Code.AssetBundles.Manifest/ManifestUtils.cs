using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Assets.Core.Code.AssetBundles;
using Core.BI;
using Newtonsoft.Json;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.IO;
using Wizards.Mtga.Logging;
using Wotc.Mtga;

namespace Core.Code.AssetBundles.Manifest;

public static class ManifestUtils
{
	public const string LOCAL_FILE_PREFIX = "file://";

	public static Logger Logger { get; set; }

	static ManifestUtils()
	{
		Logger = new UnityCrossThreadLogger("Manifest");
		LoggerManager.Register(Logger);
	}

	public static List<AssetBundleManifestMetadata> ParsePointerData(string data)
	{
		if (!data.StartsWith("["))
		{
			return ParsePointerFileV1(data);
		}
		return ParsePointerFileV2(data);
	}

	public static List<AssetBundleManifestMetadata> ParsePointerFileV1(string pointerData)
	{
		List<AssetBundleManifestMetadata> list = new List<AssetBundleManifestMetadata>(2);
		AssetPriority priority = AssetPriority.Automatic;
		string[] array = pointerData.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (!string.IsNullOrWhiteSpace(text))
			{
				list.Add(new AssetBundleManifestMetadata(priority, string.Empty, text));
				priority = AssetPriority.Future;
			}
		}
		return list;
	}

	public static List<AssetBundleManifestMetadata> ParsePointerFileV2(string pointerData)
	{
		List<AssetBundleManifestMetadata> list = null;
		try
		{
			list = JsonConvert.DeserializeObject<List<AssetBundleManifestMetadata>>(pointerData);
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
		}
		return list ?? new List<AssetBundleManifestMetadata>();
	}

	public static AssetFileManifest LoadManifestFromFile(FileInfo localManifestFile, AssetBundleManifestMetadata manifestMetadata)
	{
		if (manifestMetadata.Priority == AssetPriority.Future && !localManifestFile.Exists)
		{
			return new AssetFileManifest(AssetPriority.Future, Enumerable.Empty<AssetFileInfo>());
		}
		using StreamReader reader = localManifestFile.SafeOpenText();
		return LoadManifestFromStream(reader, manifestMetadata);
	}

	public static AssetFileManifest LoadManifestFromStream(StreamReader reader, AssetBundleManifestMetadata manifestMetadata)
	{
		try
		{
			AssetFileManifest assetFileManifest = AssetFileManifestExtensions.LoadFrom(reader);
			if (manifestMetadata.Priority != AssetPriority.Automatic)
			{
				assetFileManifest = new AssetFileManifest(manifestMetadata.Priority, assetFileManifest.AssetFileInfoByName.Values);
			}
			assetFileManifest.ManifestName = manifestMetadata.Filename;
			assetFileManifest.Hash = manifestMetadata.Hash;
			assetFileManifest.Category = manifestMetadata.Category;
			return assetFileManifest;
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
			BIEventType.AssetBundleManifestParseFailure.SendWithDefaults(("Filename", manifestMetadata.Filename), ("Error", ex.Message), ("Count", 1.ToString()));
			throw new ManifestLoadException(ex);
		}
	}

	public static Promise<FileInfo> DownloadManifestToFile(FileInfo destinationFile, AssetBundleManifestMetadata metadata)
	{
		string manifestFileName = metadata.Filename;
		Logger.Debug("Downloading " + manifestFileName + "...");
		return DownloadManifestData(metadata).IfSuccess((Promise<string> p) => WriteToFile(destinationFile, p.Result).AsPromise()).Convert((string _) => destinationFile).IfError(delegate(Promise<FileInfo> p)
		{
			Logger.Error("Failed to get manifest data for " + manifestFileName + ": " + p.Error);
		});
	}

	private static async Task<string> WriteToFile(FileInfo localManifestFile, string manifestData)
	{
		FileStream writer = localManifestFile.SafeOpen(FileMode.Create);
		object obj = null;
		int num = 0;
		string result = default(string);
		object obj4;
		try
		{
			StreamWriter stringWriter = new StreamWriter(writer);
			object obj2 = null;
			int num2 = 0;
			string text = default(string);
			try
			{
				await stringWriter.WriteAsync(manifestData);
				text = manifestData;
				num2 = 1;
			}
			catch (object obj3)
			{
				obj2 = obj3;
			}
			if (stringWriter != null)
			{
				await ((IAsyncDisposable)stringWriter).DisposeAsync();
			}
			obj4 = obj2;
			if (obj4 != null)
			{
				ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
			}
			if (num2 == 1)
			{
				result = text;
				num = 1;
			}
		}
		catch (object obj3)
		{
			obj = obj3;
		}
		if (writer != null)
		{
			await ((IAsyncDisposable)writer).DisposeAsync();
		}
		obj4 = obj;
		if (obj4 != null)
		{
			ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
		}
		if (num == 1)
		{
			return result;
		}
		string result2 = default(string);
		return result2;
	}

	private static Promise<string> DownloadManifestData(AssetBundleManifestMetadata metadata)
	{
		_ = metadata.Filename;
		bool flag = string.IsNullOrEmpty(metadata.Category);
		Uri uri = Pantry.Get<AssetBundleSourcesModel>().CurrentSource.GetBundleUrl(metadata);
		Logger.Info($"GET manifest from {uri} ...");
		if (uri.AbsoluteUri.StartsWith("file://"))
		{
			Logger.Info("AbsoluteUri contains file prefix, reading from local path");
			return LoadManifestFromLocalFile(uri, flag).IfError(delegate(Error e)
			{
				HandleError(e, uri);
			});
		}
		return WebPromise.Get(uri.AbsoluteUri, new Dictionary<string, string>(), flag).IfError(delegate(Error e)
		{
			HandleError(e, uri);
		});
	}

	private static void HandleError(Error e, Uri uri)
	{
		int code = e.Code;
		string text = ((code <= 0) ? $"Error getting asset bundle manifest file from {uri}: ({e}" : ((code != 404) ? $"Unexpected HTTP response when fetching asset bundle manifest file from {uri}: ({e.Code}) {e.Message}" : $"Asset bundle manifest file was not found: {uri}"));
		string message = text;
		Logger.Error(message);
	}

	public static bool DoesLocalFileExist(Uri uri)
	{
		return File.Exists(uri.LocalPath);
	}

	private static Promise<string> LoadManifestFromLocalFile(Uri uri, bool compressedManifest)
	{
		if (!DoesLocalFileExist(uri))
		{
			return new SimplePromise<string>(new Error(404, "File not found"));
		}
		string localPath = uri.LocalPath;
		string result;
		if (compressedManifest)
		{
			FileInfo fileInfo = new FileInfo(localPath);
			string text = Path.Combine(fileInfo.DirectoryName, $"temp_{fileInfo.Name}");
			CompressionUtilities.DecompressFile(localPath, text);
			result = File.ReadAllText(text);
			File.Delete(text);
		}
		else
		{
			result = File.ReadAllText(localPath);
		}
		return new SimplePromise<string>(result);
	}
}
