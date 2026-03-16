using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpinnerGroup : MonoBehaviour
{
	[SerializeField]
	private bool _useMin = true;

	[SerializeField]
	private int _minValue;

	[SerializeField]
	private bool _useMax;

	[SerializeField]
	private int _maxValue;

	private List<ISpinner> _allSpinners = new List<ISpinner>();

	public bool UseMin
	{
		get
		{
			return _useMin;
		}
		set
		{
			_useMin = value;
		}
	}

	public int MinValue
	{
		get
		{
			return _minValue;
		}
		set
		{
			_minValue = value;
		}
	}

	public bool UseMax
	{
		get
		{
			return _useMax;
		}
		set
		{
			_useMax = value;
		}
	}

	public int MaxValue
	{
		get
		{
			return _maxValue;
		}
		set
		{
			_maxValue = value;
		}
	}

	public void RegisterSpinner(ISpinner spinner)
	{
		_allSpinners.Add(spinner);
	}

	public int ClampNewValue(int newValue, ISpinner thisSpinner)
	{
		int num = _allSpinners.Where((ISpinner s) => s != thisSpinner).Sum((ISpinner s) => s.Value);
		if (_useMin)
		{
			newValue = Mathf.Max(newValue, _minValue - num);
		}
		if (_useMax)
		{
			newValue = Mathf.Min(newValue, _maxValue - num);
		}
		return newValue;
	}
}
