using AssetLookupTree;
using GreClient.CardData;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;

namespace Core.Meta.Cards;

public static class EmergencyCardBanUtils
{
	public static HangerConfig HangerData(AssetLookupSystem assetLookupSystem, IClientLocProvider locProvider, CardPrintingData printing)
	{
		string iconSpritePath = AbilityBadgeUtil.GetBadgeDataForCondition(assetLookupSystem, ConditionType.EmergencyTempBanned, printing.ConvertToCardModel()).IconSpritePath;
		return new HangerConfig(locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Banned_Header"), locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/TempBanned_Body"), null, iconSpritePath);
	}
}
