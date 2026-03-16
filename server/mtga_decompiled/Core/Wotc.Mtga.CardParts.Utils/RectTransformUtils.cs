using UnityEngine;

namespace Wotc.Mtga.CardParts.Utils;

public static class RectTransformUtils
{
	public static Rect GetRelativeRect(RectTransform rectTransform, Vector2 position)
	{
		Rect rect = rectTransform.rect;
		rect.position += new Vector2(position.x, position.y);
		float num = rect.width * rectTransform.localScale.x;
		float num2 = rect.height * rectTransform.localScale.y;
		rect.position = rect.center - new Vector2(num / 2f, num2 / 2f);
		rect.width = num;
		rect.height = num2;
		return rect;
	}
}
