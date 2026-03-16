using System;
using UnityEngine;

public class DeluxeTooltip : MonoBehaviour
{
	public virtual void Show(Action dismissableCallback = null)
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
