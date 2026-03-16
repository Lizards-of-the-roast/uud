using System;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.TreeLoading.CachePatterns;
using AssetLookupTree.TreeLoading.DeserializationPatterns;
using AssetLookupTree.TreeLoading.LoadPatterns;
using AssetLookupTree.TreeLoading.SavePatterns;
using Wizards.Mtga;
using Wizards.Mtga.AssetLookupTree.Watcher;

namespace Core.Code.AssetLookupTree.AssetLookup;

public class AssetLookupManagerFactory
{
	public static AssetLookupManager Create()
	{
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		HostPlatform hostPlatform = currentEnvironment.HostPlatform;
		if ((uint)(hostPlatform - 1) <= 1u)
		{
			IWatcherLogger watcherLogger = new NullLogger();
			ITreeLoadPattern loadPattern = new AssetLoaderTreeLoadPattern(new PackedTreeDeserializerPattern());
			IBILogger iBILogger = null;
			iBILogger = Pantry.Get<IBILogger>();
			return new AssetLookupManager(new AssetLookupSystem(new AssetLookupTreeLoader(loadPattern, new NullTreeSavePattern(), new RuntimeTreeCachePattern(), watcherLogger, iBILogger), new Blackboard()));
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
