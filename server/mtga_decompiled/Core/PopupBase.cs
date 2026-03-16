using System;
using Core.Meta.MainNavigation.PopUps;
using UnityEngine;
using Wizards.Mtga;

public abstract class PopupBase : MonoBehaviour
{
	public Action OnHide;

	protected IAccountClient _accountClient;

	protected PopupManager _popupManager;

	public bool IsShowing { get; protected set; }

	protected virtual void Awake()
	{
		_accountClient = Pantry.Get<IAccountClient>();
		_popupManager = Pantry.Get<PopupManager>();
	}

	protected virtual void Show()
	{
		base.gameObject.SetActive(value: true);
		IsShowing = true;
		_popupManager?.RegisterPopup(this);
	}

	protected virtual void Hide()
	{
		IsShowing = false;
		base.gameObject.SetActive(value: false);
		_popupManager?.UnregisterPopup(this);
		OnHide?.Invoke();
	}

	public abstract void OnEscape();

	public abstract void OnEnter();

	public virtual void Activate(bool activate)
	{
		if (activate)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}
}
