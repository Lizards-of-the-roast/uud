using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using ReferenceMap;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class TriggeredByFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly Func<MtgGameState> _getCurrentGameState;

	private readonly IClientLocProvider _clientLocManager;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private HashSet<MtgCardInstance> _triggeredInstances = new HashSet<MtgCardInstance>();

	private HashSet<FaceHanger.FaceCardInfo> _outputCache = new HashSet<FaceHanger.FaceCardInfo>();

	public TriggeredByFaceInfoGenerator(ICardDatabaseAdapter cardDatabase, IClientLocProvider clientLocManager, Func<MtgGameState> getCurrentGameState)
	{
		_cardDatabase = cardDatabase;
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_getCurrentGameState = getCurrentGameState;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		return GenerateFaceCardInfo(cardData, _getCurrentGameState());
	}

	private HashSet<MtgCardInstance> TriggeredInstances(ICardDataAdapter cardData, ref MtgGameState gameState)
	{
		_triggeredInstances.Clear();
		if (cardData == null)
		{
			return _triggeredInstances;
		}
		if (cardData.ObjectType != GameObjectType.Ability)
		{
			return _triggeredInstances;
		}
		if (gameState == null)
		{
			return _triggeredInstances;
		}
		Map referenceMap = gameState.ReferenceMap;
		if (referenceMap == null)
		{
			return _triggeredInstances;
		}
		HashSet<ReferenceMap.Reference> results = new HashSet<ReferenceMap.Reference>();
		referenceMap.GetTriggeredBy(cardData.InstanceId, ref results);
		if (results.Count == 0)
		{
			return _triggeredInstances;
		}
		foreach (ReferenceMap.Reference item in results)
		{
			if (item.Type != ReferenceMap.ReferenceType.Triggered)
			{
				continue;
			}
			MtgCardInstance cardById = gameState.GetCardById(item.A);
			if (cardById != null)
			{
				MtgZone zone = cardById.Zone;
				if (zone != null && zone.Type == ZoneType.Limbo)
				{
					_triggeredInstances.Add(cardById);
				}
			}
		}
		return _triggeredInstances;
	}

	private HashSet<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, MtgGameState gameState)
	{
		_outputCache.Clear();
		foreach (MtgCardInstance item in TriggeredInstances(cardData, ref gameState))
		{
			MtgCardInstance examineCopy = item.GetExamineCopy();
			CardData cardData2 = CardDataExtensions.CreateWithDatabase(examineCopy, _cardDatabase);
			cardData2.Instance.SkinCode = examineCopy.SkinCode;
			cardData2.Instance.SleeveCode = examineCopy.SleeveCode;
			uint sourceZoneIdOfTriggeringObject = gameState.ReferenceMap.GetSourceZoneIdOfTriggeringObject(item.InstanceId);
			if (sourceZoneIdOfTriggeringObject == 0)
			{
				sourceZoneIdOfTriggeringObject = gameState.ReferenceMap.GetSourceZoneIdOfTriggeringObject(cardData.InstanceId);
			}
			ZoneType zoneTypeById = gameState.GetZoneTypeById(sourceZoneIdOfTriggeringObject);
			_outputCache.Add(new FaceHanger.FaceCardInfo(cardData2, GetHeaderText(zoneTypeById), new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None)));
		}
		return _outputCache;
	}

	private string GetHeaderText(ZoneType sourceZoneType)
	{
		if (sourceZoneType != ZoneType.None)
		{
			string key = $"Enum/ZoneType/ZoneType_{sourceZoneType}";
			string localizedText = _clientLocManager.GetLocalizedText(key);
			return _clientLocManager.GetLocalizedText("DuelScene/FaceHanger/LeftZone", ("sourceZone", localizedText));
		}
		return _clientLocManager.GetLocalizedText("DuelScene/FaceHanger/LeftBattlfield");
	}
}
