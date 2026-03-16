namespace Wotc.Mtga;

public class NullContext : IContext
{
	public static readonly IContext Default = new NullContext();

	public T Get<T>()
	{
		return default(T);
	}
}
