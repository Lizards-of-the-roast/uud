using System.Collections.Generic;

namespace cTMP;

internal static class cTMP_ListPool<T>
{
	private static readonly cTMP_ObjectPool<List<T>> s_ListPool = new cTMP_ObjectPool<List<T>>(null, delegate(List<T> l)
	{
		l.Clear();
	});

	public static List<T> Get()
	{
		return s_ListPool.Get();
	}

	public static void Release(List<T> toRelease)
	{
		s_ListPool.Release(toRelease);
	}
}
