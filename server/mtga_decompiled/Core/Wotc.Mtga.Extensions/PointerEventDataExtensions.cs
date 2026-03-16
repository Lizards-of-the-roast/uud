using UnityEngine.EventSystems;
using Wotc.Mtga.CustomInput;

namespace Wotc.Mtga.Extensions;

public static class PointerEventDataExtensions
{
	public static bool ConfirmOnlyButtonPressed(this PointerEventData eventData, PointerEventData.InputButton button)
	{
		if (CustomInputModule.IsUsingNewInputSystem())
		{
			return eventData.button == button;
		}
		if (eventData.button == button)
		{
			return eventData.pointerId <= 0;
		}
		return false;
	}
}
