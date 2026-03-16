using System;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class RevealedCardsManager : IRevealedCardsManager, IRevealedCardsProvider, IRevealedCardsController, IDisposable
{
	private readonly MutableRevealedCardsProvider _provider;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly IGameStateProvider _gameStateProvider;

	public RevealedCardsManager(MutableRevealedCardsProvider provider, ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, ICardHolderProvider cardHolderProvider)
	{
		_provider = provider ?? new MutableRevealedCardsProvider();
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
	}

	public MtgCardInstance GetCardRevealed(uint instanceId)
	{
		return _provider.GetCardRevealed(instanceId);
	}

	public ICardHolder GetAssociatedCardHolder(uint instanceId)
	{
		return _provider.GetAssociatedCardHolder(instanceId);
	}

	public void CreateRevealedCard(uint owner, MtgCardInstance instance, DuelScene_CDC applyTo = null)
	{
		MtgZone zoneForPlayer = ((MtgGameState)_gameStateProvider.CurrentGameState).GetZoneForPlayer(owner, ZoneType.Hand);
		if (zoneForPlayer != null && _cardHolderProvider.TryGetCardHolder<OpponentHandCardHolder>(zoneForPlayer.Id, out var result))
		{
			_provider.RevealedCards[instance.InstanceId] = instance;
			_provider.CardHolderMap[instance.InstanceId] = result;
			ICardDataAdapter toAdd = instance.ToCardData(_cardDatabase);
			if (ApplyToSourceCard(applyTo))
			{
				result.AddRevealData(toAdd, applyTo);
				return;
			}
			result.AddRevealData(toAdd);
			result.BankRevealedData(toAdd);
		}
	}

	private bool ApplyToSourceCard(DuelScene_CDC source)
	{
		if (source == null)
		{
			return false;
		}
		if (source.RevealOverride != null)
		{
			return false;
		}
		return source.Model?.IsDisplayedFaceDown ?? false;
	}

	public void UpdateRevealedCard(MtgCardInstance instance)
	{
		if (instance != null)
		{
			_provider.RevealedCards[instance.InstanceId] = instance;
			if (_provider.CardHolderMap.TryGetValue(instance.InstanceId, out var value))
			{
				value.UpdateRevealedData(instance.ToCardData(_cardDatabase));
			}
		}
	}

	public void DeleteRevealedCard(uint instanceId)
	{
		if (_provider.CardHolderMap.TryGetValue(instanceId, out var value))
		{
			_provider.RevealedCards.Remove(instanceId);
			_provider.CardHolderMap.Remove(instanceId);
			value.RemoveRevealedModel(instanceId);
		}
	}

	public void Dispose()
	{
		_provider.CardHolderMap.Clear();
		_provider.RevealedCards.Clear();
	}
}
