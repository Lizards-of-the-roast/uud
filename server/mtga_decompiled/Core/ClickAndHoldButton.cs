using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickAndHoldButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private float _clickAndHoldDuration = 0.1f;

	private float _clickTimeAtPointerDown;

	private Coroutine _clickAndHoldTimer;

	public event Action<PointerEventData> Clicked;

	public event Action<PointerEventData> PointerDown;

	public event Action<PointerEventData> PointerEnter;

	public event Action<PointerEventData> PointerExit;

	public event Action<PointerEventData> PointerUp;

	public event Action ClickAndHold;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!ClickWasTooLong(Time.time, _clickTimeAtPointerDown))
		{
			this.Clicked?.Invoke(eventData);
		}
	}

	private bool ClickWasTooLong(float currentClickTime, float atPointerDown)
	{
		return currentClickTime - atPointerDown >= _clickAndHoldDuration;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_clickTimeAtPointerDown = Time.time;
		this.PointerDown?.Invoke(eventData);
		if (base.gameObject.activeInHierarchy)
		{
			_clickAndHoldTimer = StartCoroutine(CheckClickAndHold());
		}
		EventSystem.current.SetSelectedGameObject(base.gameObject);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this.PointerEnter?.Invoke(eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.PointerExit?.Invoke(eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		this.PointerUp?.Invoke(eventData);
		if (_clickAndHoldTimer != null)
		{
			StopCoroutine(_clickAndHoldTimer);
		}
	}

	public IEnumerator CheckClickAndHold()
	{
		yield return new WaitForSecondsRealtime(_clickAndHoldDuration);
		this.ClickAndHold?.Invoke();
	}

	private void OnDestroy()
	{
		this.Clicked = null;
		this.PointerDown = null;
		this.PointerEnter = null;
		this.PointerExit = null;
		this.PointerUp = null;
		this.ClickAndHold = null;
	}
}
