using System;
using System.Collections.Generic;

namespace Wotc.Mtga;

public class LambdaEqualityComparer<T> : IEqualityComparer<T>
{
	private readonly Func<T, T, bool> _comparer;

	private readonly Func<T, int> _hasher;

	public LambdaEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hasher)
	{
		_comparer = comparer;
		_hasher = hasher;
	}

	public bool Equals(T x, T y)
	{
		return _comparer(x, y);
	}

	public int GetHashCode(T obj)
	{
		return _hasher(obj);
	}
}
