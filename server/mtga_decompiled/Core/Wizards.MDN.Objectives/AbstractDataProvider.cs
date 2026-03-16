using UnityEngine;
using UnityEngine.Events;

namespace Wizards.MDN.Objectives;

public class AbstractDataProvider<T> : AbstractDataProvider where T : class
{
	private T _data;

	public T Data
	{
		get
		{
			return _data;
		}
		set
		{
			if (_data != value)
			{
				_data = value;
			}
			onDataChanged.Invoke();
		}
	}
}
public class AbstractDataProvider : MonoBehaviour
{
	public UnityEvent onDataChanged = new UnityEvent();
}
