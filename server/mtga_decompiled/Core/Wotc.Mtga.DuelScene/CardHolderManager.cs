using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class CardHolderManager : ICardHolderManager, ICardHolderProvider, ICardHolderController, IGameEffectController, IDisposable
{
	private readonly MutableCardHolderProvider _provider;

	private readonly ICardHolderBuilder _builder;

	private readonly ISignalDispatch<ZoneCardHolderCreatedSignalArgs> _zoneCardHolderCreated;

	public CardBrowserCardHolder DefaultBrowser { get; private set; }

	public ExamineViewCardHolder Examine { get; private set; }

	public CardHolderManager(MutableCardHolderProvider provider, ICardHolderBuilder builder, ISignalDispatch<ZoneCardHolderCreatedSignalArgs> zoneCardHolderCreated)
	{
		_provider = provider ?? new MutableCardHolderProvider();
		_builder = builder ?? NullCardHolderBuilder.Default;
		_zoneCardHolderCreated = zoneCardHolderCreated;
	}

	public void BuildNonZoneCardHolders()
	{
		CreateCardHolder(CardHolderType.Invalid, GREPlayerNum.Invalid);
		DefaultBrowser = CreateCardHolder(CardHolderType.CardBrowserDefault, GREPlayerNum.Invalid) as CardBrowserCardHolder;
		Examine = CreateCardHolder(CardHolderType.Examine, GREPlayerNum.Invalid) as ExamineViewCardHolder;
		CreateCardHolder(CardHolderType.CardBrowserViewDismiss, GREPlayerNum.Invalid);
	}

	public ICardHolder GetCardHolderByZoneId(uint zoneId)
	{
		return _provider.GetCardHolderByZoneId(zoneId);
	}

	public ICardHolder GetCardHolder(GREPlayerNum playerNum, CardHolderType zoneType)
	{
		return _provider.GetCardHolder(playerNum, zoneType);
	}

	public bool TryGetCardHolder(GREPlayerNum playerNum, CardHolderType zoneType, out ICardHolder cardHolder)
	{
		return _provider.TryGetCardHolder(playerNum, zoneType, out cardHolder);
	}

	public void AddGameEffect(DuelScene_CDC card, GameEffectType effectType)
	{
		if (!(card == null) && card.Model != null && TryGetCardHolder(card.Model.OwnerNum, CardHolderType.Command, out var cardHolder) && cardHolder is IGameEffectController gameEffectController)
		{
			gameEffectController.AddGameEffect(card, effectType);
		}
	}

	public IEnumerable<DuelScene_CDC> GetAllGameEffects()
	{
		foreach (CardHolderBase allCardHolder in _provider.AllCardHolders)
		{
			if (!(allCardHolder is IGameEffectController gameEffectController))
			{
				continue;
			}
			foreach (DuelScene_CDC allGameEffect in gameEffectController.GetAllGameEffects())
			{
				yield return allGameEffect;
			}
		}
	}

	public ICardHolder CreateCardHolder(MtgZone zone)
	{
		if (zone == null)
		{
			return null;
		}
		ICardHolder cardHolder = _builder.CreateCardHolder(zone.Type.ToCardHolderType(), zone.OwnerNum);
		if (cardHolder == null)
		{
			return null;
		}
		_provider.ZoneIdToCardHolder[zone.Id] = cardHolder;
		GREPlayerNum ownerNum = zone.OwnerNum;
		CardHolderType key = zone.Type.ToCardHolderType();
		if (_provider.PlayerTypeMap.TryGetValue(ownerNum, out var value))
		{
			value[key] = cardHolder;
		}
		else
		{
			_provider.PlayerTypeMap[ownerNum] = new Dictionary<CardHolderType, ICardHolder> { { key, cardHolder } };
		}
		if (cardHolder is CardHolderBase cardHolderBase)
		{
			_provider.AllCardHolders.Add(cardHolderBase);
			string name = ((zone.Owner != null) ? $"{cardHolderBase.name} ZoneId: #{zone.Id} | Type: {zone.Type} | OwnerId: #{zone.Owner.InstanceId}" : $"{cardHolderBase.name} ZoneId: #{zone.Id} | Type: {zone.Type} ");
			cardHolderBase.name = name;
		}
		if (cardHolder is ZoneCardHolderBase zoneCardHolderBase)
		{
			zoneCardHolderBase.UpdateZoneModel(zone);
		}
		_zoneCardHolderCreated.Dispatch(new ZoneCardHolderCreatedSignalArgs(this, cardHolder, zone));
		return cardHolder;
	}

	public ICardHolder CreateSubCardHolder(MtgZone zone, MtgPlayer player)
	{
		if (zone == null || player == null)
		{
			return null;
		}
		ICardHolder cardHolder = _builder.CreateCardHolder(zone.Type.ToCardHolderType(), player.ClientPlayerEnum);
		if (cardHolder == null)
		{
			return null;
		}
		uint id = zone.Id;
		uint instanceId = player.InstanceId;
		_provider.SubCardHolderMap[(id, instanceId)] = cardHolder;
		_provider.PlayerTypeMap[player.ClientPlayerEnum][zone.Type.ToCardHolderType()] = cardHolder;
		if (cardHolder is CardHolderBase cardHolderBase)
		{
			_provider.AllCardHolders.Add(cardHolderBase);
			string name = $"{zone.Type} PlayerId: #{player.InstanceId}";
			cardHolderBase.name = name;
		}
		if (_provider.ZoneIdToCardHolder.TryGetValue(zone.Id, out var value))
		{
			if (value is ICommandCardHolder commandCardHolder)
			{
				commandCardHolder.AddSubCardHolderForPlayer(cardHolder, instanceId);
			}
			else if (value is IExileCardHolder exileCardHolder)
			{
				exileCardHolder.AddSubCardHolderForPlayer(cardHolder, instanceId);
			}
		}
		if (cardHolder is ZoneCardHolderBase zoneCardHolderBase)
		{
			zoneCardHolderBase.UpdateZoneModel(zone);
		}
		return cardHolder;
	}

	public bool DeleteCardHolder(uint zoneId)
	{
		if (!_provider.ZoneIdToCardHolder.TryGetValue(zoneId, out var value))
		{
			return false;
		}
		if (_provider.PlayerTypeMap.TryGetValue(value.PlayerNum, out var value2))
		{
			value2.Remove(value.CardHolderType);
		}
		if (value is CardHolderBase item)
		{
			_provider.AllCardHolders.Remove(item);
		}
		return _builder.DestroyCardHolder(value);
	}

	public bool DeleteSubCardHolder(uint zoneId, uint playerId)
	{
		if (!_provider.SubCardHolderMap.TryGetValue((zoneId, playerId), out var value))
		{
			return false;
		}
		if (_provider.ZoneIdToCardHolder.TryGetValue(zoneId, out var value2))
		{
			if (value2 is ICommandCardHolder commandCardHolder)
			{
				commandCardHolder.RemoveSubCardHolderForPlayer(playerId);
			}
			else if (value2 is IExileCardHolder exileCardHolder)
			{
				exileCardHolder.RemoveSubCardHolderForPlayer(playerId);
			}
		}
		if (_provider.PlayerTypeMap.TryGetValue(value.PlayerNum, out var value3))
		{
			value3.Remove(value.CardHolderType);
		}
		return _builder.DestroyCardHolder(value);
	}

	private CardHolderBase CreateCardHolder(CardHolderType cardHolderType, GREPlayerNum owner)
	{
		ICardHolder cardHolder = _builder.CreateCardHolder(cardHolderType, owner);
		if (cardHolder == null)
		{
			return null;
		}
		if (_provider.PlayerTypeMap.TryGetValue(owner, out var value))
		{
			value[cardHolderType] = cardHolder;
		}
		else
		{
			_provider.PlayerTypeMap[owner] = new Dictionary<CardHolderType, ICardHolder> { { cardHolderType, cardHolder } };
		}
		if (cardHolder is CardHolderBase cardHolderBase)
		{
			_provider.AllCardHolders.Add(cardHolderBase);
			return cardHolderBase;
		}
		return null;
	}

	public bool TryGetCardHolder<T>(GREPlayerNum playerType, CardHolderType cardHolderType, out T result) where T : ICardHolder
	{
		result = (T)GetCardHolder(playerType, cardHolderType);
		return result != null;
	}

	public void Dispose()
	{
		foreach (CardHolderBase allCardHolder in _provider.AllCardHolders)
		{
			allCardHolder.CardViews.Clear();
		}
		_provider.AllCardHolders.Clear();
	}
}
