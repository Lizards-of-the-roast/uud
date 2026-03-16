using System;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class CustomTab : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Localize _text;

	[SerializeField]
	private Localize _highlightText;

	[SerializeField]
	public GameObject _activeTabIndicator;

	[SerializeField]
	private GameObject _notificationDot;

	private string Anim_Locked_Bool = "Lock";

	private Action Clicked;

	public bool Locked { get; private set; }

	private void Awake()
	{
		if ((bool)_activeTabIndicator)
		{
			_activeTabIndicator.UpdateActive(active: false);
		}
	}

	private void OnEnable()
	{
		_animator.SetBool(Anim_Locked_Bool, Locked);
	}

	private void OnDestroy()
	{
		Clicked = null;
	}

	public void SetLabel(MTGALocalizedString locTerm)
	{
		if ((bool)_text)
		{
			_text.SetText(locTerm);
		}
		if ((bool)_highlightText)
		{
			_highlightText.SetText(locTerm);
		}
	}

	public void OnClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_01, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_card_cosmetic_editing_browser_rollover, base.gameObject);
		if (!Locked)
		{
			Clicked?.Invoke();
		}
	}

	public void OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public void SetLocked(bool locked)
	{
		Locked = locked;
		if (_animator.isActiveAndEnabled)
		{
			_animator.SetBool(Anim_Locked_Bool, Locked);
		}
	}

	public void SetClicked(Action onClick)
	{
		Clicked = onClick;
	}

	public void SetTabActiveVisuals(bool show)
	{
		_activeTabIndicator.UpdateActive(show);
		if ((bool)_highlightText)
		{
			_highlightText.gameObject.UpdateActive(show);
		}
	}

	public void SetPipVisible(bool show)
	{
		_notificationDot.UpdateActive(show);
	}
}
