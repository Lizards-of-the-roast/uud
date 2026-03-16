using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchTooltipTrigger : TooltipTrigger, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	private bool _toolTipActive;

	private Coroutine _delayCoroutine;

	private void Update()
	{
		if (!Application.isEditor && _toolTipActive && Input.touchCount == 0)
		{
			_delayCoroutine = StartCoroutine(DelayedDisable());
			_toolTipActive = false;
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
	}

	public virtual void OnPointerDown(PointerEventData eventData)
	{
		if (IsActive)
		{
			if (_delayCoroutine != null)
			{
				StopCoroutine(_delayCoroutine);
				_delayCoroutine = null;
			}
			ShowTooltip(eventData);
			_toolTipActive = true;
		}
	}

	public virtual void OnPointerUp(PointerEventData eventData)
	{
		if (IsActive)
		{
			_delayCoroutine = StartCoroutine(DelayedDisable());
			_toolTipActive = false;
		}
	}

	private IEnumerator DelayedDisable()
	{
		yield return new WaitForSeconds(TooltipProperties.DelayDisableDuration);
		TooltipTrigger._tooltipSystem?.RemoveDynamicTooltip(base.gameObject);
		_delayCoroutine = null;
	}
}
