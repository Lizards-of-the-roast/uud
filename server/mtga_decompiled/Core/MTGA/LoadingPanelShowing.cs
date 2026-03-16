using System;
using UnityEngine;

namespace MTGA;

public class LoadingPanelShowing : MonoBehaviour
{
	private static bool _isShowing;

	public static bool IsShowing
	{
		get
		{
			return _isShowing;
		}
		private set
		{
			_isShowing = value;
			LoadingPanelShowing.IsShowingChanged?.Invoke(_isShowing);
		}
	}

	public static event Action<bool> IsShowingChanged;

	private void OnEnable()
	{
		IsShowing = true;
	}

	private void OnDisable()
	{
		IsShowing = false;
	}
}
