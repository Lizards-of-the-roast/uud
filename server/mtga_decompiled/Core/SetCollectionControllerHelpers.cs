using System;
using Wotc.Mtgo.Gre.External.Messaging;

public static class SetCollectionControllerHelpers
{
	public static CardRarity AsRarity(this SetCollectionController.Metrics metric)
	{
		return metric switch
		{
			SetCollectionController.Metrics.Common => CardRarity.Common, 
			SetCollectionController.Metrics.Uncommon => CardRarity.Uncommon, 
			SetCollectionController.Metrics.Rare => CardRarity.Rare, 
			SetCollectionController.Metrics.MythicRare => CardRarity.MythicRare, 
			_ => throw new ArgumentOutOfRangeException("metric", $"Given metric is not a rarity: {metric}"), 
		};
	}

	public static CardColor AsCardColor(this SetCollectionController.Metrics metric)
	{
		return metric switch
		{
			SetCollectionController.Metrics.White => CardColor.White, 
			SetCollectionController.Metrics.Blue => CardColor.Blue, 
			SetCollectionController.Metrics.Black => CardColor.Black, 
			SetCollectionController.Metrics.Red => CardColor.Red, 
			SetCollectionController.Metrics.Green => CardColor.Green, 
			SetCollectionController.Metrics.Colorless => CardColor.Colorless, 
			_ => throw new ArgumentOutOfRangeException("metric", $"Given metric is not a color: {metric}"), 
		};
	}

	public static string AsDigitalReleaseCode(this string mapping)
	{
		return mapping.Replace('_', '-');
	}

	public static bool IsRarityMetric(this SetCollectionController.Metrics metric)
	{
		if (metric != SetCollectionController.Metrics.Common && metric != SetCollectionController.Metrics.Uncommon && metric != SetCollectionController.Metrics.Rare)
		{
			return metric == SetCollectionController.Metrics.MythicRare;
		}
		return true;
	}
}
