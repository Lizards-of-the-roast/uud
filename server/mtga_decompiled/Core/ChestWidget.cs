using System;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class ChestWidget : MonoBehaviour
{
	[SerializeField]
	private CustomButton button;

	[SerializeField]
	private Localize body;

	private bool initialized = true;

	public bool Disable;

	public event Action Clicked;

	private void Awake()
	{
		button.OnClick.AddListener(Click);
	}

	public void Init()
	{
		initialized = true;
	}

	private void OnDestroy()
	{
		button.OnClick.RemoveListener(Click);
		this.Clicked = null;
	}

	private void Click()
	{
		if (initialized)
		{
			this.Clicked?.Invoke();
		}
	}

	public void Activate(bool activate)
	{
		base.gameObject.UpdateActive(activate && initialized && !Disable);
	}

	public void SetBodyText(string locKey)
	{
		body.SetText(locKey);
	}
}
