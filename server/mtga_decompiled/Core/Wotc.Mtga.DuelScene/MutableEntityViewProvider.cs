using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class MutableEntityViewProvider : IEntityViewProvider, ICardViewProvider, IAvatarViewProvider, IFakeCardViewProvider
{
	private readonly MutableCardViewProvider _cards;

	private readonly MutableFakeCardViewProvider _fakeCards;

	private readonly MutableAvatarViewProvider _avatars;

	public List<DuelScene_CDC> AllCards => _cards.AllCards;

	public Dictionary<uint, DuelScene_CDC> CardViews => _cards.CardViews;

	public Dictionary<string, DuelScene_CDC> FakeCards => _fakeCards.FakeCards;

	public HashSet<DuelScene_AvatarView> AllAvatars => _avatars.AllAvatars;

	public Dictionary<uint, DuelScene_AvatarView> AvatarViewsById => _avatars.AvatarViewsById;

	public Dictionary<GREPlayerNum, DuelScene_AvatarView> AvatarViewsByEnum => _avatars.AvatarViewsByEnum;

	public MutableEntityViewProvider(MutableCardViewProvider cards = null, MutableFakeCardViewProvider fakeCards = null, MutableAvatarViewProvider avatars = null)
	{
		_cards = cards ?? new MutableCardViewProvider();
		_fakeCards = fakeCards ?? new MutableFakeCardViewProvider();
		_avatars = avatars ?? new MutableAvatarViewProvider();
	}

	public IEnumerable<DuelScene_CDC> GetAllCards()
	{
		return _cards.GetAllCards();
	}

	public DuelScene_CDC GetCardView(uint cardId)
	{
		return _cards.GetCardView(cardId);
	}

	public bool TryGetCardView(uint cardId, out DuelScene_CDC cardView)
	{
		return _cards.TryGetCardView(cardId, out cardView);
	}

	public IEnumerable<DuelScene_CDC> GetAllFakeCards()
	{
		return _fakeCards.GetAllFakeCards();
	}

	public DuelScene_CDC GetFakeCard(string key)
	{
		return _fakeCards.GetFakeCard(key);
	}

	public IEnumerable<DuelScene_AvatarView> GetAllAvatars()
	{
		return _avatars.GetAllAvatars();
	}

	public DuelScene_AvatarView GetAvatarById(uint id)
	{
		return _avatars.GetAvatarById(id);
	}

	public DuelScene_AvatarView GetAvatarByPlayerSide(GREPlayerNum playerType)
	{
		return _avatars.GetAvatarByPlayerSide(playerType);
	}

	public IEntityView GetEntity(uint id)
	{
		if (_avatars.TryGetAvatarById(id, out var avatar))
		{
			return avatar;
		}
		if (_cards.TryGetCardView(id, out var cardView))
		{
			return cardView;
		}
		return null;
	}
}
