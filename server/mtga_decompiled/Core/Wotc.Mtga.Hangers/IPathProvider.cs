namespace Wotc.Mtga.Hangers;

public interface IPathProvider<T>
{
	string GetPath(T paramType);
}
