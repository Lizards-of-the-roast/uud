using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Mtga.Assets;

namespace Core.Code.AssetBundles.Manifest;

public class ManifestMetadataProvider
{
	private List<AssetBundleManifestMetadata> _cachedMetadata = new List<AssetBundleManifestMetadata>();

	private List<IManifestMetadataCollector> _collectors = new List<IManifestMetadataCollector>();

	private bool _collected;

	private ManifestConfiguration _manifestConfig;

	private static Wizards.Arena.Client.Logging.Logger Logger => ManifestUtils.Logger;

	public ManifestMetadataProvider()
	{
		_manifestConfig = ManifestConfiguration.Load();
		_collectors.Add(new ConfFileManifestMetadataCollector());
		_collectors.Add(new DoorbellManifestMetadataCollector());
		_collectors.Add(new PointerFileMetadataCollector());
	}

	public Promise<IReadOnlyCollection<AssetBundleManifestMetadata>> GetManifestMetadata()
	{
		if (_collected)
		{
			return new SimplePromise<IReadOnlyCollection<AssetBundleManifestMetadata>>(_cachedMetadata);
		}
		_cachedMetadata.Clear();
		return CollectManifestMetadataAsync().AsPromise().IfSuccess(delegate(Promise<IReadOnlyCollection<AssetBundleManifestMetadata>> p)
		{
			_cachedMetadata.AddRange(p.Result);
			_collected = true;
		});
	}

	private async Task<IReadOnlyCollection<AssetBundleManifestMetadata>> CollectManifestMetadataAsync()
	{
		List<Promise<IEnumerable<AssetBundleManifestMetadata>>> list = new List<Promise<IEnumerable<AssetBundleManifestMetadata>>>();
		foreach (IManifestMetadataCollector collector in _collectors)
		{
			list.Add(collector.Collect());
		}
		Dictionary<string, AssetBundleManifestMetadata> currentPointers = new Dictionary<string, AssetBundleManifestMetadata>();
		List<AssetBundleManifestMetadata> futurePointers = new List<AssetBundleManifestMetadata>();
		foreach (Promise<IEnumerable<AssetBundleManifestMetadata>> promise in list)
		{
			await promise.AsTask;
			if (promise.Successful)
			{
				foreach (AssetBundleManifestMetadata item in promise.Result)
				{
					if (_manifestConfig.CategoriesToSkip.Contains(item.Category))
					{
						Logger.Info("Skipping " + item.Category + " bundle category");
					}
					else if (item.Priority == AssetPriority.Future)
					{
						Logger.Debug($"Using manifest metadata: {item.Priority}|{item.Filename}");
						futurePointers.Add(item);
					}
					else if (currentPointers.TryAdd(item.Category ?? string.Empty, item))
					{
						Logger.Debug($"Using manifest metadata: {item.Priority}|{item.Filename}");
					}
				}
			}
			else
			{
				string text = "Error fetching ManifestMetadata: " + promise.Error;
				if (Application.isEditor)
				{
					Logger.Info(text + " This should be non-fatal in most cases.");
				}
				else
				{
					Logger.Error(text);
				}
			}
		}
		if (!Application.isEditor && !currentPointers.TryGetValue(string.Empty, out var _))
		{
			throw new AssetException("No client manifest pointer found!");
		}
		return currentPointers.Values.Concat(futurePointers).ToList();
	}
}
