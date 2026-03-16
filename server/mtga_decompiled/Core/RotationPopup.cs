using System;
using UnityEngine;

public class RotationPopup : PopupBase
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private CustomButton _button;

	private Action onComplete;

	protected override void Awake()
	{
		base.Awake();
		_button.OnClick.AddListener(ClickToContinue);
	}

	private void OnDestroy()
	{
		_button.OnClick.RemoveListener(ClickToContinue);
	}

	public void Init(Action complete)
	{
		onComplete = complete;
		Activate(activate: true);
	}

	private void ClickToContinue()
	{
		if (_animator.GetCurrentAnimatorStateInfo(_animator.GetLayerIndex("Base Layer")).IsName("Rotation_Idle"))
		{
			onComplete?.Invoke();
			Activate(activate: false);
		}
	}

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
		ClickToContinue();
	}
}
