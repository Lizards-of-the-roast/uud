using System;
using Newtonsoft.Json;
using UnityEngine;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace AssetLookupTree.Payloads.Ability.Metadata;

public class BadgeEntryData : IBadgeEntryData, IComparable<IBadgeEntryData>, IEquatable<IBadgeEntryData>
{
	public readonly AltAssetReference<Sprite> SpriteRef = new AltAssetReference<Sprite>();

	public INumericBadgeCalculator NumberCalculator = new NullNumericBadgeCalculator();

	public IBadgeActivationCalculator ActivationCalculator = new NullActivationCalculator();

	public readonly AltAssetReference<Sprite> ActivatedSpriteRef = new AltAssetReference<Sprite>();

	public BadgeEntryCategory Category = BadgeEntryCategory.Primary;

	public int Priority;

	public DisplayValidity Display;

	public bool UseSpecialEntryStatusOnBattlefield { get; set; }

	[JsonIgnore]
	public bool ValidOnTTP => Display.HasFlag(DisplayValidity.TTP);

	[JsonIgnore]
	public bool ValidOnBattlefield => Display.HasFlag(DisplayValidity.Battlefield);

	[JsonIgnore]
	public bool ValidOnHanger => Display.HasFlag(DisplayValidity.Hanger);

	AltAssetReference<Sprite> IBadgeEntryData.SpriteRef => SpriteRef;

	INumericBadgeCalculator IBadgeEntryData.NumberCalculator => NumberCalculator;

	IBadgeActivationCalculator IBadgeEntryData.ActivationCalculator => ActivationCalculator;

	AltAssetReference<Sprite> IBadgeEntryData.ActivatedSpriteRef => ActivatedSpriteRef;

	BadgeEntryCategory IBadgeEntryData.Category => Category;

	int IBadgeEntryData.Priority => Priority;

	public static BadgeEntryData Empty => new BadgeEntryData();

	public string GetActivationWord()
	{
		if (ActivationCalculator is WordActivationCalculator wordActivationCalculator)
		{
			return wordActivationCalculator.ActivationWord;
		}
		if (NumberCalculator is ActivationWordAdditionalDetailCount activationWordAdditionalDetailCount)
		{
			return activationWordAdditionalDetailCount.ActivationWord;
		}
		return string.Empty;
	}

	public int CompareTo(IBadgeEntryData other)
	{
		int num = 0;
		num = Category.CompareTo(other.Category);
		if (num != 0)
		{
			return num;
		}
		num = Priority.CompareTo(other.Priority);
		if (num != 0)
		{
			return num;
		}
		num = string.Compare(GetType().Name, other.GetType().Name, StringComparison.Ordinal);
		if (num != 0)
		{
			return num;
		}
		num = string.CompareOrdinal(SpriteRef.RelativePath, other.SpriteRef.RelativePath);
		if (num != 0)
		{
			return num;
		}
		num = string.CompareOrdinal(NumberCalculator.GetType().Name, other.NumberCalculator.GetType().Name);
		if (num != 0)
		{
			return num;
		}
		num = UseSpecialEntryStatusOnBattlefield.CompareTo(other.UseSpecialEntryStatusOnBattlefield);
		if (num != 0)
		{
			return num;
		}
		return 0;
	}

	public bool Equals(IBadgeEntryData other)
	{
		if (string.Equals(SpriteRef.RelativePath, other.SpriteRef.RelativePath) && NumberCalculator.GetType() == other.NumberCalculator.GetType() && ActivationCalculator.GetType() == other.ActivationCalculator.GetType() && string.Equals(GetActivationWord(), other.GetActivationWord()) && Category == other.Category)
		{
			return UseSpecialEntryStatusOnBattlefield == other.UseSpecialEntryStatusOnBattlefield;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((((((SpriteRef.RelativePath.GetHashCode() * 397) ^ NumberCalculator.GetHashCode()) * 397) ^ ActivationCalculator.GetHashCode()) * 397) ^ GetActivationWord().GetHashCode()) * 397) ^ Category.GetHashCode()) * 397) ^ UseSpecialEntryStatusOnBattlefield.GetHashCode();
	}

	public bool ShouldSerializeActivatedSpriteRef()
	{
		if (!string.IsNullOrEmpty(ActivatedSpriteRef.Guid))
		{
			return !string.IsNullOrEmpty(ActivatedSpriteRef.RelativePath);
		}
		return false;
	}
}
