namespace Wotc.Mtga.DuelScene;

public interface IEntityViewManager : IEntityViewProvider, ICardViewProvider, IAvatarViewProvider, IFakeCardViewProvider, ICardViewManager, ICardViewController, IFakeCardViewManager, IFakeCardViewController, IAvatarViewManager, IAvatarViewController
{
}
