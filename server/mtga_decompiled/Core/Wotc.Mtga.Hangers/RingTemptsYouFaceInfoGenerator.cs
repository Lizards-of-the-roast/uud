using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class RingTemptsYouFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly ICardDataProvider _cardDataProvider;

	private readonly IClientLocProvider _locManager;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public RingTemptsYouFaceInfoGenerator(ICardDataProvider cardDataProvider, IClientLocProvider locManager)
	{
		_cardDataProvider = cardDataProvider ?? new NullCardDataProvider();
		_locManager = locManager ?? NullLocProvider.Default;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if (HasRingTemptsYou(cardData) || HasRingTemptsYou(sourceModel))
		{
			CardPrintingData cardPrintingById = _cardDataProvider.GetCardPrintingById(87496u);
			if (cardPrintingById != null)
			{
				_data.Add(RingFaceHanger(cardPrintingById));
			}
		}
		return _data;
	}

	private FaceHanger.FaceCardInfo RingFaceHanger(CardPrintingData printing)
	{
		CardData cardData = new CardData(printing.CreateInstance(), printing);
		string localizedText = _locManager.GetLocalizedText("DuelScene/FaceHanger/TheRing");
		FaceHanger.FaceHangerArrowData arrowData = new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None);
		return new FaceHanger.FaceCardInfo(cardData, localizedText, arrowData, FaceHanger.HangerType.RingTemptsYou);
	}

	private bool HasRingTemptsYou(ICardDataAdapter cardData)
	{
		if (cardData == null || cardData.ObjectSourceGrpId == 87496)
		{
			return false;
		}
		foreach (AbilityPrintingData ability in cardData.Abilities)
		{
			if (ability.Id == 169427)
			{
				return true;
			}
			foreach (AbilityType referencedAbilityType in ability.ReferencedAbilityTypes)
			{
				if (referencedAbilityType == AbilityType.TheRingTempts)
				{
					return true;
				}
			}
		}
		if (cardData.Printing == null)
		{
			return false;
		}
		foreach (AbilityPrintingData hiddenAbility in cardData.Printing.HiddenAbilities)
		{
			if (hiddenAbility.Id == 169427)
			{
				return true;
			}
			foreach (AbilityType referencedAbilityType2 in hiddenAbility.ReferencedAbilityTypes)
			{
				if (referencedAbilityType2 == AbilityType.TheRingTempts)
				{
					return true;
				}
			}
		}
		return false;
	}
}
