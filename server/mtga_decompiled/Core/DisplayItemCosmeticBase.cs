using System;
using UnityEngine;

public class DisplayItemCosmeticBase : MonoBehaviour
{
	protected Action _onOpen;

	protected Action _onHide;

	protected Action<Action> _storeAction;

	private bool _isReadOnly;

	[SerializeField]
	protected Animator _animator;

	private static readonly int Locked = Animator.StringToHash("Locked");

	protected bool IsReadOnly
	{
		get
		{
			return _isReadOnly;
		}
		set
		{
			_isReadOnly = value;
			if (_animator != null)
			{
				_animator.SetBool(Locked, _isReadOnly);
			}
		}
	}

	public void OnEnable()
	{
		if (_animator != null)
		{
			_animator.SetBool(Locked, _isReadOnly);
		}
	}

	public virtual void SetOnOpenCallback(Action onOpen)
	{
		_onOpen = onOpen;
	}

	public virtual void SetOnCloseCallback(Action onHide)
	{
		_onHide = onHide;
	}

	public void SetOnStoreCallback(Action<Action> storeAction)
	{
		_storeAction = storeAction;
	}

	protected virtual void OnClose()
	{
		_onHide?.Invoke();
	}

	protected virtual void OnStoreSelected(Action storeRedirect)
	{
		_storeAction?.Invoke(storeRedirect);
	}

	public virtual void OpenSelector()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		_onOpen?.Invoke();
	}

	public virtual void CloseSelector()
	{
	}
}
