using System;
using UnityEngine;

namespace Wotc.Mtga.Extensions;

public static class TransformExtensions
{
	public static void SetLayerRecursive(this Transform trans, string toLayerName, bool forceLayer = false)
	{
		int num = LayerMask.NameToLayer(toLayerName);
		if (num < 0 || num >= 32)
		{
			throw new ArgumentException("invalid layer name (" + toLayerName + ")", "toLayerName");
		}
		trans.SetLayerRecursiveHelper(num, trans.gameObject.layer, forceLayer);
	}

	public static void SetLayerRecursive(this Transform trans, int toLayer, bool forceLayer = false)
	{
		if (toLayer < 0 || toLayer >= 32)
		{
			throw new ArgumentException($"invalid layer ({toLayer})", "toLayer");
		}
		trans.SetLayerRecursiveHelper(toLayer, trans.gameObject.layer, forceLayer);
	}

	private static void SetLayerRecursiveHelper(this Transform trans, int toLayer, int fromLayer, bool foceLayerOverride = false)
	{
		if (trans.gameObject.layer == fromLayer || foceLayerOverride)
		{
			trans.gameObject.layer = toLayer;
		}
		for (int i = 0; i < trans.childCount; i++)
		{
			trans.GetChild(i).SetLayerRecursiveHelper(toLayer, fromLayer, foceLayerOverride);
		}
	}

	public static void ZeroOut(this Transform me)
	{
		if ((bool)me)
		{
			me.localPosition = Vector3.zero;
			me.localRotation = Quaternion.identity;
			me.localScale = Vector3.one;
		}
	}

	public static void DestroyChildren(this Transform transform, bool immediate = false)
	{
		for (int num = transform.childCount - 1; num >= 0; num--)
		{
			if (immediate)
			{
				UnityEngine.Object.DestroyImmediate(transform.GetChild(num).gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(transform.GetChild(num).gameObject);
			}
		}
	}

	public static Vector2 ParentSize(this RectTransform rectTransform)
	{
		if (rectTransform.parent is RectTransform { rect: var rect })
		{
			return rect.size;
		}
		return rectTransform.sizeDelta;
	}

	public static void StretchToParent(this RectTransform rectTransform, Transform parent = null)
	{
		if (parent != null && rectTransform.parent != parent)
		{
			rectTransform.SetParent(parent);
		}
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.sizeDelta = Vector2.zero;
		rectTransform.ZeroOut();
	}

	public static Bounds GetBounds(this RectTransform rectTransform)
	{
		Transform parentSpace = (rectTransform.GetComponent<Canvas>() ? rectTransform : rectTransform.parent);
		return rectTransform.GetBounds(parentSpace);
	}

	public static Bounds GetBounds(this RectTransform rectTransform, Transform parentSpace)
	{
		Rect rect = rectTransform.rect;
		Vector3 vector = (parentSpace ? parentSpace.TransformPoint(rect.min) : ((Vector3)rect.min));
		Vector3 vector2 = (parentSpace ? parentSpace.TransformPoint(rect.max) : ((Vector3)rect.max));
		return new Bounds(rectTransform.position, vector2 - vector);
	}

	public static bool GetMouseOver(this RectTransform rectTransform)
	{
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, CurrentCamera.Value, out var localPoint))
		{
			return rectTransform.rect.Contains(localPoint);
		}
		return false;
	}
}
