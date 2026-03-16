using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace StatsMonitor.Util;

public class Utils
{
	public static string StripHTMLTags(string s)
	{
		return Regex.Replace(s, "<.*?>", string.Empty);
	}

	public static Color RGBAToColor(float r, float g, float b, float a)
	{
		return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	public static string Color32ToHex(Color32 color)
	{
		return color.r.ToString("x2") + color.g.ToString("x2") + color.b.ToString("x2") + color.a.ToString("x2");
	}

	public static Color HexToColor32(string hex)
	{
		if (hex.Length < 1)
		{
			return Color.black;
		}
		return new Color32(byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber));
	}

	public static void ResetTransform(GameObject obj)
	{
		obj.transform.position = Vector3.zero;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.rotation = Quaternion.identity;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
	}

	public static RectTransform RTransform(GameObject obj, Vector2 anchor, float x = 0f, float y = 0f, float w = 0f, float h = 0f)
	{
		RectTransform rectTransform = obj.GetComponent<RectTransform>();
		if (rectTransform == null)
		{
			rectTransform = obj.AddComponent<RectTransform>();
		}
		RectTransform rectTransform2 = rectTransform;
		RectTransform rectTransform3 = rectTransform;
		Vector2 vector = (rectTransform.anchorMax = anchor);
		Vector2 pivot = (rectTransform3.anchorMin = vector);
		rectTransform2.pivot = pivot;
		rectTransform.anchoredPosition = new Vector2(x, y);
		if (w > 0f)
		{
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
		}
		if (h > 0f)
		{
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
		}
		return rectTransform;
	}

	public static void Fill(Texture2D texture, Color color)
	{
		Color[] pixels = texture.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = color;
		}
		texture.SetPixels(pixels);
		texture.Apply();
	}

	public static void AddToUILayer(GameObject obj)
	{
		int num = LayerMask.NameToLayer("UI");
		if (num > -1)
		{
			obj.layer = num;
		}
	}

	public static float DPIScaleFactor(bool round = false)
	{
		float dpi = Screen.dpi;
		if (dpi <= 0f)
		{
			return -1f;
		}
		float num = dpi / 96f;
		if (num < 1f)
		{
			return 1f;
		}
		if (!round)
		{
			return num;
		}
		return Mathf.Round(num);
	}
}
