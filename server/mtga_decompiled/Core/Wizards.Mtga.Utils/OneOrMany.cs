using System;
using System.Collections;
using System.Collections.Generic;

namespace Wizards.Mtga.Utils;

public readonly struct OneOrMany<T> : IEnumerable<T>, IEnumerable
{
	private struct SingleEnumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly T _single;

		private uint _state;

		public T Current => _state switch
		{
			0u => default(T), 
			1u => _single, 
			2u => default(T), 
			_ => throw new ArgumentOutOfRangeException(), 
		};

		object IEnumerator.Current => Current;

		public SingleEnumerator(in T single)
		{
			_single = single;
			_state = 0u;
		}

		public bool MoveNext()
		{
			return ++_state == 1;
		}

		public void Reset()
		{
			_state = 0u;
		}

		public void Dispose()
		{
			if (_single is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	private readonly T _single;

	private readonly IEnumerable<T> _enumerable;

	internal OneOrMany(in IEnumerable<T> enumerable)
	{
		_single = default(T);
		_enumerable = enumerable;
	}

	private OneOrMany(in T single)
	{
		_single = single;
		_enumerable = null;
	}

	public static implicit operator OneOrMany<T>(in T t)
	{
		return new OneOrMany<T>(in t);
	}

	public static implicit operator OneOrMany<T>(in List<T> ts)
	{
		IEnumerable<T> enumerable = ts;
		return new OneOrMany<T>(in enumerable);
	}

	public static implicit operator OneOrMany<T>(in T[] ts)
	{
		IEnumerable<T> enumerable = ts;
		return new OneOrMany<T>(in enumerable);
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (_enumerable == null)
		{
			return new SingleEnumerator(in _single);
		}
		return _enumerable.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
