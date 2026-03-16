using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class ExpansionSymbol : IPayload
{
	public enum ExpansionSymbolVariant
	{
		Regular,
		White,
		Historic
	}

	public readonly AltAssetReference<Sprite> CommonIconRef = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<Sprite> UncommonIconRef = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<Sprite> RareIconRef = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<Sprite> MythicIconRef = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<Sprite> CommonWhiteIconRef = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<Sprite> CommonHistoricIconRef = new AltAssetReference<Sprite>();

	public AltAssetReference<Sprite> GetIconRef(CardRarity rarity)
	{
		return rarity switch
		{
			CardRarity.MythicRare => MythicIconRef, 
			CardRarity.Rare => RareIconRef, 
			CardRarity.Uncommon => UncommonIconRef, 
			CardRarity.Common => CommonIconRef, 
			_ => CommonIconRef, 
		};
	}

	public AltAssetReference<Sprite> GetStoreExpansionIconRef(ExpansionSymbolVariant variant = ExpansionSymbolVariant.Regular)
	{
		AltAssetReference<Sprite> altAssetReference = variant switch
		{
			ExpansionSymbolVariant.White => CommonWhiteIconRef, 
			ExpansionSymbolVariant.Historic => CommonHistoricIconRef, 
			_ => CommonIconRef, 
		};
		if (string.IsNullOrEmpty(altAssetReference.RelativePath))
		{
			altAssetReference = CommonIconRef;
		}
		return altAssetReference;
	}

	public IEnumerable<string> GetFilePaths()
	{
		yield return CommonIconRef.RelativePath;
		yield return UncommonIconRef.RelativePath;
		yield return RareIconRef.RelativePath;
		yield return MythicIconRef.RelativePath;
		yield return CommonWhiteIconRef.RelativePath;
		yield return CommonHistoricIconRef.RelativePath;
	}
}
