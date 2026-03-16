using UnityEngine;

public class TimerAnimationEvents : MonoBehaviour
{
	private LowTimeWarning info;

	private void Awake()
	{
		info = GetComponentInParent<LowTimeWarning>();
	}

	public void TimerPulse()
	{
		if (info.ActiveandVis)
		{
			AudioManager.PostEvent("sfx_ui_timer_pulse", base.gameObject);
		}
	}

	public void TimerCrit()
	{
		if (info.ActiveandVis)
		{
			AudioManager.PostEvent("sfx_ui_timer_pulse_critical", base.gameObject);
		}
	}
}
