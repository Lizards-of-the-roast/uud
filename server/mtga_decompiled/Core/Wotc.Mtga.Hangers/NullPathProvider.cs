namespace Wotc.Mtga.Hangers;

public class NullPathProvider<T> : IPathProvider<T>
{
	public static readonly IPathProvider<T> Default = new NullPathProvider<T>();

	public string GetPath(T paramType)
	{
		return string.Empty;
	}
}
