using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wizards.Mtga.DeckBuilder;

[RequireComponent(typeof(RectMask2D))]
public class Rect2DTextMeshProFix : MonoBehaviour
{
	private void OnEnable()
	{
		RectMask2D component = GetComponent<RectMask2D>();
		TextMeshPro[] componentsInChildren = GetComponentsInChildren<TextMeshPro>();
		foreach (TextMeshPro clippable in componentsInChildren)
		{
			component.RemoveClippable(clippable);
		}
	}

	private void OnDisable()
	{
		RectMask2D component = GetComponent<RectMask2D>();
		TextMeshPro[] componentsInChildren = GetComponentsInChildren<TextMeshPro>();
		foreach (TextMeshPro clippable in componentsInChildren)
		{
			component.RemoveClippable(clippable);
		}
	}
}
