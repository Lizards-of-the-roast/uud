using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class ConjureFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private IClientLocProvider _locManager;

	private readonly List<FaceHanger.FaceCardInfo> _data = new List<FaceHanger.FaceCardInfo>();

	public ConjureFaceInfoGenerator(ICardDatabaseAdapter cardDatabase, IClientLocProvider locManager)
	{
		_cardDatabase = cardDatabase;
		_locManager = locManager;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if (sourceModel.Printing == null)
		{
			return _data;
		}
		AddConjurationsFromPrinting(sourceModel.Printing, sourceModel.SleeveCode, sourceModel.SkinCode);
		foreach (CardPrintingData linkedFacePrinting in sourceModel.LinkedFacePrintings)
		{
			AddConjurationsFromPrinting(linkedFacePrinting, sourceModel.SleeveCode, sourceModel.SkinCode);
		}
		SortHangers(sourceModel);
		return _data;
	}

	private void SortHangers(ICardDataAdapter sourceModel)
	{
		for (int num = _data.Count - 1; num >= 0; num--)
		{
			if (_data[num].CardData.TitleId == sourceModel.TitleId)
			{
				FaceHanger.FaceCardInfo item = _data[num];
				_data.RemoveAt(num);
				_data.Add(item);
			}
		}
		if (sourceModel.TitleId == 755648)
		{
			_data.Sort((FaceHanger.FaceCardInfo x, FaceHanger.FaceCardInfo y) => (int)(x.CardData.ConvertedManaCost - y.CardData.ConvertedManaCost));
		}
	}

	private void AddConjurationsFromPrinting(CardPrintingData cardPrintingData, string sleeve, string skin)
	{
		foreach (AbilityPrintingData ability in cardPrintingData.Abilities)
		{
			AddAnyConjurationsThatThisAbilityCreates(cardPrintingData.AbilityIdToLinkedConjurations, ability.Id, sleeve, skin);
		}
		foreach (AbilityPrintingData hiddenAbility in cardPrintingData.HiddenAbilities)
		{
			AddAnyConjurationsThatThisAbilityCreates(cardPrintingData.AbilityIdToLinkedConjurations, hiddenAbility.Id, sleeve, skin);
		}
	}

	private void AddAnyConjurationsThatThisAbilityCreates(IReadOnlyDictionary<uint, IReadOnlyList<LinkedConjuration>> abilityIdToLinkedConjurations, uint abilityId, string sleeve, string skin)
	{
		if (!abilityIdToLinkedConjurations.TryGetValue(abilityId, out var value) || value == null)
		{
			return;
		}
		foreach (LinkedConjuration item in value)
		{
			if (!IsDuplicateEntry(item))
			{
				CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(item.CardGrpId, skin);
				CardData cardData = new CardData(cardPrintingById.CreateInstance(), cardPrintingById);
				cardData.Instance.SleeveCode = sleeve;
				cardData.Instance.SkinCode = skin;
				_data.Add(new FaceHanger.FaceCardInfo(cardData, (item.ConjurationType == ConjurationType.NamedCard) ? _locManager.GetLocalizedText("DuelScene/FaceHanger/Conjure") : _locManager.GetLocalizedText("DuelScene/FaceHanger/Spellbook"), new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None), FaceHanger.HangerType.ConjureReference));
			}
		}
	}

	private bool IsDuplicateEntry(LinkedConjuration conjuration)
	{
		return _data.Exists(conjuration.CardGrpId, (FaceHanger.FaceCardInfo faceCardInfo, uint grpId) => faceCardInfo.CardData.GrpId == grpId);
	}
}
