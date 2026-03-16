using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class DFCFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IClientLocProvider _localizationManager;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public DFCFaceInfoGenerator(ICardDatabaseAdapter cardDatabase)
	{
		_cardDatabase = cardDatabase;
		_localizationManager = cardDatabase.ClientLocProvider;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if ((cardData.LinkedFaceType == LinkedFace.DfcFront || cardData.LinkedFaceType == LinkedFace.DfcBack) && sourceModel.Instance != null && sourceModel.Instance.OthersideGrpId == 0)
		{
			return _data;
		}
		string text = string.Empty;
		switch (cardData.ObjectType)
		{
		case GameObjectType.SplitLeft:
			if (cardData.LinkedFacePrintings.Count > 0)
			{
				text = "DuelScene/FaceHanger/Aftermath";
			}
			break;
		case GameObjectType.SplitRight:
			if (cardData.LinkedFacePrintings.Count > 0)
			{
				text = "DuelScene/FaceHanger/AftermathOf";
			}
			break;
		case GameObjectType.Card:
			text = GetHeaderForTransformVsFrontBack(cardData.LinkedFaceType, cardData.LinkedFaceGrpIds.Count, useTransformation: true);
			break;
		case GameObjectType.Token:
			text = GetHeaderForTransformVsFrontBack(cardData.LinkedFaceType, cardData.LinkedFaceGrpIds.Count, cardData.Printing.IsToken);
			break;
		case GameObjectType.RevealedCard:
			switch (cardData.LinkedFaceType)
			{
			case LinkedFace.SplitCard:
				if (cardData.LinkedFacePrintings.Count > 0)
				{
					text = ((cardData.LinkedFacePrintings[0].LinkedFaceGrpIds.IndexOf(cardData.GrpId) == 0) ? "DuelScene/FaceHanger/Aftermath" : "DuelScene/FaceHanger/AftermathOf");
				}
				break;
			case LinkedFace.DfcBack:
				if (cardData.LinkedFaceGrpIds.Count > 0)
				{
					text = "DuelScene/FaceHanger/DFC_Back_TransformsInto";
				}
				break;
			case LinkedFace.DfcFront:
				if (cardData.LinkedFaceGrpIds.Count > 0)
				{
					text = "DuelScene/FaceHanger/DFC_Front_TransformedFrom";
				}
				break;
			}
			break;
		}
		if (string.IsNullOrEmpty(text))
		{
			return _data;
		}
		ICardDataAdapter linkedFaceAtIndex = cardData.GetLinkedFaceAtIndex(0, ignoreInstance: false, _cardDatabase.CardDataProvider);
		if (linkedFaceAtIndex == null)
		{
			return _data;
		}
		if (sourceModel.HasPerpetualChanges())
		{
			PerpetualChangeUtilities.CopyPerpetualEffects(sourceModel, linkedFaceAtIndex, _cardDatabase.AbilityDataProvider);
		}
		string localizedText = _localizationManager.GetLocalizedText(text);
		_data.Add(new FaceHanger.FaceCardInfo(linkedFaceAtIndex, localizedText, new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.Directional), FaceHanger.HangerType.DFC));
		return _data;
	}

	private static string GetHeaderForTransformVsFrontBack(LinkedFace linkedFaceType, int numberLinkedFaces, bool useTransformation)
	{
		switch (linkedFaceType)
		{
		case LinkedFace.DfcBack:
			if (numberLinkedFaces > 0)
			{
				if (!useTransformation)
				{
					return "DuelScene/FaceHanger/BackSide";
				}
				return "DuelScene/FaceHanger/DFC_Back_TransformsInto";
			}
			break;
		case LinkedFace.DfcFront:
			if (numberLinkedFaces > 0)
			{
				if (!useTransformation)
				{
					return "DuelScene/FaceHanger/FrontSide";
				}
				return "DuelScene/FaceHanger/DFC_Front_TransformedFrom";
			}
			break;
		}
		return "";
	}
}
