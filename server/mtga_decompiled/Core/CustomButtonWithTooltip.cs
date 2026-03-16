using UnityEngine;
using Wotc.Mtga.Extensions;

public class CustomButtonWithTooltip : CustomButton
{
	private TooltipTrigger _tooltip;

	private bool _init;

	private void Init()
	{
		if (!_init)
		{
			_init = true;
			_tooltip = GetComponent<TooltipTrigger>();
			if (_tooltip == null)
			{
				Debug.LogWarning("Button should have a tooltip and it does not!");
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		Init();
	}

	public void SetTooltipText(string localizedText)
	{
		Init();
		if (_tooltip != null)
		{
			_tooltip.TooltipData.Text = localizedText;
		}
	}

	public void Show(bool interactable, bool showTooltip)
	{
		Init();
		base.gameObject.UpdateActive(active: true);
		base.Interactable = interactable;
		if (_tooltip != null)
		{
			_tooltip.IsActive = showTooltip;
		}
	}

	public void Hide()
	{
		base.gameObject.UpdateActive(active: false);
	}

	public bool IsHidden()
	{
		return !base.gameObject.activeSelf;
	}
}
