using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class LimboActionEffectController : IActionEffectController, IDisposable
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IGameEffectBuilder _gameEffectBuilder;

	private readonly Dictionary<uint, uint> _abilityIdToAbilityCount = new Dictionary<uint, uint>();

	private readonly Dictionary<uint, DuelScene_CDC> _abilityIdToMiniCDCs = new Dictionary<uint, DuelScene_CDC>();

	private const string GAME_EFFECT_KEY_FORMAT = "Action_Effect_{0}";

	public LimboActionEffectController(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IGameEffectBuilder gameEffectBuilder)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_gameEffectBuilder = gameEffectBuilder ?? NullGameEffectBuilder.Default;
	}

	public void Dispose()
	{
		foreach (KeyValuePair<uint, DuelScene_CDC> abilityIdToMiniCDC in _abilityIdToMiniCDCs)
		{
			_gameEffectBuilder.Destroy(GameEffectKey(abilityIdToMiniCDC.Key));
		}
		_abilityIdToMiniCDCs.Clear();
		_abilityIdToAbilityCount.Clear();
	}

	public bool AddActionEffect(ActionInfo actionInfo)
	{
		if (!actionInfo.IsActionType(ActionType.SpecialPayment))
		{
			return false;
		}
		uint num = actionInfo.AbilityGrpId();
		if (_abilityIdToAbilityCount.ContainsKey(num))
		{
			_abilityIdToAbilityCount[num]++;
		}
		else
		{
			_abilityIdToAbilityCount[num] = 1u;
			if (!TryGetPsuedoCardInstance(actionInfo.InstanceId(), _gameStateProvider.CurrentGameState, out var instance))
			{
				return true;
			}
			ICardDataAdapter cardDataAdapter = ActionEffectModel(instance, actionInfo);
			DuelScene_CDC duelScene_CDC = _gameEffectBuilder.Create(GameEffectType.ActionEffect, GameEffectKey(num), cardDataAdapter);
			duelScene_CDC.ModelOverride = new ModelOverride(cardDataAdapter);
			_abilityIdToMiniCDCs[num] = duelScene_CDC;
		}
		return true;
	}

	public bool RemoveActionEffect(ActionInfo actionInfo)
	{
		if (!actionInfo.IsActionType(ActionType.SpecialPayment))
		{
			return false;
		}
		uint num = actionInfo.AbilityGrpId();
		if (num == 0)
		{
			return false;
		}
		if (!_abilityIdToAbilityCount.ContainsKey(num))
		{
			return false;
		}
		_abilityIdToAbilityCount[num]--;
		if (_abilityIdToAbilityCount[num] != 0)
		{
			return true;
		}
		_abilityIdToAbilityCount.Remove(num);
		if (!_abilityIdToMiniCDCs.ContainsKey(num))
		{
			return true;
		}
		_abilityIdToMiniCDCs.Remove(num);
		_gameEffectBuilder.Destroy(GameEffectKey(num));
		return true;
	}

	private ICardDataAdapter ActionEffectModel(MtgCardInstance instance, ActionInfo actionInfo)
	{
		MtgCardInstance copy = instance.GetCopy();
		copy.Zone = new MtgZone
		{
			Type = ZoneType.Command,
			Visibility = Visibility.Public,
			Owner = copy.Owner
		};
		copy.ObjectType = GameObjectType.Ability;
		CardData cardData = new CardData(copy, _cardDatabase.CardDataProvider.GetCardPrintingById(copy.GrpId));
		IRulesTextOverride rulesTextOverride = (cardData.RulesTextOverride = new AbilityTextOverride(_cardDatabase, cardData.TitleId).AddAbility(actionInfo.AbilityGrpId()).AddSource(instance).AddSource(cardData.Printing));
		cardData.RulesTextOverride = rulesTextOverride;
		return cardData;
	}

	public static bool TryGetPsuedoCardInstance(uint id, MtgGameState gameState, out MtgCardInstance instance)
	{
		instance = null;
		if (gameState == null)
		{
			return false;
		}
		MtgZone limbo = gameState.Limbo;
		if (limbo == null)
		{
			return false;
		}
		if (!limbo.CardIds.Contains(id))
		{
			return false;
		}
		return gameState.TryGetCard(id, out instance);
	}

	private static string GameEffectKey(uint id)
	{
		return $"Action_Effect_{id}";
	}

	public T GetController<T>() where T : class, IActionEffectController
	{
		return this as T;
	}
}
