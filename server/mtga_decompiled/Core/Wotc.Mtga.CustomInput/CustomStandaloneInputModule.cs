using UnityEngine;
using UnityEngine.EventSystems;

namespace Wotc.Mtga.CustomInput;

public class CustomStandaloneInputModule : StandaloneInputModule
{
	public static PointerEventData GetLastPointerEventData()
	{
		PointerEventData pointerEventData = null;
		EventSystem current = EventSystem.current;
		if ((object)current != null && current.currentInputModule is CustomStandaloneInputModule customStandaloneInputModule)
		{
			pointerEventData = customStandaloneInputModule.GetLastPointerEventData(-1);
			if (pointerEventData == null && Input.touchCount > 0)
			{
				pointerEventData = customStandaloneInputModule.GetTouchPointerEventData(Input.GetTouch(0), out var _, out var _);
			}
		}
		return pointerEventData;
	}
}
