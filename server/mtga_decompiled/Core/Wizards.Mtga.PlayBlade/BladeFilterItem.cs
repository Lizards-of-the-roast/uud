using System;

namespace Wizards.Mtga.PlayBlade;

public class BladeFilterItem : BladeListItem
{
	private BladeEventFilter _bladeEventFilter;

	private Action<BladeEventFilter> _clicked;

	public void SetModel(BladeEventFilter filter)
	{
		_bladeEventFilter = filter;
		ResetToggle();
		SetText(_bladeEventFilter.LocTitle);
	}

	public void Cleanup()
	{
		ResetToggle();
		_bladeEventFilter = null;
		_clicked = null;
	}

	public void SetOnClick(Action<BladeEventFilter> onClick)
	{
		_clicked = onClick;
		SetOnClick(OnClick);
	}

	private void OnClick()
	{
		_clicked?.Invoke(_bladeEventFilter);
	}
}
