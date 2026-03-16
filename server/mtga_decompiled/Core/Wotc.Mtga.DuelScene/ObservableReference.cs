using System;

namespace Wotc.Mtga.DuelScene;

public class ObservableReference<T> : IDisposable where T : class
{
	private T _value;

	public T Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				this.ValueUpdated?.Invoke(value);
			}
		}
	}

	public event Action<T> ValueUpdated;

	public void Dispose()
	{
		_value = null;
		this.ValueUpdated = null;
	}

	public static implicit operator T(ObservableReference<T> observableReference)
	{
		if (observableReference == null)
		{
			return null;
		}
		return observableReference.Value;
	}
}
