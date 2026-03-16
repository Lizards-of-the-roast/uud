using System;

namespace Wotc.Mtga.DuelScene;

public class ObservableValue<T> : IDisposable where T : notnull
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
			ref T value2 = ref _value;
			object obj = value;
			if (!value2.Equals(obj))
			{
				_value = value;
				this.ValueUpdated?.Invoke(value);
			}
		}
	}

	public event Action<T> ValueUpdated;

	public void Dispose()
	{
		_value = default(T);
		this.ValueUpdated = null;
	}

	public static implicit operator T(ObservableValue<T> observableValue)
	{
		if (observableValue == null)
		{
			return default(T);
		}
		return observableValue.Value;
	}
}
