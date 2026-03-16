using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class SpecializesToFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IClientLocProvider _locManager;

	private readonly List<FaceHanger.FaceCardInfo> _data = new List<FaceHanger.FaceCardInfo>();

	public SpecializesToFaceInfoGenerator(ICardDatabaseAdapter cardDatabase, IClientLocProvider locManager)
	{
		_cardDatabase = cardDatabase;
		_locManager = locManager;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		_data.Clear();
		if (cardData.Printing == null)
		{
			return _data;
		}
		if (cardData.Printing.LinkedFaceType != LinkedFace.SpecializeParent)
		{
			return _data;
		}
		if (cardData.ObjectType != GameObjectType.Ability)
		{
			return _data;
		}
		if (cardData.ZoneType != ZoneType.Library)
		{
			return _data;
		}
		CardData cardData2 = CardDataExtensions.CreateWithDatabase(_cardDatabase.CardDataProvider.GetCardPrintingById(cardData.Printing.GrpId).CreateInstance(), _cardDatabase);
		_data.Add(new FaceHanger.FaceCardInfo(cardData2, _locManager.GetLocalizedText("AbilityHanger/Keyword/Specialize_Title"), new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None), FaceHanger.HangerType.Specialized));
		return _data;
	}
}
