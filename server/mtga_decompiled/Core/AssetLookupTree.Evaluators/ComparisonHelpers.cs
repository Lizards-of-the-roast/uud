using System.Collections.Generic;

namespace AssetLookupTree.Evaluators;

public static class ComparisonHelpers
{
	public static bool Overlaps<T>(HashSet<T> hashset, IEnumerable<T> enumer)
	{
		foreach (T item in enumer)
		{
			if (hashset.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetIntersectionCount<T>(HashSet<T> hashset, IEnumerable<T> enumer)
	{
		int num = 0;
		foreach (T item in enumer)
		{
			if (hashset.Contains(item))
			{
				num++;
			}
		}
		return num;
	}

	public static bool IsSuperSet<T>(HashSet<T> superSet, IEnumerable<T> enumer)
	{
		foreach (T item in enumer)
		{
			if (!superSet.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsSubset<T>(HashSet<T> subset, IEnumerable<T> enumer)
	{
		int num = 0;
		foreach (T item in enumer)
		{
			if (subset.Contains(item))
			{
				num++;
			}
		}
		return num == subset.Count;
	}

	public static bool IsSetEqual<T>(HashSet<T> hashset, IEnumerable<T> enumer)
	{
		int num = 0;
		foreach (T item in enumer)
		{
			if (!hashset.Contains(item))
			{
				return false;
			}
			num++;
		}
		return num == hashset.Count;
	}
}
