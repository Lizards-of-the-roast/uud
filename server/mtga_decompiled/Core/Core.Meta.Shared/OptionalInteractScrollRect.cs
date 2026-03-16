using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Meta.Shared;

public class OptionalInteractScrollRect : ScrollRect
{
	public bool CanScroll { private get; set; }

	public bool CanDrag { private get; set; }

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (CanDrag)
		{
			base.OnBeginDrag(eventData);
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (CanDrag)
		{
			base.OnEndDrag(eventData);
		}
	}

	public override void OnScroll(PointerEventData data)
	{
		if (CanScroll)
		{
			base.OnScroll(data);
		}
	}
}
