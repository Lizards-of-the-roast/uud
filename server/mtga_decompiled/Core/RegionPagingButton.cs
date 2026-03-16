using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegionPagingButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshPro counterText;

	[SerializeField]
	private Image buttonRenderer;

	[SerializeField]
	private Sprite idleSprite;

	[SerializeField]
	private Sprite disabledSprite;

	private int _count;

	private event Action onClick;

	public void SetPosition(Rect rect, float battlefieldY)
	{
		RectTransform obj = (RectTransform)base.transform;
		obj.position = new Vector3(rect.center.x, battlefieldY, rect.center.y);
		obj.sizeDelta = rect.size;
		obj.localRotation = Quaternion.identity;
		GetComponent<BoxCollider>().size = new Vector3(rect.size.x, rect.size.y, 1f);
	}

	public void SetCount(int count)
	{
		_count = count;
		counterText.text = $"x{count}";
		buttonRenderer.sprite = ((count > 0) ? idleSprite : disabledSprite);
	}

	public void SetCallback(Action cb)
	{
		this.onClick = cb;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!eventData.used)
		{
			eventData.Use();
			if (this.onClick != null && _count > 0)
			{
				this.onClick();
			}
		}
	}
}
