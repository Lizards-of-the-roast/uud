using System;
using TMPro;
using UnityEngine.EventSystems;

public class DeckbuilderSearchInput : TMP_InputField
{
	public event Action<bool> ShowSearchTips;

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		this.ShowSearchTips?.Invoke(obj: true);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		this.ShowSearchTips?.Invoke(obj: false);
	}
}
