namespace Wotc.Mtga.DuelScene;

public interface IEntityViewProvider : ICardViewProvider, IAvatarViewProvider, IFakeCardViewProvider
{
	IEntityView GetEntity(uint id);

	bool TryGetEntity(uint id, out IEntityView entityView)
	{
		entityView = GetEntity(id);
		return entityView != null;
	}
}
