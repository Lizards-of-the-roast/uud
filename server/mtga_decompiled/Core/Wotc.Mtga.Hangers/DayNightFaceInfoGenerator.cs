using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

internal class DayNightFaceInfoGenerator : IFaceInfoGenerator
{
	private const uint GRPID_DAY_TOKEN = 79412u;

	private const uint GRPID_NIGHT_TOKEN = 79413u;

	private HashSet<FaceHanger.FaceCardInfo> _dayFirstHangers;

	private HashSet<FaceHanger.FaceCardInfo> _nightFirstHangers;

	private static readonly HashSet<uint> DAY_FIRST_ABILITY_IDS = new HashSet<uint> { 216u, 217u, 145156u, 148583u };

	private static readonly HashSet<uint> NIGHT_FIRST_ABILITY_IDS = new HashSet<uint> { 145443u, 146742u };

	private readonly ICardDataProvider _cardDataProvider;

	private readonly IClientLocProvider _locManager;

	public DayNightFaceInfoGenerator(ICardDataProvider cardDataProvider, IClientLocProvider locManager)
	{
		_cardDataProvider = cardDataProvider ?? new NullCardDataProvider();
		_locManager = locManager ?? NullLocProvider.Default;
		FaceHanger.FaceCardInfo item = DayNightFaceHanger(_cardDataProvider.GetCardPrintingById(79412u) ?? CardPrintingData.Blank);
		FaceHanger.FaceCardInfo item2 = DayNightFaceHanger(_cardDataProvider.GetCardPrintingById(79413u) ?? CardPrintingData.Blank);
		_dayFirstHangers = new HashSet<FaceHanger.FaceCardInfo> { item, item2 };
		_nightFirstHangers = new HashSet<FaceHanger.FaceCardInfo> { item2, item };
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		if (IsDayTokenFirst(cardData))
		{
			return _dayFirstHangers;
		}
		if (IsNightTokenFirst(cardData))
		{
			return _nightFirstHangers;
		}
		return (IReadOnlyCollection<FaceHanger.FaceCardInfo>)(object)Array.Empty<FaceHanger.FaceCardInfo>();
	}

	private FaceHanger.FaceCardInfo DayNightFaceHanger(CardPrintingData printing)
	{
		CardData cardData = new CardData(printing.CreateInstance(), printing);
		string localizedText = _locManager.GetLocalizedText("DuelScene/FaceHanger/Related");
		FaceHanger.FaceHangerArrowData arrowData = new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None);
		return new FaceHanger.FaceCardInfo(cardData, localizedText, arrowData, FaceHanger.HangerType.RelatedObj_DayNight);
	}

	private bool IsDayTokenFirst(ICardDataAdapter cardData)
	{
		return DAY_FIRST_ABILITY_IDS.Overlaps(cardData.AbilityIds);
	}

	private bool IsNightTokenFirst(ICardDataAdapter cardData)
	{
		return NIGHT_FIRST_ABILITY_IDS.Overlaps(cardData.AbilityIds);
	}
}
