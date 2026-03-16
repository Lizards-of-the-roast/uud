namespace Wotc.Mtga.DuelScene;

public interface IEntityNameProvider<T>
{
	string GetName(T entity, bool formatted = true);
}
