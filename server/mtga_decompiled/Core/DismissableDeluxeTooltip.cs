using System;
using UnityEngine;

public class DismissableDeluxeTooltip : MonoBehaviour
{
	private Action _dismissSystemAction;

	private static readonly int _outro = Animator.StringToHash("Outro");

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Launch(Action dismissButtonPayload, bool autoSkip)
	{
		_dismissSystemAction = dismissButtonPayload;
		base.gameObject.SetActive(value: true);
		if (autoSkip)
		{
			Animator component = GetComponent<Animator>();
			if (component != null)
			{
				component.SetTrigger(_outro);
			}
		}
	}

	public void DeluxeTooltipCompleted()
	{
		if (_dismissSystemAction != null)
		{
			_dismissSystemAction();
		}
	}

	public void PlayNPEDomIntro()
	{
		AudioManager.PlayAudio("sfx_npe_intro_though_trees", base.gameObject);
	}
}
