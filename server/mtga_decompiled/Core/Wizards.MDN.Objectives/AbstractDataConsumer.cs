using UnityEngine;

namespace Wizards.MDN.Objectives;

public abstract class AbstractDataConsumer<T, P> : AbstractDataConsumer where T : class where P : AbstractDataProvider<T>
{
	protected P _dataProvider;

	protected T Data
	{
		get
		{
			if (_dataProvider != null)
			{
				return _dataProvider.Data;
			}
			return null;
		}
	}

	private void OnEnable()
	{
		if (_dataProvider == null)
		{
			_dataProvider = GetComponentInParent<P>();
		}
		if (_dataProvider != null)
		{
			_dataProvider.onDataChanged.AddListener(OnDataChanged);
			OnDataChanged();
		}
	}

	private void OnDisable()
	{
		if (_dataProvider != null)
		{
			_dataProvider.onDataChanged.RemoveListener(OnDataChanged);
		}
	}
}
public abstract class AbstractDataConsumer : MonoBehaviour
{
	protected abstract void OnDataChanged();
}
