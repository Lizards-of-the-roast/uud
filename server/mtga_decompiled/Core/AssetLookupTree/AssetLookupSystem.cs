using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Card;
using Wizards.Mtga.AssetLookupTree.Watcher;

namespace AssetLookupTree;

public readonly struct AssetLookupSystem
{
	public readonly AssetLookupTreeLoader TreeLoader;

	public readonly IBlackboard Blackboard;

	private static readonly AssetLookupTree<MaterialOverride> _unusedTMaterialOverride;

	private static readonly AssetLookupTree<TextureOverride> _unusedTTextureOverride;

	private static readonly AssetLookupTree<ArtIdOverride> _unusedTArtIdOveride;

	private static readonly AssetLookupTree<ColorOverride> _unusedTColorOveride;

	private static readonly AssetLookupTree<TextColor> unusedTTextColor;

	private static readonly AssetLookupTree<FaceSymbol> _unusedTFaceSymbol;

	private static readonly AssetLookupTree<LandSymbol> _unusedTLandSymbol;

	private static readonly AssetLookupTree<ExpansionSymbol> _unusedTExpansionSymbol;

	public IWatcherLogger WatcherLogger => TreeLoader?.WatcherLogger;

	public AssetLookupSystem(AssetLookupTreeLoader treeLoader, IBlackboard blackboard)
	{
		TreeLoader = treeLoader;
		Blackboard = blackboard;
	}
}
