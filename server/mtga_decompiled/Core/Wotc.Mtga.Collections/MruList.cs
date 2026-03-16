using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wotc.Mtga.Collections;

public class MruList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	private readonly HashSet<T> _cache;

	private readonly List<T> _mru;

	public int Count => _cache.Count;

	public int Capacity => _mru.Capacity - 1;

	public bool IsReadOnly => false;

	public T this[T item]
	{
		get
		{
			if (!_cache.Add(item))
			{
				_mru.Remove(item);
			}
			_mru.Insert(0, item);
			TrimMru();
			return item;
		}
	}

	public T this[int index]
	{
		get
		{
			T item = _mru[index];
			return this[item];
		}
	}

	public MruList(int capacity = 100)
	{
		_cache = new HashSet<T>();
		_mru = new List<T>(capacity + 1);
	}

	public MruList(IEnumerable<T> initialValues, int capacity = 100)
	{
		_cache = new HashSet<T>();
		_mru = new List<T>(capacity + 1);
		foreach (T item in initialValues.Take(capacity))
		{
			if (_cache.Add(item))
			{
				_mru.Add(item);
			}
		}
	}

	public void Add(T item)
	{
		if (!_cache.Add(item))
		{
			_mru.Remove(item);
		}
		_mru.Insert(0, item);
		TrimMru();
	}

	public bool Contains(T item)
	{
		return _cache.Contains(item);
	}

	public bool Remove(T item)
	{
		if (_cache.Remove(item))
		{
			_mru.Remove(item);
			return true;
		}
		return false;
	}

	public void Clear()
	{
		_cache.Clear();
		_mru.Clear();
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		_mru.CopyTo(array, arrayIndex);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _mru.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void TrimMru()
	{
		if (_mru.Count == _mru.Capacity)
		{
			_cache.Remove(_mru[_mru.Count - 1]);
			_mru.RemoveAt(_mru.Count - 1);
		}
	}
}
