using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MulliganRulesView : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public event Action<MulliganRulesView> ClosePressed;

	public void OnPointerClick(PointerEventData eventData)
	{
		this.ClosePressed?.Invoke(this);
	}

	private void OnEnable()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_mulligan_discard, base.gameObject);
	}

	private void OnDisable()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_mulligan_discard, base.gameObject);
	}
}
