using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlideElipse : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField]
	private Image _elipseImage;

	[SerializeField]
	private Sprite _activeElipse;

	[SerializeField]
	private Sprite _inactiveElipse;

	private uint _index;

	private SlideContent _content;

	private Action<uint> _onClickHandler;

	private bool _isActive;

	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			if (value != _isActive)
			{
				if (value)
				{
					_elipseImage.sprite = _activeElipse;
				}
				else
				{
					_elipseImage.sprite = _inactiveElipse;
				}
				_isActive = value;
			}
		}
	}

	public uint Index
	{
		set
		{
			_index = value;
		}
	}

	public SlideContent SlideContent
	{
		get
		{
			return _content;
		}
		set
		{
			_content = value;
		}
	}

	public Action<uint> OnClickHandler
	{
		set
		{
			_onClickHandler = value;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_onClickHandler(_index);
	}
}
