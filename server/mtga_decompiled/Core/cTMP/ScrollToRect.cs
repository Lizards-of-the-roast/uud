using UnityEngine;
using UnityEngine.UI;

namespace cTMP;

public class ScrollToRect : MonoBehaviour
{
	private static float SCROLL_MARGIN = 0.3f;

	private ScrollRect sr;

	public void Awake()
	{
		sr = base.gameObject.GetComponent<ScrollRect>();
	}

	public void ScrollToSelected(GameObject selectedObject)
	{
		float height = sr.content.rect.height;
		float height2 = sr.viewport.rect.height;
		float y = selectedObject.transform.localPosition.y;
		float num = y + selectedObject.GetComponent<RectTransform>().rect.height / 2f;
		float num2 = y - selectedObject.GetComponent<RectTransform>().rect.height / 2f;
		float num3 = (height - height2) * sr.normalizedPosition.y - height;
		float num4 = num3 + height2;
		float num5;
		if (num > num4)
		{
			num5 = num - height2 + selectedObject.GetComponent<RectTransform>().rect.height * SCROLL_MARGIN;
		}
		else
		{
			if (!(num2 < num3))
			{
				return;
			}
			num5 = num2 - selectedObject.GetComponent<RectTransform>().rect.height * SCROLL_MARGIN;
		}
		float value = (num5 + height) / (height - height2);
		sr.normalizedPosition = new Vector2(0f, Mathf.Clamp01(value));
	}
}
