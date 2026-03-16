using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullEntityViewProvider : IEntityViewProvider, ICardViewProvider, IAvatarViewProvider, IFakeCardViewProvider
{
	public static readonly IEntityViewProvider Default = new NullEntityViewProvider();

	private static readonly ICardViewProvider Cards = NullCardViewProvider.Default;

	private static readonly IFakeCardViewProvider FakeCards = NullFakeCardViewProvider.Default;

	private static readonly IAvatarViewProvider Avatars = NullAvatarViewProvider.Default;

	public IEntityView GetEntity(uint id)
	{
		return null;
	}

	public DuelScene_CDC GetCardView(uint cardId)
	{
		return Cards.GetCardView(cardId);
	}

	public IEnumerable<DuelScene_CDC> GetAllCards()
	{
		return Cards.GetAllCards();
	}

	public bool TryGetCardView(uint cardId, out DuelScene_CDC cardView)
	{
		return Cards.TryGetCardView(cardId, out cardView);
	}

	public IEnumerable<DuelScene_CDC> GetAllFakeCards()
	{
		return FakeCards.GetAllFakeCards();
	}

	public DuelScene_CDC GetFakeCard(string key)
	{
		return FakeCards.GetFakeCard(key);
	}

	public IEnumerable<DuelScene_AvatarView> GetAllAvatars()
	{
		return Avatars.GetAllAvatars();
	}

	public DuelScene_AvatarView GetAvatarById(uint id)
	{
		return Avatars.GetAvatarById(id);
	}

	public DuelScene_AvatarView GetAvatarByPlayerSide(GREPlayerNum playerType)
	{
		return Avatars.GetAvatarByPlayerSide(playerType);
	}
}
