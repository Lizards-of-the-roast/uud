using AssetLookupTree.Payloads.Ability.Metadata;
using GreClient.CardData;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;
using Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace Wotc.Mtga.DuelScene.CardView;

public struct BadgeEntrySpawnData
{
	public IBadgeEntryData BadgeEntryData;

	public BadgeEntryStatus BadgeEntryStatus;

	public AbilityPrintingData AbilityPrintingData;

	public BadgeEntryViewCreator BadgeEntryViewCreator;

	public BadgeEntrySpawnData(AbilityBadgeData abilityBadgeData)
	{
		AbilityPrintingData = abilityBadgeData.Ability;
		BadgeEntryStatus = BadgeEntryStatus.Special;
		BadgeEntryViewCreator = new BadgeEntryViewCreator(abilityBadgeData.BadgePrefabPath);
		BadgeEntryData obj = new BadgeEntryData
		{
			Category = BadgeEntryCategory.Special,
			Display = DisplayValidity.TTP,
			Priority = -999
		};
		INumericBadgeCalculator numberCalculator;
		if (string.IsNullOrWhiteSpace(abilityBadgeData.ActivationWord) || !int.TryParse(abilityBadgeData.ActivationWordCount, out var result))
		{
			INumericBadgeCalculator numericBadgeCalculator = new NullNumericBadgeCalculator();
			numberCalculator = numericBadgeCalculator;
		}
		else
		{
			INumericBadgeCalculator numericBadgeCalculator = new ConstCalculator(result);
			numberCalculator = numericBadgeCalculator;
		}
		obj.NumberCalculator = numberCalculator;
		IBadgeActivationCalculator activationCalculator;
		if (string.IsNullOrWhiteSpace(abilityBadgeData.ActivationWord) && !abilityBadgeData.ForceHighlight)
		{
			IBadgeActivationCalculator badgeActivationCalculator = new NullActivationCalculator();
			activationCalculator = badgeActivationCalculator;
		}
		else
		{
			IBadgeActivationCalculator badgeActivationCalculator = new ConstActivationCalculator();
			activationCalculator = badgeActivationCalculator;
		}
		obj.ActivationCalculator = activationCalculator;
		BadgeEntryData = obj;
		BadgeEntryData.SpriteRef.RelativePath = abilityBadgeData.IconSpritePath;
	}

	public BadgeEntrySpawnData(IBadgeEntryData badgeEntryData, BadgeEntryStatus badgeEntryStatus, AbilityPrintingData abilityPrintingData, BadgeEntryViewCreator badgeEntryView)
	{
		BadgeEntryData = badgeEntryData;
		BadgeEntryStatus = badgeEntryStatus;
		AbilityPrintingData = abilityPrintingData;
		BadgeEntryViewCreator = badgeEntryView;
	}
}
