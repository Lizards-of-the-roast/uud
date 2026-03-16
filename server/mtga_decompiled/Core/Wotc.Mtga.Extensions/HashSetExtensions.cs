using System.Collections.Generic;

namespace Wotc.Mtga.Extensions;

public static class HashSetExtensions
{
	public static bool AddIfNotNull<T>(this HashSet<T> hashSet, T obj) where T : class
	{
		if (obj == null)
		{
			return false;
		}
		return hashSet.Add(obj);
	}
}
