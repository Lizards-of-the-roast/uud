using System.Collections.Generic;

namespace Wotc.Mtga;

public static class IContextExtensions
{
	public static bool TryGet<T>(this IContext context, out T result)
	{
		result = context.Get<T>();
		return !EqualityComparer<T>.Default.Equals(result, default(T));
	}
}
