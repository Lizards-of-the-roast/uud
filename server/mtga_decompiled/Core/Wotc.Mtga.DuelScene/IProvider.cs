namespace Wotc.Mtga.DuelScene;

public interface IProvider<T> where T : class
{
	T Get();

	bool TryGet(out T result)
	{
		result = Get();
		return result != null;
	}
}
