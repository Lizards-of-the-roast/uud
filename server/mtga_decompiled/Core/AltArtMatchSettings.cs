using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Extractors.CardData;
using AssetLookupTree.Nodes;
using AssetLookupTree.Payloads.Card;
using Wotc.Mtga.Cards.Database;

public class AltArtMatchSettings : IAltArtSettings, IDisposable
{
	private HashSet<string> _alternateArtSkinCodes;

	public AltArtMatchSettings(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem)
	{
		_alternateArtSkinCodes = new HashSet<string>();
		SearchAssetTreeForArtOverrideSkinCodes(assetLookupSystem);
		GetAltArtPrintingsFromCardDatabase(cardDatabase);
		AltArtSettings.SetInstance(this);
	}

	public bool ShouldHideAltArtSkin(string skinCode)
	{
		if (MDNPlayerPrefs.HideAltArtStyles && _alternateArtSkinCodes != null)
		{
			return _alternateArtSkinCodes.Contains(skinCode);
		}
		return false;
	}

	public void Dispose()
	{
		_alternateArtSkinCodes = null;
	}

	private void GetAltArtPrintingsFromCardDatabase(ICardDatabaseAdapter cardDatabase)
	{
		_alternateArtSkinCodes.Clear();
		_alternateArtSkinCodes.UnionWith(cardDatabase.AltPrintingProvider.GetAltArtSkinCodes());
	}

	private void SearchAssetTreeForArtOverrideSkinCodes(AssetLookupSystem assetLookupSystem)
	{
		AssetLookupTree<ArtIdOverride> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<ArtIdOverride>(returnNewTree: false);
		if (assetLookupTree == null)
		{
			return;
		}
		foreach (INode<ArtIdOverride> item in assetLookupTree.EnumerateNodes())
		{
			if (!(item is BucketNode<ArtIdOverride, string> bucketNode) || !(bucketNode.Extractor is CardData_Skin))
			{
				continue;
			}
			foreach (KeyValuePair<string, INode<ArtIdOverride>> child in bucketNode.Children)
			{
				if (!_alternateArtSkinCodes.Contains(child.Key))
				{
					_alternateArtSkinCodes.Add(child.Key);
				}
			}
		}
	}
}
