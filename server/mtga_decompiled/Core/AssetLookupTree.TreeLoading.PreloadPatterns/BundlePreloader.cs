using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AssetLookupTree.TreeLoading.DeserializationPatterns;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Logging;

namespace AssetLookupTree.TreeLoading.PreloadPatterns;

public class BundlePreloader : ITreePreloader
{
	private IEnumerable<Type> _allPayloadTypes;

	private IReadOnlyDictionary<string, Type> _altPayloadNameToTypes;

	private IEnumerable<string> _altBundlePaths;

	private AssetLookupTreeLoader _assetLookupTreeLoader;

	private int _parallelism;

	private readonly UnityLogger _logger;

	private readonly NewtonsoftJsonTreeDeserialization _jsonDeserializer;

	public BundlePreloader()
	{
		_logger = new UnityLogger("BundlePreloader", LoggerLevel.Debug);
		_jsonDeserializer = new NewtonsoftJsonTreeDeserialization();
	}

	private void InitializeTreeInformation()
	{
		_allPayloadTypes = AssetLookupTreeUtils.GetAllPayloadTypes();
		_altPayloadNameToTypes = _allPayloadTypes.ToDictionary((Type x) => "ALT_" + AssetLookupTreeUtils.GetPayloadTypeName(x));
		_altBundlePaths = AssetLoader.GetFilePathsForAssetType("ALT");
	}

	private void PreloadTreesFromStream(Stream fileStream)
	{
		foreach (IAssetLookupTree item in _jsonDeserializer.DeserializeTrees(fileStream, _altPayloadNameToTypes, _logger))
		{
			_assetLookupTreeLoader.CachePattern.PushTree(item.GetPayloadType(), item);
		}
	}

	private void PreloadTreesFromStreams(Stream[] fileStreams)
	{
		if (_parallelism == 0 || _parallelism == 1)
		{
			foreach (Stream fileStream in fileStreams)
			{
				PreloadTreesFromStream(fileStream);
			}
		}
		else
		{
			Parallel.ForEach(fileStreams, new ParallelOptions
			{
				MaxDegreeOfParallelism = _parallelism
			}, PreloadTreesFromStream);
		}
	}

	private int GetDegreeOfParallelism()
	{
		return Application.platform switch
		{
			RuntimePlatform.WindowsEditor => -1, 
			RuntimePlatform.WindowsPlayer => -1, 
			RuntimePlatform.OSXEditor => -1, 
			RuntimePlatform.OSXPlayer => 0, 
			RuntimePlatform.Android => 8, 
			RuntimePlatform.IPhonePlayer => 0, 
			_ => 0, 
		};
	}

	private Stream GetStreamFromPath(string filePath)
	{
		Stream result = null;
		if (FileSystemUtils.FileExists(filePath))
		{
			result = FileSystemUtils.OpenRead(filePath);
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			result = EmbeddedContentUtil.LoadEmbeddedContent(filePath);
		}
		return result;
	}

	public Task PreloadTreesAsync(AssetLookupTreeLoader loader)
	{
		_assetLookupTreeLoader = loader;
		_parallelism = GetDegreeOfParallelism();
		InitializeTreeInformation();
		Stream[] altBundleStreams = (from x in _altBundlePaths.Select(GetStreamFromPath)
			where x != null
			select x).ToArray();
		return Task.Run(delegate
		{
			PreloadTreesFromStreams(altBundleStreams);
		});
	}
}
