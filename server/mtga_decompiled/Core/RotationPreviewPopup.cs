using System;
using UnityEngine;

public class RotationPreviewPopup : PopupBase
{
	[SerializeField]
	private Animator _animator;

	private Action _onComplete;

	public void Init(Action onComplete)
	{
		_animator.gameObject.SetActive(value: true);
		_onComplete = onComplete;
		Activate(activate: true);
	}

	private void Update()
	{
		if (!_animator.gameObject.activeSelf)
		{
			OnComplete();
		}
	}

	public void OnComplete()
	{
		_onComplete?.Invoke();
		Activate(activate: false);
	}

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
	}
}
