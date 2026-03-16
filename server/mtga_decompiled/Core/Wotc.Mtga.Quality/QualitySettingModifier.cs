using System;

namespace Wotc.Mtga.Quality;

public class QualitySettingModifier
{
	public readonly string SettingName;

	public readonly string FriendlyName;

	public readonly int MaxValue;

	private readonly Action<int> _setSettingDelegate;

	private readonly Func<int, string> _getCurrentValueNameDelegate;

	public int CurrentValue { get; private set; }

	public static event Action<QualitySettingModifier> OnSettingChanged;

	public QualitySettingModifier(string settingName, string friendlyName, int maxValue, Func<int, string> getCurrentValueNameDel, Action<int> setSettingsDel)
	{
		SettingName = settingName;
		FriendlyName = friendlyName;
		MaxValue = maxValue;
		_getCurrentValueNameDelegate = getCurrentValueNameDel;
		_setSettingDelegate = setSettingsDel;
	}

	public string CurrentSettingValueName()
	{
		return _getCurrentValueNameDelegate(CurrentValue);
	}

	public void Set(int v)
	{
		CurrentValue = v % (MaxValue + 1);
		UpdateSetting();
	}

	public void Increment()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		Set(CurrentValue + 1);
	}

	public void Decrement()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		Set(CurrentValue + MaxValue);
	}

	public void UpdateSetting()
	{
		_setSettingDelegate?.Invoke(CurrentValue);
		QualitySettingModifier.OnSettingChanged?.Invoke(this);
	}
}
