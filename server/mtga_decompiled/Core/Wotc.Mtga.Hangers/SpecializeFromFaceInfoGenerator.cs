using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class SpecializeFromFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly IClientLocProvider _localizationManager;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public SpecializeFromFaceInfoGenerator(IClientLocProvider locManager)
	{
		_localizationManager = locManager ?? NullLocProvider.Default;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if (CanGenerate(cardData))
		{
			CardPrintingData cardPrintingData = cardData.Printing.LinkedFacePrintings[0];
			CardData cardData2 = new CardData(cardPrintingData.CreateInstance(), cardPrintingData);
			cardData2.Instance.SkinCode = sourceModel.SkinCode;
			cardData2.Instance.SleeveCode = sourceModel.SleeveCode;
			string localizedText = _localizationManager.GetLocalizedText("DuelScene/FaceHanger/Specialized");
			_data.Add(new FaceHanger.FaceCardInfo(cardData2, localizedText, new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None), FaceHanger.HangerType.Specialized));
		}
		return _data;
	}

	private bool CanGenerate(ICardDataAdapter cardData)
	{
		if (cardData == null)
		{
			return false;
		}
		MtgCardInstance instance = cardData.Instance;
		if (instance == null)
		{
			return false;
		}
		if (instance.IsObjectCopy)
		{
			return false;
		}
		if (instance.ObjectType == GameObjectType.Ability)
		{
			return false;
		}
		CardPrintingData printing = cardData.Printing;
		if (printing == null)
		{
			return false;
		}
		if (printing.LinkedFaceType != LinkedFace.SpecializeParent)
		{
			return false;
		}
		if (printing.LinkedFacePrintings == null)
		{
			return false;
		}
		if (printing.LinkedFacePrintings.Count == 0)
		{
			return false;
		}
		return true;
	}
}
