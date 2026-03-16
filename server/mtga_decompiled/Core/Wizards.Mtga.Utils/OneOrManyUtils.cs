using System.Collections.Generic;

namespace Wizards.Mtga.Utils;

public static class OneOrManyUtils
{
	public static OneOrMany<T> ToOneOrMany<T>(this IEnumerable<T> ts)
	{
		return new OneOrMany<T>(in ts);
	}
}
