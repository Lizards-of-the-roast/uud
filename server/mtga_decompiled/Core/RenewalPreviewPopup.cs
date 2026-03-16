using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Loc;

public class RenewalPreviewPopup : PopupBase
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private CustomButton _button;

	[SerializeField]
	private Localize _dateTimeText;

	private Action _onComplete;

	protected override void Awake()
	{
		base.Awake();
		_button.OnClick.AddListener(OnClick);
	}

	private void OnDestroy()
	{
		_button.OnClick.RemoveListener(OnClick);
	}

	private void Update()
	{
		if (!_animator.gameObject.activeSelf)
		{
			OnComplete();
		}
	}

	public void Init(Action onComplete = null)
	{
		_animator.gameObject.SetActive(value: true);
		_onComplete = onComplete;
		_dateTimeText.SetText("Renewal/UpcomingRenewal_SubTitle", new Dictionary<string, string> { 
		{
			"dateTime",
			WrapperController.Instance.RenewalManager.GetCurrentRenewalStartDate().ToString("MMMM dd")
		} });
		Activate(activate: true);
	}

	private void OnClick()
	{
		AudioManager.PlayAudio("sfx_ui_renewal_rotation_egg_dismiss", base.gameObject);
		_animator.SetTrigger("Outro");
	}

	private void OnComplete()
	{
		Action onComplete = _onComplete;
		if (onComplete != null)
		{
			onComplete?.Invoke();
		}
		WrapperController.Instance.NavBarController.EnableProfilePip();
		Activate(activate: false);
	}

	public void InteractEgg()
	{
		AudioManager.PlayAudio("sfx_ui_renewal_rotation_egg_tap", base.gameObject);
	}

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
	}
}
