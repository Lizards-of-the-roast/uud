using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class GamewideCountController : IGamewideCountController, IDisposable
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IGameEffectBuilder _gameEffectBuilder;

	private readonly Dictionary<uint, DuelScene_CDC> _miniCDCs = new Dictionary<uint, DuelScene_CDC>();

	private const string FAKE_CARD_KEY_FORMAT = "GameWideCount: {0}";

	public GamewideCountController(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IGameEffectBuilder gameEffectBuilder)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_gameEffectBuilder = gameEffectBuilder ?? NullGameEffectBuilder.Default;
	}

	public void AddGamewideCount(GamewideCountData gamewideCountData)
	{
		uint id = gamewideCountData.Id;
		if (!_miniCDCs.ContainsKey(id))
		{
			ICardDataAdapter cardDataAdapter = ToCardData(gamewideCountData);
			if (cardDataAdapter != null)
			{
				_miniCDCs[id] = _gameEffectBuilder.Create(GameEffectType.GameHistory, FakeCardKey(id), cardDataAdapter);
			}
		}
	}

	public void UpdateGamewideCount(GamewideCountData gamewideCountData)
	{
		if (_miniCDCs.TryGetValue(gamewideCountData.Id, out var value))
		{
			ICardDataAdapter cardDataAdapter = ToCardData(gamewideCountData);
			if (cardDataAdapter != null)
			{
				value.SetModel(cardDataAdapter);
			}
		}
	}

	public void RemoveGamewideCount(GamewideCountData gamewideCountData)
	{
		uint id = gamewideCountData.Id;
		if (_miniCDCs.Remove(id))
		{
			_gameEffectBuilder.Destroy(FakeCardKey(id));
		}
	}

	private ICardDataAdapter ToCardData(GamewideCountData countData)
	{
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		MtgCardInstance cardById = mtgGameState.GetCardById(countData.AffectorId);
		MtgPlayer playerById = mtgGameState.GetPlayerById(countData.AffectedId);
		if (cardById == null || playerById == null)
		{
			return null;
		}
		CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(cardById.GrpId, cardById.SkinCode);
		AbilityPrintingData abilityPrintingById = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(countData.AbilityId);
		CardPrintingData cardPrintingData = cardPrintingById.CreateMiniCDCPrintingData(abilityPrintingById.Id);
		MtgCardInstance mtgCardInstance = cardPrintingData.CreateInstance(GameObjectType.Ability);
		mtgCardInstance.Abilities = new List<AbilityPrintingData> { abilityPrintingById };
		mtgCardInstance.Zone = new MtgZone
		{
			Type = ZoneType.Command,
			Visibility = Visibility.Public,
			Owner = playerById
		};
		mtgCardInstance.Visibility = Visibility.Public;
		mtgCardInstance.ObjectSourceGrpId = cardPrintingData.GrpId;
		mtgCardInstance.Controller = playerById;
		mtgCardInstance.GamewideCounts.Add(countData);
		return new CardData(mtgCardInstance, cardPrintingData);
	}

	public void Dispose()
	{
		foreach (KeyValuePair<uint, DuelScene_CDC> miniCDC in _miniCDCs)
		{
			_gameEffectBuilder.Destroy(FakeCardKey(miniCDC.Key));
		}
		_miniCDCs.Clear();
	}

	private string FakeCardKey(uint gamewideCountId)
	{
		return $"GameWideCount: {gamewideCountId}";
	}
}
