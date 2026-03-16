using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class FacedownInfoGenerator : IFaceInfoGenerator
{
	private readonly ICardDataProvider _cardProvider;

	private readonly IClientLocProvider _localizationManager;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public FacedownInfoGenerator(ICardDataProvider cardDataProvider, IClientLocProvider locManager)
	{
		_cardProvider = cardDataProvider ?? new NullCardDataProvider();
		_localizationManager = locManager ?? NullLocProvider.Default;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if (cardData.Zone == null)
		{
			return _data;
		}
		if (cardData.Instance == null)
		{
			return _data;
		}
		if (!cardData.Instance.FaceDownState.IsFaceDown)
		{
			return _data;
		}
		if (cardData.IsDisplayedFaceDown)
		{
			return _data;
		}
		if (cardData.Instance.ObjectType == GameObjectType.Ability)
		{
			return _data;
		}
		if ((ulong)cardData.Instance.GrpId != (ulong)cardData.Instance.CatalogId && cardData.Instance.Visibility == Visibility.Private && cardData.Printing.TitleId != 0 && cardData.ZoneType != ZoneType.Battlefield)
		{
			return _data;
		}
		if (cardData.Instance.BaseGrpId == 3)
		{
			return _data;
		}
		string localizedText = _localizationManager.GetLocalizedText("DuelScene/FaceHanger/FaceUp");
		CardPrintingData cardPrintingById = _cardProvider.GetCardPrintingById((cardData.Instance.CopyObjectGrpId != 0) ? cardData.Instance.CopyObjectGrpId : cardData.Instance.BaseGrpId, sourceModel.SkinCode);
		CardData cardData2 = new CardData(cardPrintingById.CreateInstance(), cardPrintingById);
		cardData2.Instance.SkinCode = sourceModel.SkinCode;
		cardData2.Instance.SleeveCode = sourceModel.SleeveCode;
		_data.Add(new FaceHanger.FaceCardInfo(cardData2, localizedText, new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None), FaceHanger.HangerType.MorphReference));
		return _data;
	}
}
