using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.CustomInput;

public class CustomInputModule : MonoBehaviour
{
	private static bool _usingNewInputSystem;

	public void Start()
	{
		_usingNewInputSystem = ActionSystemFactory.UseNewInput;
		if (_usingNewInputSystem)
		{
			base.gameObject.AddComponent<CustomUIInputModule>();
		}
		else
		{
			base.gameObject.AddComponent<CustomStandaloneInputModule>();
		}
	}

	public static bool IsUsingNewInputSystem()
	{
		return _usingNewInputSystem;
	}

	public static void DeselectAll()
	{
		if (EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	public static bool PointerWasPressedThisFrame()
	{
		if (!IsUsingNewInputSystem())
		{
			return Input.GetMouseButtonDown(0);
		}
		return Pointer.current?.press.wasPressedThisFrame ?? false;
	}

	public static bool PointerWasReleasedThisFrame()
	{
		if (!IsUsingNewInputSystem())
		{
			return Input.GetMouseButtonUp(0);
		}
		return Pointer.current?.press.wasReleasedThisFrame ?? false;
	}

	public static bool PointerIsHeldDown()
	{
		if (!IsUsingNewInputSystem())
		{
			return Input.GetMouseButton(0);
		}
		return Pointer.current?.press.isPressed ?? false;
	}

	public static bool IsRightClick()
	{
		if (!IsUsingNewInputSystem())
		{
			return Input.GetMouseButton(1);
		}
		return Mouse.current?.rightButton.IsPressed() ?? false;
	}

	public static Vector2 GetMouseScroll()
	{
		if (!IsUsingNewInputSystem())
		{
			return Input.mouseScrollDelta;
		}
		return Mouse.current?.scroll.ReadValue() ?? Vector2.zero;
	}

	public static int GetTouchCount()
	{
		if (IsUsingNewInputSystem())
		{
			Touchscreen current = Touchscreen.current;
			if (current == null)
			{
				return 0;
			}
			return current.touches.Count((TouchControl t) => t.isInProgress);
		}
		return Input.touchCount;
	}

	public static PointerEventData GetLastPointerEventData()
	{
		if (!IsUsingNewInputSystem())
		{
			return CustomStandaloneInputModule.GetLastPointerEventData();
		}
		return CustomUIInputModule.GetLastPointerEventData();
	}

	public static IEnumerable<GameObject> GetHovered()
	{
		IEnumerable<GameObject> enumerable2;
		if (!IsUsingNewInputSystem())
		{
			IEnumerable<GameObject> enumerable = CustomStandaloneInputModule.GetLastPointerEventData()?.hovered;
			enumerable2 = enumerable;
			if (enumerable2 == null)
			{
				return Array.Empty<GameObject>();
			}
		}
		else
		{
			enumerable2 = CustomUIInputModule.GetHovered();
		}
		return enumerable2;
	}

	public static Vector2 GetPointerPosition()
	{
		if (IsUsingNewInputSystem())
		{
			return Pointer.current?.position.ReadValue() ?? Vector2.zero;
		}
		return Input.mousePosition;
	}

	public static bool IsOutOfScreenBounds(Vector3 pointerPos, int screenWidth, int screenHeight)
	{
		if (!pointerPos.IsNaN() && !pointerPos.IsInfinity() && !(pointerPos.x < 0f) && !(pointerPos.x >= (float)screenWidth) && !(pointerPos.y < 0f))
		{
			return pointerPos.y >= (float)screenHeight;
		}
		return true;
	}

	public static bool IsAnyInputPressed()
	{
		if (IsUsingNewInputSystem())
		{
			Keyboard current = Keyboard.current;
			if ((current == null || !current.anyKey.isPressed) && !PointerIsHeldDown())
			{
				return IsRightClick();
			}
			return true;
		}
		return Input.anyKey;
	}
}
