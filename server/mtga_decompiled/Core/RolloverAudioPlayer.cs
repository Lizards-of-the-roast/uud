using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RolloverAudioPlayer : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public string EnterEventName;

	public string ExitEventName;

	public bool MustBeInteractable = true;

	private WwiseEvents _enterEvent;

	private WwiseEvents _exitEvent;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (CanPlayEvent(EnterEventName))
		{
			if (_enterEvent == null || _enterEvent.EventName != EnterEventName)
			{
				_enterEvent = new WwiseEvents(EnterEventName);
			}
			AudioManager.PlayAudio(_enterEvent, AudioManager.Default);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (CanPlayEvent(ExitEventName))
		{
			if (_exitEvent == null || _exitEvent.EventName != ExitEventName)
			{
				_exitEvent = new WwiseEvents(ExitEventName);
			}
			AudioManager.PlayAudio(_exitEvent, AudioManager.Default);
		}
	}

	private bool CanPlayEvent(string eventName)
	{
		if (string.IsNullOrEmpty(eventName))
		{
			return false;
		}
		if (MustBeInteractable)
		{
			CustomButton component = GetComponent<CustomButton>();
			if (component != null && !component.Interactable)
			{
				return false;
			}
			Button component2 = GetComponent<Button>();
			if (component2 != null && !component2.IsInteractable())
			{
				return false;
			}
		}
		return true;
	}
}
