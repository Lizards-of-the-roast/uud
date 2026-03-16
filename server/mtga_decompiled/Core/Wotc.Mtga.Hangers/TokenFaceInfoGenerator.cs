using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class TokenFaceInfoGenerator : IFaceInfoGenerator
{
	private IComparer<FaceHanger.FaceCardInfo> _defaultComparer;

	private IComparer<FaceHanger.FaceCardInfo> _powerComparer = new TokenPowerComparer();

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IClientLocProvider _localizationManager;

	private readonly List<FaceHanger.FaceCardInfo> _data = new List<FaceHanger.FaceCardInfo>();

	private const int REEF_WORM_TITLEID = 700;

	public TokenFaceInfoGenerator(ICardDatabaseAdapter cardDataProvider, IClientLocProvider locManager, AssetLookupSystem assetLookupSystem)
	{
		_cardDatabase = cardDataProvider;
		_localizationManager = locManager ?? NullLocProvider.Default;
		_defaultComparer = new PrintedTokenComparer(assetLookupSystem);
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if (sourceModel.Printing == null)
		{
			return _data;
		}
		if (cardData.Instance != null && cardData.Instance.IsObjectCopy)
		{
			return _data;
		}
		if (cardData.TitleId == 877511)
		{
			return _data;
		}
		AddTokensFromPrinting(sourceModel.Printing, sourceModel.SleeveCode, sourceModel.SkinCode);
		AddLinkedFaceTokens(sourceModel);
		if (sourceModel.Instance != null)
		{
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(sourceModel.Instance.BaseGrpId, sourceModel.Instance.SkinCode);
			if (cardPrintingById != null)
			{
				AddTokensFromPrinting(cardPrintingById, sourceModel.SleeveCode, sourceModel.SkinCode);
			}
			foreach (MtgCardInstance mutationChild in sourceModel.Instance.MutationChildren)
			{
				CardPrintingData cardPrintingById2 = _cardDatabase.CardDataProvider.GetCardPrintingById(mutationChild.GrpId, mutationChild.SkinCode);
				if (cardPrintingById2 != null)
				{
					AddTokensFromPrinting(cardPrintingById2, sourceModel.SleeveCode, sourceModel.SkinCode);
				}
			}
		}
		IComparer<FaceHanger.FaceCardInfo> comparer = ((sourceModel.TitleId == 700) ? _powerComparer : _defaultComparer);
		_data.Sort(comparer);
		return _data;
	}

	private void AddLinkedFaceTokens(ICardDataAdapter sourceModel)
	{
		if (!HangerUtilities.ShowLinkedFaceFaceHangers(sourceModel.LinkedFaceType))
		{
			return;
		}
		foreach (CardPrintingData linkedFacePrinting in sourceModel.LinkedFacePrintings)
		{
			AddTokensFromPrinting(linkedFacePrinting, sourceModel.SleeveCode, sourceModel.SkinCode);
		}
	}

	private void AddTokensFromPrinting(CardPrintingData cardPrintingData, string sleeve, string skin)
	{
		foreach (AbilityPrintingData ability in cardPrintingData.Abilities)
		{
			AddAnyTokenThatThisAbilityCreates(cardPrintingData.AbilityIdToLinkedTokenPrinting, ability.Id, sleeve, skin);
		}
		foreach (AbilityPrintingData hiddenAbility in cardPrintingData.HiddenAbilities)
		{
			AddAnyTokenThatThisAbilityCreates(cardPrintingData.AbilityIdToLinkedTokenPrinting, hiddenAbility.Id, sleeve, skin);
		}
	}

	private void AddAnyTokenThatThisAbilityCreates(IReadOnlyDictionary<uint, IReadOnlyList<CardPrintingData>> abilityIdToLinkedTokenPrinting, uint abilityId, string sleeve, string skin)
	{
		if (abilityIdToLinkedTokenPrinting.TryGetValue(abilityId, out var value) && value != null)
		{
			AddFaceHangerFromPrintingList(value, sleeve, skin);
		}
	}

	private void AddFaceHangerFromPrintingList(IReadOnlyList<CardPrintingData> cardPrintingList, string sleeve, string skin)
	{
		foreach (CardPrintingData cardPrinting in cardPrintingList)
		{
			if (!IsADuplicateEntry(cardPrinting))
			{
				AddTokenFaceCardData(cardPrinting, sleeve, skin);
				AddFaceHangerFromPrintingList(cardPrinting.LinkedFacePrintings, sleeve, skin);
			}
		}
	}

	private bool IsADuplicateEntry(CardPrintingData printing)
	{
		foreach (FaceHanger.FaceCardInfo datum in _data)
		{
			if (datum.CardData.GrpId == printing.GrpId)
			{
				return true;
			}
		}
		return false;
	}

	private void AddTokenFaceCardData(CardPrintingData cardPrinting, string sleeve, string skin)
	{
		string localizedText = _localizationManager.GetLocalizedText("DuelScene/FaceHanger/Token");
		CardData cardData = CardDataExtensions.CreateWithDatabase(cardPrinting.CreateInstance(GameObjectType.Token), _cardDatabase);
		cardData.Instance.SleeveCode = sleeve;
		cardData.Instance.SkinCode = skin;
		FaceHanger.HangerType hangerType = FaceHanger.HangerType.TokenReference;
		if (cardData.Instance.Subtypes.Contains(SubType.Role))
		{
			hangerType = FaceHanger.HangerType.RoleReference;
		}
		_data.Add(new FaceHanger.FaceCardInfo(cardData, localizedText, new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None), hangerType));
	}
}
