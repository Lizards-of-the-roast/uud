using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class AdditionalCostFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly Func<MtgGameState> _getCurrentGameState;

	private readonly IClientLocProvider _clientLocManager;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private HashSet<MtgCardInstance> _additionalCostInstances = new HashSet<MtgCardInstance>();

	private HashSet<FaceHanger.FaceCardInfo> _outputCache = new HashSet<FaceHanger.FaceCardInfo>();

	public AdditionalCostFaceInfoGenerator(ICardDatabaseAdapter cardDatabase, IClientLocProvider clientLocManager, Func<MtgGameState> getCurrentGameState)
	{
		_cardDatabase = cardDatabase;
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_getCurrentGameState = getCurrentGameState;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		return GenerateFaceCardInfo(cardData, _getCurrentGameState());
	}

	private HashSet<MtgCardInstance> AdditionalCostInstances(ICardDataAdapter cardData, ref MtgGameState gameState)
	{
		_additionalCostInstances.Clear();
		if (cardData == null)
		{
			return _additionalCostInstances;
		}
		MtgZone zone = cardData.Zone;
		if (zone == null || zone.Type != ZoneType.Stack)
		{
			return _additionalCostInstances;
		}
		if (gameState == null)
		{
			return _additionalCostInstances;
		}
		if (!gameState.TryGetCard(cardData.InstanceId, out var card))
		{
			return _additionalCostInstances;
		}
		foreach (uint additionalCostId in card.AdditionalCostIds)
		{
			if (gameState.TryGetCard(additionalCostId, out var card2))
			{
				_additionalCostInstances.Add(card2);
			}
		}
		return _additionalCostInstances;
	}

	private HashSet<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, MtgGameState gameState)
	{
		_outputCache.Clear();
		foreach (MtgCardInstance item in AdditionalCostInstances(cardData, ref gameState))
		{
			MtgCardInstance examineCopy = item.GetExamineCopy();
			CardData cardData2 = CardDataExtensions.CreateWithDatabase(examineCopy, _cardDatabase);
			cardData2.Instance.SkinCode = examineCopy.SkinCode;
			cardData2.Instance.SleeveCode = examineCopy.SleeveCode;
			_outputCache.Add(new FaceHanger.FaceCardInfo(cardData2, GetLocalizedHeaderText("DuelScene/FaceHanger/AdditionalCost"), new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None)));
		}
		return _outputCache;
	}

	private string GetLocalizedHeaderText(string key)
	{
		return _clientLocManager.GetLocalizedText(key);
	}
}
