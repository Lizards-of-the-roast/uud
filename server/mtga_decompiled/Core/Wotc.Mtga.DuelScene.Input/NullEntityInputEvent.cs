namespace Wotc.Mtga.DuelScene.Input;

public class NullEntityInputEvent<T> : IEntityInputEvent<T> where T : IEntityView
{
	public void Execute(T entity)
	{
	}
}
