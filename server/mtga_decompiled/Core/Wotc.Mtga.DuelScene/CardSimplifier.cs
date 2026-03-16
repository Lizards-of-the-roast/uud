using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public static class CardSimplifier
{
	public enum Context
	{
		ModelOverride,
		Examine
	}

	private const string HYBRID = "hybrid";

	private static readonly string[] HYBRID_FRAME_DETAILS = new string[1] { "hybrid" };

	public static ICardDataAdapter Simplify(Context context, ICardDataAdapter sourceModel, ICardDataProvider cardProvider, IAbilityDataProvider abilityProvider, bool keepArtId = false)
	{
		CardPrintingData printing = sourceModel.Printing;
		MtgCardInstance mtgCardInstance = sourceModel.Instance;
		if (printing == null)
		{
			return sourceModel;
		}
		if (mtgCardInstance == null)
		{
			mtgCardInstance = printing.CreateInstance();
		}
		MtgCardInstance mtgCardInstance2 = ((context == Context.Examine) ? mtgCardInstance.GetExamineCopy() : mtgCardInstance.GetCopy());
		mtgCardInstance2.IsForceSimplified = keepArtId;
		mtgCardInstance2.BaseSkinCode = mtgCardInstance2.SkinCode;
		mtgCardInstance2.SkinCode = string.Empty;
		mtgCardInstance2.ManaCostOverride = (IReadOnlyCollection<ManaQuantity>)(object)Array.Empty<ManaQuantity>();
		CardPrintingRecord cardRecordById = cardProvider.GetCardRecordById(mtgCardInstance2.BaseGrpId, mtgCardInstance2.SkinCode);
		cardRecordById = ClearRecord(cardRecordById, GetArtSize(printing));
		if (keepArtId)
		{
			CardPrintingRecord baseRecord = cardRecordById;
			uint? artId = printing.ArtId;
			string artistCredit = printing.ArtistCredit;
			cardRecordById = new CardPrintingRecord(baseRecord, null, artId, null, null, null, null, null, null, null, null, artistCredit);
		}
		List<CardPrintingData> list = new List<CardPrintingData>();
		for (int i = 0; i < (printing?.LinkedFacePrintings.Count ?? 0); i++)
		{
			CardPrintingData cardPrintingData = printing.LinkedFacePrintings[i];
			CardPrintingData item = new CardPrintingData(ClearRecord(cardPrintingData.Record, GetArtSize(cardPrintingData)), cardProvider, abilityProvider);
			list.Add(item);
		}
		CardPrintingData printing2 = new CardPrintingData(cardRecordById, cardProvider, abilityProvider, list);
		return new CardData(mtgCardInstance2, printing2)
		{
			RulesTextOverride = sourceModel.RulesTextOverride
		};
	}

	private static CardPrintingRecord ClearRecord(CardPrintingRecord baseRecord, CardArtSize cardArtSize)
	{
		IReadOnlyList<MetaDataTag> readOnlyList = baseRecord.Tags.Where((MetaDataTag x) => !CanRemoveTag(x)).ToArray();
		CardPrintingRecord baseRecord2 = baseRecord;
		string rawFrameDetail = (baseRecord.RawFrameDetail.Contains("hybrid") ? "hybrid" : string.Empty);
		IReadOnlyCollection<string> additionalFrameDetails = (IReadOnlyCollection<string>)(object)(baseRecord.AdditionalFrameDetails.Contains("hybrid") ? HYBRID_FRAME_DETAILS : Array.Empty<string>());
		IReadOnlyDictionary<ExtraFrameDetailType, string> extraFrameDetails = new Dictionary<ExtraFrameDetailType, string>();
		IReadOnlyList<MetaDataTag> tags = readOnlyList;
		CardArtSize? artSize = cardArtSize;
		return new CardPrintingRecord(baseRecord2, null, null, null, null, null, null, null, null, null, null, null, artSize, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, rawFrameDetail, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, additionalFrameDetails, extraFrameDetails, tags);
	}

	private static CardArtSize GetArtSize(CardPrintingData printing)
	{
		if (printing.IsToken)
		{
			return printing.ArtSize;
		}
		return CardArtSize.Normal;
	}

	private static bool CanRemoveTag(MetaDataTag tag)
	{
		string text = tag.ToString();
		if (!text.StartsWith("Card_"))
		{
			return text.StartsWith("Style_");
		}
		return true;
	}
}
