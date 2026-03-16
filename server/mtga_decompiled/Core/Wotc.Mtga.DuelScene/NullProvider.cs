namespace Wotc.Mtga.DuelScene;

public class NullProvider<T> : IProvider<T> where T : class
{
	public static readonly IProvider<T> Default = new NullProvider<T>();

	public T Get()
	{
		return null;
	}
}
