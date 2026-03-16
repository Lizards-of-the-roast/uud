namespace Wotc.Mtga.DuelScene.Input;

public interface IEntityInputEvent<T> where T : IEntityView
{
	void Execute(T entity);
}
