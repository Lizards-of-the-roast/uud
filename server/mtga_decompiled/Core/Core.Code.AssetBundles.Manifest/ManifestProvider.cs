using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.BI;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.IO;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Storage;
using Wotc.Mtga;

namespace Core.Code.AssetBundles.Manifest;

public class ManifestProvider
{
	private ManifestMetadataProvider _metadataProvider = Pantry.Get<ManifestMetadataProvider>();

	private IStorageContext _storageContext = PlatformContext.GetStorageContext();

	private List<AssetFileManifest> _cachedManifests = new List<AssetFileManifest>();

	private bool _cached;

	private static Logger Logger => ManifestUtils.Logger;

	public Promise<List<AssetFileManifest>> GetManifests()
	{
		if (_cached)
		{
			return new SimplePromise<List<AssetFileManifest>>(_cachedManifests);
		}
		string assetBundleStoragePath = _storageContext.GetAssetBundleStoragePath();
		Logger.Info("Bundle download destination: " + assetBundleStoragePath);
		return _metadataProvider.GetManifestMetadata().Scatter(GetManifest).Gather()
			.Convert(delegate(IEnumerable<AssetFileManifest> r)
			{
				_cachedManifests = r.ToList();
				_cached = true;
				return _cachedManifests;
			})
			.IfError(delegate(Promise<List<AssetFileManifest>> p)
			{
				Logger.Error(p.Error.ToString());
			})
			.Then(delegate
			{
				Logger.Debug("Manifest download complete");
			});
	}

	private Promise<AssetFileManifest> GetManifest(AssetBundleManifestMetadata metadata)
	{
		string filename = metadata.Filename;
		string assetBundleStoragePath = _storageContext.GetAssetBundleStoragePath();
		FileInfo localManifestFile = new FileInfo(Path.Combine(assetBundleStoragePath, filename));
		bool flag = true;
		if (localManifestFile.SafeExists())
		{
			string hash = HashingUtilities.GetHash(localManifestFile.FullName);
			if (metadata.Hash != hash)
			{
				Logger.Info(filename + " has incorrect hash; redownloading...");
				localManifestFile.SafeDelete();
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			BIEventType.AssetBundleManifestDownloadStart.SendWithDefaults(("Filename", metadata.Filename));
			return ManifestUtils.DownloadManifestToFile(localManifestFile, metadata).IfError(delegate(Promise<FileInfo> p)
			{
				BIEventType.AssetBundleManifestDownloadFailure.SendWithDefaults(("Filename", metadata.Filename), ("Error", p.Error.Message));
				return (metadata.Priority != AssetPriority.Future) ? p : new SimplePromise<FileInfo>(localManifestFile);
			}, metadata.Priority == AssetPriority.Future).IfSuccess(delegate
			{
				BIEventType.AssetBundleManifestDownloadEnd.SendWithDefaults(("Filename", metadata.Filename));
			})
				.Convert((FileInfo result) => ManifestUtils.LoadManifestFromFile(result, metadata));
		}
		return new SimplePromise<AssetFileManifest>(ManifestUtils.LoadManifestFromFile(localManifestFile, metadata));
	}
}
