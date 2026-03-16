using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SuspendLikeController : ISuspendLikeController, IDisposable
{
	private const string SUSPEND_LIKE_KEY = "SuspendLikeEffect_#{0}";

	private const uint ABILITY_ID_ALAUNDO = 151476u;

	private const uint ABILITY_ID_DARIGAAZ_CHAMP_EGG = 164737u;

	private static HashSet<uint> _specialCounterAbilities = new HashSet<uint> { 118883u, 139884u, 142072u, 151476u, 164737u, 63u };

	private static HashSet<uint> _specialCounterBaseIds = new HashSet<uint> { 63u };

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IGameEffectBuilder _gameEffectBuilder;

	private readonly Dictionary<uint, SuspendLikeData> _suspendLikeData = new Dictionary<uint, SuspendLikeData>();

	private readonly Dictionary<uint, DuelScene_CDC> _suspendLikeMiniCDCs = new Dictionary<uint, DuelScene_CDC>();

	private MtgGameState GameState => _gameStateProvider.CurrentGameState;

	public SuspendLikeController(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IGameEffectBuilder gameEffectBuilder)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_gameEffectBuilder = gameEffectBuilder ?? NullGameEffectBuilder.Default;
	}

	public void AddSuspendLikeData(SuspendLikeData data)
	{
		_suspendLikeData[data.Id] = data;
		if (ShouldCreateMiniCDC(data, GameState))
		{
			CreateMiniCDC(data);
		}
	}

	public void UpdateSuspendLikeData(SuspendLikeData data)
	{
		DuelScene_CDC value2;
		if (ShouldCreateMiniCDC(data, GameState))
		{
			if (_suspendLikeMiniCDCs.TryGetValue(data.Id, out var value))
			{
				value.SetModel(GenerateMiniCDCModel(data, GameState, _cardDatabase));
			}
			else
			{
				CreateMiniCDC(data);
			}
		}
		else if (_suspendLikeMiniCDCs.TryGetValue(data.Id, out value2))
		{
			DestroyMiniCDC(data.Id);
		}
	}

	public void RemoveSuspendLikeData(uint id)
	{
		if (_suspendLikeData.Remove(id))
		{
			DestroyMiniCDC(id);
		}
	}

	private void CreateMiniCDC(SuspendLikeData suspendLikeData)
	{
		DuelScene_CDC duelScene_CDC = _gameEffectBuilder.Create(GameEffectType.SuspendEffect, $"SuspendLikeEffect_#{suspendLikeData.Id}", GenerateMiniCDCModel(suspendLikeData, GameState, _cardDatabase));
		if (duelScene_CDC != null)
		{
			_suspendLikeMiniCDCs[suspendLikeData.Id] = duelScene_CDC;
		}
	}

	private void DestroyMiniCDC(uint id)
	{
		if (_suspendLikeMiniCDCs.Remove(id))
		{
			_gameEffectBuilder.Destroy($"SuspendLikeEffect_#{id}");
		}
	}

	public void Dispose()
	{
		foreach (KeyValuePair<uint, SuspendLikeData> suspendLikeDatum in _suspendLikeData)
		{
			DestroyMiniCDC(suspendLikeDatum.Key);
		}
		_suspendLikeData.Clear();
	}

	private static bool ShouldCreateMiniCDC(SuspendLikeData data, MtgGameState gameState)
	{
		if (gameState == null)
		{
			return false;
		}
		if (gameState.TryGetCard(data.AffectedId, out var card) && gameState.TryGetCard(data.AffectorId, out var card2))
		{
			if (data.AbilityId == 151476 && card2.CounterDatas.Count == 0)
			{
				return false;
			}
			return card.InstanceId == card2.InstanceId;
		}
		return false;
	}

	public static ICardDataAdapter GenerateMiniCDCModel(SuspendLikeData suspendLikeData, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		MtgCardInstance cardById = gameState.GetCardById(suspendLikeData.AffectorId);
		MtgPlayer controller = cardById.Controller;
		AbilityPrintingData abilityPrintingById = cardDatabase.AbilityDataProvider.GetAbilityPrintingById(suspendLikeData.AbilityId);
		MtgCardInstance mtgCardInstance = ((cardById.Parent != null) ? cardById.Parent : cardById);
		CardPrintingData cardPrintingData = cardDatabase.CardDataProvider.GetCardPrintingById(GrpIdInternal(mtgCardInstance), mtgCardInstance.SkinCode) ?? CardPrintingData.Blank;
		CardPrintingData cardPrintingData2 = cardPrintingData.CreateMiniCDCPrintingData(suspendLikeData.AbilityId);
		MtgCardInstance mtgCardInstance2 = cardPrintingData2.CreateInstance(GameObjectType.Ability);
		mtgCardInstance2.Owner = controller;
		mtgCardInstance2.Abilities = new List<AbilityPrintingData>(cardPrintingData2.Abilities);
		mtgCardInstance2.Zone = new MtgZone
		{
			Type = ZoneType.Command,
			Visibility = Visibility.Public,
			Owner = controller
		};
		mtgCardInstance2.Visibility = Visibility.Public;
		mtgCardInstance2.Viewers.Add(GREPlayerNum.LocalPlayer);
		mtgCardInstance2.Viewers.Add(GREPlayerNum.Opponent);
		mtgCardInstance2.ParentId = cardById.InstanceId;
		mtgCardInstance2.Parent = cardById;
		mtgCardInstance2.GrpId = abilityPrintingById.Id;
		mtgCardInstance2.ObjectSourceGrpId = cardPrintingData2.GrpId;
		mtgCardInstance2.TitleId = cardPrintingData2.TitleId;
		mtgCardInstance2.Controller = controller;
		mtgCardInstance2.LinkedInfoTitleLocIds.UnionWith(cardById.LinkedInfoTitleLocIds);
		if (_specialCounterAbilities.Contains(suspendLikeData.AbilityId) || _specialCounterBaseIds.Contains(abilityPrintingById.BaseId))
		{
			mtgCardInstance2.Counters.Clear();
			mtgCardInstance2.Counters = new Dictionary<CounterType, int>(cardById.Counters);
		}
		return new CardData(mtgCardInstance2, cardPrintingData2)
		{
			RulesTextOverride = new AbilityTextOverride(cardDatabase, cardPrintingData.TitleId, removeLoyaltyPrefix: true).AddAbility(abilityPrintingById.Id).AddSource(cardById).AddSource(cardPrintingData2)
		};
	}

	private static uint GrpIdInternal(MtgCardInstance card)
	{
		if (card == null)
		{
			return 0u;
		}
		if (card.ObjectType == GameObjectType.Emblem || card.ObjectType == GameObjectType.Boon)
		{
			return card.ObjectSourceGrpId;
		}
		return card.GrpId;
	}
}
