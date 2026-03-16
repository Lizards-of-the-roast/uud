using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationTrigger : MonoBehaviour
{
	[Serializable]
	public class Entry
	{
		public TriggerType type;

		public UnityEvent callback = new UnityEvent();
	}

	public enum TriggerType
	{
		OnPointerClick,
		OnPointerEnter,
		OnPointerExit,
		OnPointerUp,
		OnPointerDown,
		OnEnable,
		OnDisable
	}

	public List<Entry> delegates;

	public void Execute(TriggerType type)
	{
		if (delegates == null)
		{
			return;
		}
		int i = 0;
		for (int count = delegates.Count; i < count; i++)
		{
			Entry entry = delegates[i];
			if (entry.type == type && entry.callback != null)
			{
				entry.callback.Invoke();
			}
		}
	}
}
