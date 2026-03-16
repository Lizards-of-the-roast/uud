using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public abstract class MinSizeByTextAdjusterImpl : MonoBehaviour
{
	[Tooltip("Optional: If set this will control the layout element. \n If unset it will control the size of this object.")]
	public LayoutElement layout;

	[Header("Width")]
	public bool controlWidth;

	public float extraWidth;

	public float minWidth;

	public float maxWidth = 1000f;

	public bool setPreferedWidth;

	[Header("Height")]
	public bool controlHeight;

	public float extraHeight;

	public float minHeight;

	public float maxHeight = 1000f;

	public bool setPreferedHeight;

	protected RectTransform rectTransform;

	protected virtual void Reset()
	{
		if (!layout)
		{
			layout = GetComponent<LayoutElement>();
		}
	}

	protected abstract TextMeshProUGUI[] GetTexts();

	protected virtual void Start()
	{
		rectTransform = base.transform as RectTransform;
	}

	private void LateUpdate()
	{
		Adjust();
	}

	private void Adjust()
	{
		bool flag = false;
		float num = 0f;
		float num2 = 0f;
		TextMeshProUGUI[] texts = GetTexts();
		if (texts != null && texts.Length != 0)
		{
			TextMeshProUGUI[] array = texts;
			foreach (TextMeshProUGUI textMeshProUGUI in array)
			{
				if (!(textMeshProUGUI == null))
				{
					flag = true;
					float num3 = textMeshProUGUI.preferredWidth + extraWidth;
					float num4 = textMeshProUGUI.preferredHeight + extraHeight;
					if (num < num3)
					{
						num = num3;
					}
					if (num2 < num4)
					{
						num2 = num4;
					}
				}
			}
		}
		if (!flag)
		{
			return;
		}
		float num5 = Mathf.Clamp(num, minWidth, maxWidth);
		float num6 = Mathf.Clamp(num2, minHeight, maxHeight);
		if ((bool)layout)
		{
			if (controlWidth && num5 != layout.minWidth)
			{
				layout.minWidth = num5;
			}
			if (controlHeight && num6 != layout.minHeight)
			{
				layout.minHeight = num6;
			}
			if (setPreferedWidth)
			{
				layout.preferredWidth = num;
			}
			if (setPreferedHeight)
			{
				layout.preferredHeight = num2;
			}
		}
		else
		{
			if (controlWidth && num5 != rectTransform.sizeDelta.x)
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num5);
			}
			if (controlHeight && num6 != rectTransform.sizeDelta.y)
			{
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num6);
			}
		}
	}
}
