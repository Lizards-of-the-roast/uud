using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIGenericButton : MonoBehaviour
{
	private Func<bool> buttonIsEnabledCallback;

	private Button button;

	private event Action buttonClickedEvent;

	public void RegisterCallback(Action myCallback)
	{
		buttonClickedEvent += myCallback;
	}

	public void RegisterCallback<T>(Action<T> myCallback, T myID)
	{
		Action myCallback2 = delegate
		{
			if (myCallback != null)
			{
				myCallback(myID);
			}
		};
		RegisterCallback(myCallback2);
	}

	public void RegisterIsEnabled(Func<bool> myIsEnabledCallback)
	{
		buttonIsEnabledCallback = myIsEnabledCallback;
	}

	public void SetActive(bool isActive)
	{
		button.gameObject.SetActive(isActive);
	}

	public void UnregisterCallbacks()
	{
		this.buttonClickedEvent = null;
	}

	private void Awake()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(HandleButtonClick);
	}

	private void Update()
	{
		if (buttonIsEnabledCallback != null)
		{
			button.interactable = buttonIsEnabledCallback();
		}
	}

	private void HandleButtonClick()
	{
		if (this.buttonClickedEvent != null)
		{
			this.buttonClickedEvent();
		}
	}

	private void OnDestroy()
	{
		this.buttonClickedEvent = null;
	}
}
