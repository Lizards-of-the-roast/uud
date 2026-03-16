using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using ReferenceMap;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class LimboParentFaceInfoGenerator : IFaceInfoGenerator
{
	private readonly Func<MtgGameState> _getCurrentGameState;

	private readonly IClientLocProvider _clientLocManager;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly HashSet<FaceHanger.FaceCardInfo> _outputCache = new HashSet<FaceHanger.FaceCardInfo>();

	public LimboParentFaceInfoGenerator(ICardDatabaseAdapter cardDatabase, IClientLocProvider clientLocManager, Func<MtgGameState> getCurrentGameState)
	{
		_cardDatabase = cardDatabase;
		_clientLocManager = clientLocManager ?? NullLocProvider.Default;
		_getCurrentGameState = getCurrentGameState;
	}

	public IReadOnlyCollection<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, ICardDataAdapter sourceModel)
	{
		return GenerateFaceCardInfo(cardData, _getCurrentGameState());
	}

	private MtgCardInstance LimboParent(ICardDataAdapter cardData, ref MtgGameState gameState)
	{
		if (cardData == null)
		{
			return null;
		}
		if (gameState == null)
		{
			return null;
		}
		MtgCardInstance instance = cardData.Instance;
		if (instance == null)
		{
			return null;
		}
		if (instance.ObjectType != GameObjectType.Ability)
		{
			return null;
		}
		uint parentId = instance.ParentId;
		if (parentId == 0)
		{
			return null;
		}
		MtgCardInstance cardById = gameState.GetCardById(parentId);
		if (cardById == null)
		{
			return null;
		}
		MtgZone zone = cardById.Zone;
		if (zone == null)
		{
			return null;
		}
		if (zone.Type != ZoneType.Limbo)
		{
			return null;
		}
		if (cardById.CatalogId == WellKnownCatalogId.WellKnownCatalogId_TransientEffect)
		{
			return null;
		}
		return cardById;
	}

	private HashSet<FaceHanger.FaceCardInfo> GenerateFaceCardInfo(ICardDataAdapter cardData, MtgGameState gameState)
	{
		_outputCache.Clear();
		MtgCardInstance mtgCardInstance = LimboParent(cardData, ref gameState);
		if (mtgCardInstance != null)
		{
			MtgCardInstance examineCopy = mtgCardInstance.GetExamineCopy();
			if (examineCopy != null)
			{
				CardData cardData2 = CardDataExtensions.CreateWithDatabase(examineCopy, _cardDatabase);
				if (cardData2 == null)
				{
					return _outputCache;
				}
				cardData2.Instance.SkinCode = examineCopy.SkinCode;
				cardData2.Instance.SleeveCode = examineCopy.SleeveCode;
				uint sourceZoneIdOfAbilityInstanceParent = gameState.ReferenceMap.GetSourceZoneIdOfAbilityInstanceParent(mtgCardInstance.InstanceId);
				ZoneType zoneTypeById = gameState.GetZoneTypeById(sourceZoneIdOfAbilityInstanceParent);
				if (zoneTypeById == ZoneType.Limbo)
				{
					sourceZoneIdOfAbilityInstanceParent = gameState.ReferenceMap.GetSourceZoneIdOfTriggeringObject(cardData.InstanceId);
					zoneTypeById = gameState.GetZoneTypeById(sourceZoneIdOfAbilityInstanceParent);
					if (zoneTypeById == ZoneType.Limbo)
					{
						return _outputCache;
					}
				}
				_outputCache.Add(new FaceHanger.FaceCardInfo(cardData2, GetHeaderText(zoneTypeById), new FaceHanger.FaceHangerArrowData(FaceHanger.FaceHangerArrowType.None)));
			}
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
