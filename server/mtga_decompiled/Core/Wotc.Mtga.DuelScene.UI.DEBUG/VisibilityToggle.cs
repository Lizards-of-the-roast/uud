using System;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class VisibilityToggle : MonoBehaviour
{
	[SerializeField]
	private Toggle _toggle;

	[SerializeField]
	private GameObject[] _objects;

	[SerializeField]
	private RectTransform[] _forcedRebuilds;

	[SerializeField]
	private RectTransform[] _forcedParentRebuilds;

	public event Action<bool> ToggleChanged;

	private void Awake()
	{
		_toggle.onValueChanged.AddListener(OnValueChanged);
	}

	private void OnDestroy()
	{
		_toggle.onValueChanged.RemoveListener(OnValueChanged);
	}

	private void OnValueChanged(bool value)
	{
		GameObject[] array = _objects ?? Array.Empty<GameObject>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value);
		}
		RectTransform[] array2 = _forcedParentRebuilds ?? Array.Empty<RectTransform>();
		for (int i = 0; i < array2.Length; i++)
		{
			RectTransform rectTransform = array2[i].parent as RectTransform;
			while (rectTransform != null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
				rectTransform = rectTransform.parent as RectTransform;
			}
		}
		array2 = _forcedRebuilds ?? Array.Empty<RectTransform>();
		for (int i = 0; i < array2.Length; i++)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(array2[i]);
		}
		this.ToggleChanged?.Invoke(value);
	}
}
