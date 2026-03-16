using System.Collections;
using System.Collections.Generic;

namespace Core.Code.Collections;

public class VersionedList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IVersioned
{
	private List<T> _innerList;

	public uint Version { get; private set; } = 1u;

	public int Count => _innerList.Count;

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get
		{
			return _innerList[index];
		}
		set
		{
			_innerList[index] = value;
			MarkDirty();
		}
	}

	private void MarkDirty()
	{
		uint version = Version + 1;
		Version = version;
	}

	public VersionedList()
	{
		_innerList = new List<T>();
	}

	public VersionedList(IEnumerable<T> items)
	{
		_innerList = new List<T>(items);
	}

	public VersionedList(int capacity)
	{
		_innerList = new List<T>(capacity);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _innerList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(T item)
	{
		_innerList.Add(item);
		MarkDirty();
	}

	public void Clear()
	{
		_innerList.Clear();
		MarkDirty();
	}

	public bool Contains(T item)
	{
		return _innerList.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		_innerList.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		if (_innerList.Remove(item))
		{
			MarkDirty();
			return true;
		}
		return false;
	}

	public int IndexOf(T item)
	{
		return _innerList.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		_innerList.Insert(index, item);
		MarkDirty();
	}

	public void RemoveAt(int index)
	{
		_innerList.RemoveAt(index);
		MarkDirty();
	}
}
