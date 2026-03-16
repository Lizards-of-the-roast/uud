using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card.RulesText;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SideboardActionEffectController : IActionEffectController, IDisposable
{
	private readonly ICardDataProvider _cardDataProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	private readonly IClientLocProvider _locManager;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly Dictionary<uint, CategoryData> _sourceMappings = new Dictionary<uint, CategoryData>();

	private MtgGameState GameState => _gameStateProvider.CurrentGameState;

	public SideboardActionEffectController(ICardDataProvider cardDataProvider, IGameStateProvider gameStateProvider, ICardHolderProvider cardHolderProvider, ICardBuilder<DuelScene_CDC> cardBuilder, IClientLocProvider locManager, AssetLookupSystem assetLookupSystem)
	{
		_cardDataProvider = cardDataProvider;
		_gameStateProvider = gameStateProvider;
		_cardHolderProvider = cardHolderProvider;
		_cardBuilder = cardBuilder;
		_locManager = locManager;
		_assetLookupSystem = assetLookupSystem;
	}

	public bool AddActionEffect(ActionInfo actionInfo)
	{
		if (actionInfo.IsSideboardAction(GameState))
		{
			if (!_sourceMappings.TryGetValue(actionInfo.Action.SourceId, out var value))
			{
				MtgCardInstance cardById = GameState.GetCardById(actionInfo.Action.SourceId);
				CardPrintingData cardPrintingById = _cardDataProvider.GetCardPrintingById(cardById.GrpId);
				CardPrintingRecord record = cardPrintingById.Record;
				IReadOnlyList<(uint, uint)> abilityIds = Array.Empty<(uint, uint)>();
				uint? flavorTextId = 0u;
				string empty = string.Empty;
				IReadOnlyList<SuperType> supertypes = Array.Empty<SuperType>();
				IReadOnlyList<CardType> types = Array.Empty<CardType>();
				IReadOnlyList<SubType> subtypes = Array.Empty<SubType>();
				CardPrintingData cardPrintingData = new CardPrintingData(cardPrintingById, new CardPrintingRecord(record, null, null, null, null, null, null, flavorTextId, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, empty, null, null, null, null, null, null, null, null, null, null, types, subtypes, supertypes, abilityIds));
				MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance(GameObjectType.Ability);
				mtgCardInstance.Controller = GameState.LocalPlayer;
				CardData cardData = new CardData(mtgCardInstance, cardPrintingData);
				_assetLookupSystem.Blackboard.Clear();
				_assetLookupSystem.Blackboard.GreAction = actionInfo.Action;
				_assetLookupSystem.Blackboard.GreActionType = actionInfo.Action.ActionType;
				RulesTextOverride payload = _assetLookupSystem.TreeLoader.LoadTree<RulesTextOverride>().GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					cardData.RulesTextOverride = new ClientLocTextOverride(_locManager, payload.LocKey);
				}
				else
				{
					cardData.RulesTextOverride = null;
				}
				DuelScene_CDC duelScene_CDC = _cardBuilder.CreateCDC(cardData);
				MtgPlayer playerById = GameState.GetPlayerById(actionInfo.SeatId);
				_cardHolderProvider.GetCardHolder(playerById.ClientPlayerEnum, CardHolderType.Hand).AddCard(duelScene_CDC);
				value = new CategoryData
				{
					CategoricalCdc = duelScene_CDC,
					InstanceIds = new List<uint>()
				};
				_sourceMappings.Add(actionInfo.Action.SourceId, value);
			}
			if (!value.InstanceIds.Contains(actionInfo.Action.InstanceId))
			{
				value.InstanceIds.Add(actionInfo.Action.InstanceId);
			}
			return true;
		}
		return false;
	}

	public bool RemoveActionEffect(ActionInfo actionInfo)
	{
		if (_sourceMappings.TryGetValue(actionInfo.Action.SourceId, out var value))
		{
			value.InstanceIds.Remove(actionInfo.Action.InstanceId);
			if (value.InstanceIds.Count < 1)
			{
				MtgPlayer playerById = GameState.GetPlayerById(actionInfo.SeatId);
				_cardHolderProvider.GetCardHolder(playerById.ClientPlayerEnum, CardHolderType.Hand).RemoveCard(value.CategoricalCdc);
				_cardBuilder.DestroyCDC(value.CategoricalCdc);
				_sourceMappings.Remove(actionInfo.Action.SourceId);
			}
			return true;
		}
		return false;
	}

	public T GetController<T>() where T : class, IActionEffectController
	{
		return this as T;
	}

	public bool IsSideboardCdc(DuelScene_CDC cardView)
	{
		foreach (KeyValuePair<uint, CategoryData> sourceMapping in _sourceMappings)
		{
			if (sourceMapping.Value.CategoricalCdc == cardView)
			{
				return true;
			}
		}
		return false;
	}

	public uint GetSourceId(DuelScene_CDC cardView)
	{
		foreach (KeyValuePair<uint, CategoryData> sourceMapping in _sourceMappings)
		{
			if (sourceMapping.Value.CategoricalCdc == cardView)
			{
				return sourceMapping.Key;
			}
		}
		return 0u;
	}

	public DuelScene_CDC GetSideboardCdc(uint sourceId)
	{
		if (_sourceMappings.TryGetValue(sourceId, out var value))
		{
			return value.CategoricalCdc;
		}
		return null;
	}

	public List<uint> InstanceIds(uint sourceId)
	{
		if (_sourceMappings.TryGetValue(sourceId, out var value))
		{
			return value.InstanceIds;
		}
		return new List<uint>();
	}

	public void Dispose()
	{
		_sourceMappings.Clear();
	}
}
