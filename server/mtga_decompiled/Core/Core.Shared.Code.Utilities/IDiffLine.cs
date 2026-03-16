namespace Core.Shared.Code.Utilities;

public interface IDiffLine<out T>
{
	T Content { get; }

	string Symbol { get; }
}
