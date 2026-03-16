using System;
using AssetLookupTree;
using AssetLookupTree.Nodes;
using AssetLookupTree.TreeLoading.CachePatterns;
using AssetLookupTree.TreeLoading.LoadPatterns;
using AssetLookupTree.TreeLoading.SavePatterns;
using UnityEngine;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.AssetLookupTree.Watcher;

public class AssetLookupTreeLoader : IAssetLookupTreeLoader
{
	private readonly bool _isPlaying;

	private IBILogger _biLogger;

	public ITreeLoadPattern LoadPattern { get; }

	public ITreeSavePattern SavePattern { get; }

	public ITreeCachePattern CachePattern { get; }

	public IWatcherLogger WatcherLogger { get; }

	public AssetLookupTreeLoader(ITreeLoadPattern loadPattern, ITreeSavePattern savePattern, ITreeCachePattern cachePattern, IWatcherLogger watcherLogger, IBILogger biLogger = null)
	{
		LoadPattern = loadPattern ?? new NullTreeLoadPattern();
		SavePattern = savePattern ?? new NullTreeSavePattern();
		CachePattern = cachePattern ?? new NullTreeCachePattern();
		WatcherLogger = watcherLogger ?? new NullLogger();
		_isPlaying = Application.isPlaying;
		_biLogger = biLogger;
	}

	public bool TryLoadTree<T>() where T : class, IPayload
	{
		AssetLookupTree<T> loadedTree;
		return TryLoadTree(out loadedTree);
	}

	public INode<T> GetRootNodeOfTree<T>() where T : class, IPayload
	{
		TryLoadTree(out AssetLookupTree<T> loadedTree);
		return loadedTree?.Root;
	}

	public bool TryLoadTree<T>(out AssetLookupTree<T> loadedTree) where T : class, IPayload
	{
		if (CachePattern.TryPullTree(out loadedTree))
		{
			return true;
		}
		try
		{
			loadedTree = LoadPattern.LoadTree<T>();
			if (loadedTree != null)
			{
				loadedTree.InjectLogger(WatcherLogger);
				CachePattern.PushTree(loadedTree);
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			_biLogger?.Send(ClientBusinessEventType.ResourceError, new ResourceError
			{
				Message = "Failed to load tree",
				Error = ex.Message,
				EventTime = DateTime.UtcNow
			});
		}
		loadedTree = null;
		return false;
	}

	public AssetLookupTree<T> LoadTree<T>(bool returnNewTree = true) where T : class, IPayload
	{
		if (TryLoadTree(out AssetLookupTree<T> loadedTree))
		{
			return loadedTree;
		}
		if (returnNewTree)
		{
			return new AssetLookupTree<T>();
		}
		return null;
	}

	public bool SaveTree<T>(AssetLookupTree<T> tree) where T : class, IPayload
	{
		if (tree == null)
		{
			return false;
		}
		try
		{
			SavePattern.SaveTree(tree);
			CachePattern.PushTree<T>(null);
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}
}
