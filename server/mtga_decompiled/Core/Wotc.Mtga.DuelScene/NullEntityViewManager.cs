using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullEntityViewManager : IEntityViewManager, IEntityViewProvider, ICardViewProvider, IAvatarViewProvider, IFakeCardViewProvider, ICardViewManager, ICardViewController, IFakeCardViewManager, IFakeCardViewController, IAvatarViewManager, IAvatarViewController
{
	public static readonly IEntityViewManager Default = new NullEntityViewManager();

	private static readonly IEntityViewProvider Provider = NullEntityViewProvider.Default;

	private static readonly ICardViewController CardController = NullCardViewController.Default;

	private static readonly IFakeCardViewController FakeCardController = NullFakeCardViewController.Default;

	private static readonly IAvatarViewController AvatarController = NullAvatarViewController.Default;

	public uint GetCardUpdatedId(uint id)
	{
		return 0u;
	}

	public uint GetCardPreviousId(uint id)
	{
		return 0u;
	}

	public IEntityView GetEntity(uint id)
	{
		return Provider.GetEntity(id);
	}

	public DuelScene_CDC GetCardView(uint cardId)
	{
		return Provider.GetCardView(cardId);
	}

	public IEnumerable<DuelScene_CDC> GetAllCards()
	{
		return Provider.GetAllCards();
	}

	public bool TryGetCardView(uint cardId, out DuelScene_CDC cardView)
	{
		return Provider.TryGetCardView(cardId, out cardView);
	}

	public IEnumerable<DuelScene_AvatarView> GetAllAvatars()
	{
		return Provider.GetAllAvatars();
	}

	public DuelScene_AvatarView GetAvatarById(uint id)
	{
		return Provider.GetAvatarById(id);
	}

	public DuelScene_AvatarView GetAvatarByPlayerSide(GREPlayerNum playerType)
	{
		return Provider.GetAvatarByPlayerSide(playerType);
	}

	public IEnumerable<DuelScene_CDC> GetAllFakeCards()
	{
		return Provider.GetAllFakeCards();
	}

	public DuelScene_CDC GetFakeCard(string key)
	{
		return Provider.GetFakeCard(key);
	}

	public DuelScene_CDC CreateCardView(ICardDataAdapter cardData)
	{
		return CardController.CreateCardView(cardData);
	}

	public DuelScene_CDC UpdateIdForCardView(uint oldId, uint newId)
	{
		return CardController.UpdateIdForCardView(oldId, newId);
	}

	public void DeleteCard(params uint[] cardIds)
	{
		CardController.DeleteCard(cardIds);
	}

	public DuelScene_AvatarView CreateAvatarView(MtgPlayer player)
	{
		return AvatarController.CreateAvatarView(player);
	}

	public bool DeleteAvatar(uint playerId)
	{
		return AvatarController.DeleteAvatar(playerId);
	}

	public DuelScene_CDC CreateFakeCard(string key, ICardDataAdapter cardData, bool isVisible = false)
	{
		return FakeCardController.CreateFakeCard(key, cardData, isVisible);
	}

	public bool DeleteFakeCard(string key)
	{
		return FakeCardController.DeleteFakeCard(key);
	}
}
