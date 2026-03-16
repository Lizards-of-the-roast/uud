using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

public class CopyFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly ICardDataProvider _cardDatabase;

	private readonly IClientLocProvider _localizationManager;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public CopyFaceInfoGenerator(ICardDataProvider cardDataProvider, IClientLocProvider locManager)
	{
		_cardDatabase = cardDataProvider ?? new NullCardDataProvider();
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
		if (!cardData.Instance.IsObjectCopy)
		{
			return _data;
		}
		string localizedText = _localizationManager.GetLocalizedText("DuelScene/FaceHanger/Original");
		CardPrintingData cardPrintingById = _cardDatabase.GetCardPrintingById(cardData.Instance.BaseGrpId);
		CardData cardData2 = new CardData(cardPrintingById.CreateInstance(), cardPrintingById);
		cardData2.Instance.SkinCode = ((!string.IsNullOrEmpty(sourceModel.Instance?.BaseSkinCode)) ? sourceModel.Instance.BaseSkinCode : sourceModel.SkinCode);
		cardData2.Instance.SleeveCode = sourceModel.SleeveCode;
		_data.Add(new FaceHanger.FaceCardInfo(cardData2, localizedText, new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None), FaceHanger.HangerType.CopyReference));
		return _data;
	}
}
