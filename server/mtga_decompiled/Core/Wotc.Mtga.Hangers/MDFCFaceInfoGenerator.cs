using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class MDFCFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IClientLocProvider _localizationManager;

	private readonly HashSet<FaceHanger.FaceCardInfo> _data = new HashSet<FaceHanger.FaceCardInfo>();

	public MDFCFaceInfoGenerator(ICardDatabaseAdapter cardDataProvider, IClientLocProvider clientLocManager)
	{
		_cardDatabase = cardDataProvider;
		_localizationManager = clientLocManager ?? NullLocProvider.Default;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if (cardData.ObjectType != GameObjectType.Card && cardData.ObjectType != GameObjectType.RevealedCard && (cardData.ObjectType != GameObjectType.Token || !cardData.Instance.IsCopy))
		{
			return _data;
		}
		string text = string.Empty;
		bool mirrored = false;
		switch (cardData.LinkedFaceType)
		{
		case LinkedFace.MdfcFront:
			if (cardData.LinkedFaceGrpIds.Count > 0)
			{
				text = "DuelScene/FaceHanger/FrontSide";
				mirrored = true;
			}
			break;
		case LinkedFace.MdfcBack:
			if (cardData.LinkedFaceGrpIds.Count > 0)
			{
				text = "DuelScene/FaceHanger/BackSide";
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
		if (sourceModel.Instance != null)
		{
			foreach (DesignationData designation in sourceModel.Instance.Designations)
			{
				Designation type = designation.Type;
				if (type == Designation.Commander || type == Designation.Companion)
				{
					linkedFaceAtIndex.Instance.Designations.Add(designation);
				}
			}
		}
		if (sourceModel.HasPerpetualChanges())
		{
			PerpetualChangeUtilities.CopyPerpetualEffects(sourceModel, linkedFaceAtIndex, _cardDatabase.AbilityDataProvider);
		}
		string localizedText = _localizationManager.GetLocalizedText(text);
		_data.Add(new FaceHanger.FaceCardInfo(linkedFaceAtIndex, localizedText, new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.FrontBack, mirrored), FaceHanger.HangerType.MDFC));
		return _data;
	}
}
