using UnityEngine.EventSystems;
using Wizards.Mtga.Platforms;

namespace Wotc.Mtga.DuelScene;

public class FullControlToggle : FullControl, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public bool Visible { get; private set; }

	protected override void Start()
	{
	}

	protected override void UpdateVisuals(bool fullControlEnabled, bool locked)
	{
		_keyboardToggleButton.SetToggled(fullControlEnabled);
	}

	public void HideToggle()
	{
		Visible = false;
		_isHovered = false;
		_keyboardToggleButton.HideToggle();
	}

	public void ShowToggle()
	{
		Visible = true;
		_keyboardToggleButton.ShowToggle();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (!(CardDragController.DraggedCard != null))
		{
			_isHovered = true;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject(base.gameObject);
		if (PlatformUtils.IsHandheld())
		{
			OnPointerEnter(eventData);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_isHovered = false;
	}
}
