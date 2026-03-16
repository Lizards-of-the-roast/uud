using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class EntityViewManager : IEntityViewManager, IEntityViewProvider, ICardViewProvider, IAvatarViewProvider, IFakeCardViewProvider, ICardViewManager, ICardViewController, IFakeCardViewManager, IFakeCardViewController, IAvatarViewManager, IAvatarViewController, IDisposable
{
	private readonly MutableEntityViewProvider _entityProvider;

	private readonly ICardViewManager _cardViewManager;

	private readonly IFakeCardViewManager _fakeCardViewManager;

	private readonly IAvatarViewManager _avatarViewManager;

	public IEnumerable<DuelScene_AvatarView> GetAllAvatars()
	{
		return _entityProvider.GetAllAvatars();
	}

	public IEnumerable<DuelScene_CDC> GetAllCards()
	{
		return _cardViewManager.GetAllCards();
	}

	public IEnumerable<DuelScene_CDC> GetAllFakeCards()
	{
		return _entityProvider.GetAllFakeCards();
	}

	public EntityViewManager(MutableEntityViewProvider entityProvider, ICardViewManager cardViewManager, IFakeCardViewManager fakeCardViewManager, IAvatarViewManager avtarViewManager)
	{
		_entityProvider = entityProvider ?? new MutableEntityViewProvider();
		_cardViewManager = cardViewManager ?? NullCardViewManager.Default;
		_fakeCardViewManager = fakeCardViewManager ?? NullFakeCardViewManager.Default;
		_avatarViewManager = avtarViewManager ?? NullAvatarViewManager.Default;
	}

	public void Dispose()
	{
		ICardHolder noHolder = new NoCardHolder();
		_entityProvider.AllCards.FindAll((DuelScene_CDC x) => x != null).ForEach(delegate(DuelScene_CDC x)
		{
			x.CurrentCardHolder = noHolder;
		});
		uint[] array = new uint[_entityProvider.CardViews.Keys.Count];
		_entityProvider.CardViews.Keys.CopyTo(array, 0);
		DeleteCard(array);
		List<string> list = new List<string>(_entityProvider.FakeCards.Keys);
		while (list.Count > 0)
		{
			DeleteFakeCard(list[0]);
			list.RemoveAt(0);
		}
		_entityProvider.AllCards.Clear();
	}

	public IEntityView GetEntity(uint id)
	{
		return _entityProvider.GetEntity(id);
	}

	public DuelScene_AvatarView CreateAvatarView(MtgPlayer player)
	{
		uint instanceId = player.InstanceId;
		if (_avatarViewManager.TryGetAvatarById(instanceId, out var _))
		{
			Debug.LogError("Attempting to create duplicate player with Id of " + instanceId);
			return null;
		}
		return _avatarViewManager.CreateAvatarView(player);
	}

	public bool DeleteAvatar(uint playerId)
	{
		return _avatarViewManager.DeleteAvatar(playerId);
	}

	public DuelScene_AvatarView GetAvatarById(uint id)
	{
		return _entityProvider.GetAvatarById(id);
	}

	public DuelScene_AvatarView GetAvatarByPlayerSide(GREPlayerNum playerType)
	{
		return _entityProvider.GetAvatarByPlayerSide(playerType);
	}

	public uint GetCardUpdatedId(uint id)
	{
		return _cardViewManager.GetCardUpdatedId(id);
	}

	public uint GetCardPreviousId(uint id)
	{
		return _cardViewManager.GetCardPreviousId(id);
	}

	public DuelScene_CDC CreateCardView(ICardDataAdapter cardData)
	{
		return _cardViewManager.CreateCardView(cardData);
	}

	public DuelScene_CDC CreateFakeCard(string key, ICardDataAdapter cardData, bool isVisible = false)
	{
		DuelScene_CDC duelScene_CDC = _fakeCardViewManager.CreateFakeCard(key, cardData, isVisible);
		_entityProvider.AllCards.Add(duelScene_CDC);
		return duelScene_CDC;
	}

	public DuelScene_CDC GetFakeCard(string key)
	{
		return _entityProvider.GetFakeCard(key);
	}

	public bool DeleteFakeCard(string key)
	{
		if (!_fakeCardViewManager.TryGetFakeCard(key, out var fakeCdc))
		{
			return false;
		}
		_entityProvider.AllCards.Remove(fakeCdc);
		return _fakeCardViewManager.DeleteFakeCard(key);
	}

	public DuelScene_CDC GetCardView(uint cardId)
	{
		return _entityProvider.GetCardView(cardId);
	}

	public bool TryGetCardView(uint cardId, out DuelScene_CDC cardView)
	{
		return _entityProvider.TryGetCardView(cardId, out cardView);
	}

	public DuelScene_CDC UpdateIdForCardView(uint oldId, uint newId)
	{
		return _cardViewManager.UpdateIdForCardView(oldId, newId);
	}

	public void DeleteCard(params uint[] cardIds)
	{
		_cardViewManager.DeleteCard(cardIds);
	}

	public void UpdateLanguage()
	{
		foreach (DuelScene_CDC allCard in GetAllCards())
		{
			if (!(allCard == null) && allCard.Model != null)
			{
				allCard.UpdateVisuals();
			}
		}
	}
}
