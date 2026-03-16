using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.Card;
using Wotc.Mtga.Wrapper;

namespace Core.Meta.MainNavigation.Store.Utils;

public class StoreSetUtils
{
	public static Sprite SpriteForSetName(CollationMapping collationId, AssetLookupSystem assetLookupSystem, AssetTracker assetTracker, SetAvailability availableInStandard)
	{
		assetLookupSystem.Blackboard.Clear();
		string text = collationId.ToString();
		assetLookupSystem.Blackboard.SetCardDataExtensive(CardDataExtensions.CreateBlankExpansionCard(text, text));
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ExpansionSymbol> loadedTree))
		{
			ExpansionSymbol obj = loadedTree?.GetPayload(assetLookupSystem.Blackboard);
			ExpansionSymbol.ExpansionSymbolVariant variant = ((availableInStandard != SetAvailability.HistoricOnly && availableInStandard != SetAvailability.EternalOnly) ? ExpansionSymbol.ExpansionSymbolVariant.White : ExpansionSymbol.ExpansionSymbolVariant.Historic);
			AltAssetReference<Sprite> altAssetReference = obj?.GetStoreExpansionIconRef(variant);
			if (altAssetReference != null && altAssetReference.RelativePath != null)
			{
				return AssetLoader.AcquireAndTrackAsset(assetTracker, "ExpansionSymbol._symbol", altAssetReference);
			}
		}
		return null;
	}

	public static void LogoForSetName(AssetLookupSystem assetLookupSystem, ref AssetLoader.AssetTracker<Texture> textureTracker, ref RawImage rawImage, CollationMapping collationId)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.BoosterCollationMapping = collationId;
		if (!assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree))
		{
			return;
		}
		Logo payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
		if (payload != null)
		{
			if (textureTracker == null)
			{
				textureTracker = new AssetLoader.AssetTracker<Texture>("SetLogoFromNameTracker");
			}
			string headerFilePath = payload.GetHeaderFilePath();
			rawImage.texture = textureTracker.Acquire(headerFilePath);
		}
	}
}
