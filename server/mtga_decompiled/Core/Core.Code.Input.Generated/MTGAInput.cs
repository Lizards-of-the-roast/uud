using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Core.Code.Input.Generated;

public class MTGAInput : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct UIActions
	{
		private MTGAInput m_Wrapper;

		public InputAction Navigate => m_Wrapper.m_UI_Navigate;

		public InputAction Submit => m_Wrapper.m_UI_Submit;

		public InputAction Cancel => m_Wrapper.m_UI_Cancel;

		public InputAction Point => m_Wrapper.m_UI_Point;

		public InputAction Click => m_Wrapper.m_UI_Click;

		public InputAction ScrollWheel => m_Wrapper.m_UI_ScrollWheel;

		public InputAction MiddleClick => m_Wrapper.m_UI_MiddleClick;

		public InputAction RightClick => m_Wrapper.m_UI_RightClick;

		public bool enabled => Get().enabled;

		public UIActions(MTGAInput wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IUIActions instance)
		{
			if (instance != null && !m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
				Navigate.started += instance.OnNavigate;
				Navigate.performed += instance.OnNavigate;
				Navigate.canceled += instance.OnNavigate;
				Submit.started += instance.OnSubmit;
				Submit.performed += instance.OnSubmit;
				Submit.canceled += instance.OnSubmit;
				Cancel.started += instance.OnCancel;
				Cancel.performed += instance.OnCancel;
				Cancel.canceled += instance.OnCancel;
				Point.started += instance.OnPoint;
				Point.performed += instance.OnPoint;
				Point.canceled += instance.OnPoint;
				Click.started += instance.OnClick;
				Click.performed += instance.OnClick;
				Click.canceled += instance.OnClick;
				ScrollWheel.started += instance.OnScrollWheel;
				ScrollWheel.performed += instance.OnScrollWheel;
				ScrollWheel.canceled += instance.OnScrollWheel;
				MiddleClick.started += instance.OnMiddleClick;
				MiddleClick.performed += instance.OnMiddleClick;
				MiddleClick.canceled += instance.OnMiddleClick;
				RightClick.started += instance.OnRightClick;
				RightClick.performed += instance.OnRightClick;
				RightClick.canceled += instance.OnRightClick;
			}
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
			Navigate.started -= instance.OnNavigate;
			Navigate.performed -= instance.OnNavigate;
			Navigate.canceled -= instance.OnNavigate;
			Submit.started -= instance.OnSubmit;
			Submit.performed -= instance.OnSubmit;
			Submit.canceled -= instance.OnSubmit;
			Cancel.started -= instance.OnCancel;
			Cancel.performed -= instance.OnCancel;
			Cancel.canceled -= instance.OnCancel;
			Point.started -= instance.OnPoint;
			Point.performed -= instance.OnPoint;
			Point.canceled -= instance.OnPoint;
			Click.started -= instance.OnClick;
			Click.performed -= instance.OnClick;
			Click.canceled -= instance.OnClick;
			ScrollWheel.started -= instance.OnScrollWheel;
			ScrollWheel.performed -= instance.OnScrollWheel;
			ScrollWheel.canceled -= instance.OnScrollWheel;
			MiddleClick.started -= instance.OnMiddleClick;
			MiddleClick.performed -= instance.OnMiddleClick;
			MiddleClick.canceled -= instance.OnMiddleClick;
			RightClick.started -= instance.OnRightClick;
			RightClick.performed -= instance.OnRightClick;
			RightClick.canceled -= instance.OnRightClick;
		}

		public void RemoveCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IUIActions instance)
		{
			foreach (IUIActions uIActionsCallbackInterface in m_Wrapper.m_UIActionsCallbackInterfaces)
			{
				UnregisterCallbacks(uIActionsCallbackInterface);
			}
			m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct DebugActions
	{
		private MTGAInput m_Wrapper;

		public InputAction ToggleDebugMenu => m_Wrapper.m_Debug_ToggleDebugMenu;

		public InputAction OpenDebugMenu => m_Wrapper.m_Debug_OpenDebugMenu;

		public InputAction CloseDebugMenu => m_Wrapper.m_Debug_CloseDebugMenu;

		public bool enabled => Get().enabled;

		public DebugActions(MTGAInput wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Debug;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(DebugActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IDebugActions instance)
		{
			if (instance != null && !m_Wrapper.m_DebugActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_DebugActionsCallbackInterfaces.Add(instance);
				ToggleDebugMenu.started += instance.OnToggleDebugMenu;
				ToggleDebugMenu.performed += instance.OnToggleDebugMenu;
				ToggleDebugMenu.canceled += instance.OnToggleDebugMenu;
				OpenDebugMenu.started += instance.OnOpenDebugMenu;
				OpenDebugMenu.performed += instance.OnOpenDebugMenu;
				OpenDebugMenu.canceled += instance.OnOpenDebugMenu;
				CloseDebugMenu.started += instance.OnCloseDebugMenu;
				CloseDebugMenu.performed += instance.OnCloseDebugMenu;
				CloseDebugMenu.canceled += instance.OnCloseDebugMenu;
			}
		}

		private void UnregisterCallbacks(IDebugActions instance)
		{
			ToggleDebugMenu.started -= instance.OnToggleDebugMenu;
			ToggleDebugMenu.performed -= instance.OnToggleDebugMenu;
			ToggleDebugMenu.canceled -= instance.OnToggleDebugMenu;
			OpenDebugMenu.started -= instance.OnOpenDebugMenu;
			OpenDebugMenu.performed -= instance.OnOpenDebugMenu;
			OpenDebugMenu.canceled -= instance.OnOpenDebugMenu;
			CloseDebugMenu.started -= instance.OnCloseDebugMenu;
			CloseDebugMenu.performed -= instance.OnCloseDebugMenu;
			CloseDebugMenu.canceled -= instance.OnCloseDebugMenu;
		}

		public void RemoveCallbacks(IDebugActions instance)
		{
			if (m_Wrapper.m_DebugActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IDebugActions instance)
		{
			foreach (IDebugActions debugActionsCallbackInterface in m_Wrapper.m_DebugActionsCallbackInterfaces)
			{
				UnregisterCallbacks(debugActionsCallbackInterface);
			}
			m_Wrapper.m_DebugActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct CustomInputActions
	{
		private MTGAInput m_Wrapper;

		public InputAction Escape => m_Wrapper.m_CustomInput_Escape;

		public InputAction Next => m_Wrapper.m_CustomInput_Next;

		public InputAction Accept => m_Wrapper.m_CustomInput_Accept;

		public InputAction Up => m_Wrapper.m_CustomInput_Up;

		public InputAction Down => m_Wrapper.m_CustomInput_Down;

		public InputAction Left => m_Wrapper.m_CustomInput_Left;

		public InputAction Right => m_Wrapper.m_CustomInput_Right;

		public InputAction Find => m_Wrapper.m_CustomInput_Find;

		public InputAction AltViewOpen => m_Wrapper.m_CustomInput_AltViewOpen;

		public InputAction AltViewClose => m_Wrapper.m_CustomInput_AltViewClose;

		public bool enabled => Get().enabled;

		public CustomInputActions(MTGAInput wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_CustomInput;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(CustomInputActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(ICustomInputActions instance)
		{
			if (instance != null && !m_Wrapper.m_CustomInputActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_CustomInputActionsCallbackInterfaces.Add(instance);
				Escape.started += instance.OnEscape;
				Escape.performed += instance.OnEscape;
				Escape.canceled += instance.OnEscape;
				Next.started += instance.OnNext;
				Next.performed += instance.OnNext;
				Next.canceled += instance.OnNext;
				Accept.started += instance.OnAccept;
				Accept.performed += instance.OnAccept;
				Accept.canceled += instance.OnAccept;
				Up.started += instance.OnUp;
				Up.performed += instance.OnUp;
				Up.canceled += instance.OnUp;
				Down.started += instance.OnDown;
				Down.performed += instance.OnDown;
				Down.canceled += instance.OnDown;
				Left.started += instance.OnLeft;
				Left.performed += instance.OnLeft;
				Left.canceled += instance.OnLeft;
				Right.started += instance.OnRight;
				Right.performed += instance.OnRight;
				Right.canceled += instance.OnRight;
				Find.started += instance.OnFind;
				Find.performed += instance.OnFind;
				Find.canceled += instance.OnFind;
				AltViewOpen.started += instance.OnAltViewOpen;
				AltViewOpen.performed += instance.OnAltViewOpen;
				AltViewOpen.canceled += instance.OnAltViewOpen;
				AltViewClose.started += instance.OnAltViewClose;
				AltViewClose.performed += instance.OnAltViewClose;
				AltViewClose.canceled += instance.OnAltViewClose;
			}
		}

		private void UnregisterCallbacks(ICustomInputActions instance)
		{
			Escape.started -= instance.OnEscape;
			Escape.performed -= instance.OnEscape;
			Escape.canceled -= instance.OnEscape;
			Next.started -= instance.OnNext;
			Next.performed -= instance.OnNext;
			Next.canceled -= instance.OnNext;
			Accept.started -= instance.OnAccept;
			Accept.performed -= instance.OnAccept;
			Accept.canceled -= instance.OnAccept;
			Up.started -= instance.OnUp;
			Up.performed -= instance.OnUp;
			Up.canceled -= instance.OnUp;
			Down.started -= instance.OnDown;
			Down.performed -= instance.OnDown;
			Down.canceled -= instance.OnDown;
			Left.started -= instance.OnLeft;
			Left.performed -= instance.OnLeft;
			Left.canceled -= instance.OnLeft;
			Right.started -= instance.OnRight;
			Right.performed -= instance.OnRight;
			Right.canceled -= instance.OnRight;
			Find.started -= instance.OnFind;
			Find.performed -= instance.OnFind;
			Find.canceled -= instance.OnFind;
			AltViewOpen.started -= instance.OnAltViewOpen;
			AltViewOpen.performed -= instance.OnAltViewOpen;
			AltViewOpen.canceled -= instance.OnAltViewOpen;
			AltViewClose.started -= instance.OnAltViewClose;
			AltViewClose.performed -= instance.OnAltViewClose;
			AltViewClose.canceled -= instance.OnAltViewClose;
		}

		public void RemoveCallbacks(ICustomInputActions instance)
		{
			if (m_Wrapper.m_CustomInputActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(ICustomInputActions instance)
		{
			foreach (ICustomInputActions customInputActionsCallbackInterface in m_Wrapper.m_CustomInputActionsCallbackInterfaces)
			{
				UnregisterCallbacks(customInputActionsCallbackInterface);
			}
			m_Wrapper.m_CustomInputActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public interface IUIActions
	{
		void OnNavigate(InputAction.CallbackContext context);

		void OnSubmit(InputAction.CallbackContext context);

		void OnCancel(InputAction.CallbackContext context);

		void OnPoint(InputAction.CallbackContext context);

		void OnClick(InputAction.CallbackContext context);

		void OnScrollWheel(InputAction.CallbackContext context);

		void OnMiddleClick(InputAction.CallbackContext context);

		void OnRightClick(InputAction.CallbackContext context);
	}

	public interface IDebugActions
	{
		void OnToggleDebugMenu(InputAction.CallbackContext context);

		void OnOpenDebugMenu(InputAction.CallbackContext context);

		void OnCloseDebugMenu(InputAction.CallbackContext context);
	}

	public interface ICustomInputActions
	{
		void OnEscape(InputAction.CallbackContext context);

		void OnNext(InputAction.CallbackContext context);

		void OnAccept(InputAction.CallbackContext context);

		void OnUp(InputAction.CallbackContext context);

		void OnDown(InputAction.CallbackContext context);

		void OnLeft(InputAction.CallbackContext context);

		void OnRight(InputAction.CallbackContext context);

		void OnFind(InputAction.CallbackContext context);

		void OnAltViewOpen(InputAction.CallbackContext context);

		void OnAltViewClose(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();

	private readonly InputAction m_UI_Navigate;

	private readonly InputAction m_UI_Submit;

	private readonly InputAction m_UI_Cancel;

	private readonly InputAction m_UI_Point;

	private readonly InputAction m_UI_Click;

	private readonly InputAction m_UI_ScrollWheel;

	private readonly InputAction m_UI_MiddleClick;

	private readonly InputAction m_UI_RightClick;

	private readonly InputActionMap m_Debug;

	private List<IDebugActions> m_DebugActionsCallbackInterfaces = new List<IDebugActions>();

	private readonly InputAction m_Debug_ToggleDebugMenu;

	private readonly InputAction m_Debug_OpenDebugMenu;

	private readonly InputAction m_Debug_CloseDebugMenu;

	private readonly InputActionMap m_CustomInput;

	private List<ICustomInputActions> m_CustomInputActionsCallbackInterfaces = new List<ICustomInputActions>();

	private readonly InputAction m_CustomInput_Escape;

	private readonly InputAction m_CustomInput_Next;

	private readonly InputAction m_CustomInput_Accept;

	private readonly InputAction m_CustomInput_Up;

	private readonly InputAction m_CustomInput_Down;

	private readonly InputAction m_CustomInput_Left;

	private readonly InputAction m_CustomInput_Right;

	private readonly InputAction m_CustomInput_Find;

	private readonly InputAction m_CustomInput_AltViewOpen;

	private readonly InputAction m_CustomInput_AltViewClose;

	private int m_KeyboardMouseSchemeIndex = -1;

	private int m_GamepadSchemeIndex = -1;

	private int m_TouchSchemeIndex = -1;

	private int m_JoystickSchemeIndex = -1;

	private int m_XRSchemeIndex = -1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public UIActions UI => new UIActions(this);

	public DebugActions Debug => new DebugActions(this);

	public CustomInputActions CustomInput => new CustomInputActions(this);

	public InputControlScheme KeyboardMouseScheme
	{
		get
		{
			if (m_KeyboardMouseSchemeIndex == -1)
			{
				m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
			}
			return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
		}
	}

	public InputControlScheme GamepadScheme
	{
		get
		{
			if (m_GamepadSchemeIndex == -1)
			{
				m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
			}
			return asset.controlSchemes[m_GamepadSchemeIndex];
		}
	}

	public InputControlScheme TouchScheme
	{
		get
		{
			if (m_TouchSchemeIndex == -1)
			{
				m_TouchSchemeIndex = asset.FindControlSchemeIndex("Touch");
			}
			return asset.controlSchemes[m_TouchSchemeIndex];
		}
	}

	public InputControlScheme JoystickScheme
	{
		get
		{
			if (m_JoystickSchemeIndex == -1)
			{
				m_JoystickSchemeIndex = asset.FindControlSchemeIndex("Joystick");
			}
			return asset.controlSchemes[m_JoystickSchemeIndex];
		}
	}

	public InputControlScheme XRScheme
	{
		get
		{
			if (m_XRSchemeIndex == -1)
			{
				m_XRSchemeIndex = asset.FindControlSchemeIndex("XR");
			}
			return asset.controlSchemes[m_XRSchemeIndex];
		}
	}

	public MTGAInput()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"name\": \"MTGA\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"UI\",\r\n            \"id\": \"6579eee5-2ec3-414b-b7b3-18a17003f251\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Navigate\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"9ea41f84-724a-4cbf-8f95-a7efb1f0ded6\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Submit\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"61b75a88-073a-46b3-ac94-6b954dc3a97a\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Cancel\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"2d5312f1-9437-46e5-bca2-39cbd573dd21\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Point\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"be160501-a223-456d-8e6e-021bf4711e37\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Click\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"10bf53f1-4f6f-4e27-bbde-ae5dcb483f69\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"ScrollWheel\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"74f34e5a-8342-4b9d-b5d9-a463d85e3d96\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MiddleClick\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"1d57f601-9e72-41c8-91a2-294127d34531\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RightClick\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"6c7834b4-8204-40ce-bae8-3ffcc55e10c6\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"Gamepad\",\r\n                    \"id\": \"809f371f-c5e2-4e7a-83a1-d867598f40dd\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"14a5d6e8-4aaf-4119-a9ef-34b8c2c548bf\",\r\n                    \"path\": \"<Gamepad>/leftStick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"9144cbe6-05e1-4687-a6d7-24f99d23dd81\",\r\n                    \"path\": \"<Gamepad>/rightStick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"2db08d65-c5fb-421b-983f-c71163608d67\",\r\n                    \"path\": \"<Gamepad>/leftStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"58748904-2ea9-4a80-8579-b500e6a76df8\",\r\n                    \"path\": \"<Gamepad>/rightStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"8ba04515-75aa-45de-966d-393d9bbd1c14\",\r\n                    \"path\": \"<Gamepad>/leftStick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"712e721c-bdfb-4b23-a86c-a0d9fcfea921\",\r\n                    \"path\": \"<Gamepad>/rightStick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"fcd248ae-a788-4676-a12e-f4d81205600b\",\r\n                    \"path\": \"<Gamepad>/leftStick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"1f04d9bc-c50b-41a1-bfcc-afb75475ec20\",\r\n                    \"path\": \"<Gamepad>/rightStick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fb8277d4-c5cd-4663-9dc7-ee3f0b506d90\",\r\n                    \"path\": \"<Gamepad>/dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Joystick\",\r\n                    \"id\": \"e25d9774-381c-4a61-b47c-7b6b299ad9f9\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"3db53b26-6601-41be-9887-63ac74e79d19\",\r\n                    \"path\": \"<Joystick>/stick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"0cb3e13e-3d90-4178-8ae6-d9c5501d653f\",\r\n                    \"path\": \"<Joystick>/stick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"0392d399-f6dd-4c82-8062-c1e9c0d34835\",\r\n                    \"path\": \"<Joystick>/stick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"942a66d9-d42f-43d6-8d70-ecb4ba5363bc\",\r\n                    \"path\": \"<Joystick>/stick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"Keyboard\",\r\n                    \"id\": \"ff527021-f211-4c02-933e-5976594c46ed\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"563fbfdd-0f09-408d-aa75-8642c4f08ef0\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"eb480147-c587-4a33-85ed-eb0ab9942c43\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"2bf42165-60bc-42ca-8072-8c13ab40239b\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"85d264ad-e0a0-4565-b7ff-1a37edde51ac\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"74214943-c580-44e4-98eb-ad7eebe17902\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"cea9b045-a000-445b-95b8-0c171af70a3b\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"8607c725-d935-4808-84b1-8354e29bab63\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"4cda81dc-9edd-4e03-9d7c-a71a14345d0b\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9e92bb26-7e3b-4ec4-b06b-3c8f8e498ddc\",\r\n                    \"path\": \"*/{Submit}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse;Gamepad;Touch;Joystick;XR\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"82627dcc-3b13-4ba9-841d-e4b746d6553e\",\r\n                    \"path\": \"*/{Cancel}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse;Gamepad;Touch;Joystick;XR\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c52c8e0b-8179-41d3-b8a1-d149033bbe86\",\r\n                    \"path\": \"<Mouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e1394cbc-336e-44ce-9ea8-6007ed6193f7\",\r\n                    \"path\": \"<Pen>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5693e57a-238a-46ed-b5ae-e64e6e574302\",\r\n                    \"path\": \"<Touchscreen>/touch*/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Touch\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4faf7dc9-b979-4210-aa8c-e808e1ef89f5\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8d66d5ba-88d7-48e6-b1cd-198bbfef7ace\",\r\n                    \"path\": \"<Pen>/tip\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"47c2a644-3ebc-4dae-a106-589b7ca75b59\",\r\n                    \"path\": \"<Touchscreen>/touch*/press\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Touch\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"bb9e6b34-44bf-4381-ac63-5aa15d19f677\",\r\n                    \"path\": \"<XRController>/trigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"XR\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"38c99815-14ea-4617-8627-164d27641299\",\r\n                    \"path\": \"<Mouse>/scroll\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"ScrollWheel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"24066f69-da47-44f3-a07e-0015fb02eb2e\",\r\n                    \"path\": \"<Mouse>/middleButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"MiddleClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4c191405-5738-4d4b-a523-c6a301dbf754\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"RightClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Debug\",\r\n            \"id\": \"520a0286-e03d-4b7a-a577-7bd87d4bb56a\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"ToggleDebugMenu\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"7056fd78-2aaa-45da-80a8-8ddebaece26f\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"OpenDebugMenu\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"b109790b-3d23-4d8b-8329-17f4071b314f\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"CloseDebugMenu\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"119c66be-7b3d-4b39-b593-0754b0e4aae5\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"Touch\",\r\n                    \"id\": \"e9f528b7-a2e2-4b89-ac22-1110d53e92f4\",\r\n                    \"path\": \"TwoModifiers\",\r\n                    \"interactions\": \"Tap\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"ToggleDebugMenu\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"modifier1\",\r\n                    \"id\": \"55ad0892-7b9c-4a8c-b511-ab7cba329cac\",\r\n                    \"path\": \"<Touchscreen>/touch1/press\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Touch\",\r\n                    \"action\": \"ToggleDebugMenu\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"modifier2\",\r\n                    \"id\": \"71469c42-d79c-40ef-b5ea-4c0f5ccec15d\",\r\n                    \"path\": \"<Touchscreen>/touch0/press\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Touch\",\r\n                    \"action\": \"ToggleDebugMenu\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"binding\",\r\n                    \"id\": \"da9d4879-a3ed-43f0-8f34-badbfddb6e60\",\r\n                    \"path\": \"<Touchscreen>/touch2/press\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Touch\",\r\n                    \"action\": \"ToggleDebugMenu\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"60a960cc-85a2-4ca4-a13d-b1977f86c455\",\r\n                    \"path\": \"<Keyboard>/leftAlt\",\r\n                    \"interactions\": \"Press\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"OpenDebugMenu\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c9bbebef-9433-456d-9f9c-c0e59a468d89\",\r\n                    \"path\": \"<Keyboard>/leftAlt\",\r\n                    \"interactions\": \"Press(behavior=1)\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"CloseDebugMenu\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"CustomInput\",\r\n            \"id\": \"9168d3aa-fa5f-4f9c-a9d1-17837bf380e8\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Escape\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"75b120dc-e6ca-4350-b1b0-02908054b05e\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Next\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"8175bcf2-5225-4f9a-825f-f96ec217f8ac\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Accept\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"12c21499-18ff-45a1-9eb0-858557f0774d\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Up\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"f78e2b58-9edb-46a9-b4dc-2171166d574a\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Down\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"bd2ff261-1b37-44f3-a008-58f3aa5b358e\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Left\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"42e7d383-0304-43bf-a8de-7ed168c9075e\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Right\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"8eb21b69-526a-4cda-816f-543ec2541bed\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Find\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"d1c0b650-b7aa-475d-9eb0-518cc4b07a61\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"AltViewOpen\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"6d6ba0b8-fb8e-4c85-9dbb-0f2913fc2abf\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"AltViewClose\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"f4706d54-95a8-48fa-9164-515cadadd661\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3edd7b07-46bb-4072-bb28-5281d1f1d2fb\",\r\n                    \"path\": \"*/{Back}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse;Gamepad;Touch;Joystick;XR\",\r\n                    \"action\": \"Escape\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4ba58190-42fa-4168-8991-39da950957ce\",\r\n                    \"path\": \"<Keyboard>/tab\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Next\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"742a6dee-f20a-478f-91c6-dc137b59e706\",\r\n                    \"path\": \"<Keyboard>/enter\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Accept\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"981f727c-4b4c-41b2-b687-1e60c6ded87b\",\r\n                    \"path\": \"<Keyboard>/numpadEnter\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Accept\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8f79d271-3c03-4d0a-8f13-a70df0e5ad8d\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Up\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2f1553fe-c001-4ccc-b727-ac5fce133bbc\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Down\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"52f326c7-8d3c-4b3b-b0b9-48113a7e63ec\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Left\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4731fae3-6d8e-42af-894a-dd5dbd48dd61\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Right\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"ctrlf\",\r\n                    \"id\": \"82f1c666-7a3b-402e-a0f8-2e79c2edf628\",\r\n                    \"path\": \"OneModifier\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Find\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"modifier\",\r\n                    \"id\": \"32fd7fc8-48b1-4a7f-8b90-f2623b636a4a\",\r\n                    \"path\": \"<Keyboard>/ctrl\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Find\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"binding\",\r\n                    \"id\": \"e5d944c0-c3bd-4edd-98a8-6e47142058b5\",\r\n                    \"path\": \"<Keyboard>/f\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Find\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b74f7c16-c771-4356-9648-6692356ea6a1\",\r\n                    \"path\": \"<Keyboard>/f3\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Find\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"47b21ab5-c00b-4bee-80f3-aa439a63dcb5\",\r\n                    \"path\": \"<Keyboard>/alt\",\r\n                    \"interactions\": \"Press\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"AltViewOpen\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6090858c-5438-4cbc-be13-2618b481b8e6\",\r\n                    \"path\": \"<Keyboard>/alt\",\r\n                    \"interactions\": \"Press(behavior=1)\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"AltViewClose\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": [\r\n        {\r\n            \"name\": \"Keyboard&Mouse\",\r\n            \"bindingGroup\": \"Keyboard&Mouse\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Keyboard>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                },\r\n                {\r\n                    \"devicePath\": \"<Mouse>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Gamepad\",\r\n            \"bindingGroup\": \"Gamepad\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Gamepad>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Touch\",\r\n            \"bindingGroup\": \"Touch\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Touchscreen>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Joystick\",\r\n            \"bindingGroup\": \"Joystick\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Joystick>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"XR\",\r\n            \"bindingGroup\": \"XR\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<XRController>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}");
		m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
		m_UI_Navigate = m_UI.FindAction("Navigate", throwIfNotFound: true);
		m_UI_Submit = m_UI.FindAction("Submit", throwIfNotFound: true);
		m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
		m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
		m_UI_Click = m_UI.FindAction("Click", throwIfNotFound: true);
		m_UI_ScrollWheel = m_UI.FindAction("ScrollWheel", throwIfNotFound: true);
		m_UI_MiddleClick = m_UI.FindAction("MiddleClick", throwIfNotFound: true);
		m_UI_RightClick = m_UI.FindAction("RightClick", throwIfNotFound: true);
		m_Debug = asset.FindActionMap("Debug", throwIfNotFound: true);
		m_Debug_ToggleDebugMenu = m_Debug.FindAction("ToggleDebugMenu", throwIfNotFound: true);
		m_Debug_OpenDebugMenu = m_Debug.FindAction("OpenDebugMenu", throwIfNotFound: true);
		m_Debug_CloseDebugMenu = m_Debug.FindAction("CloseDebugMenu", throwIfNotFound: true);
		m_CustomInput = asset.FindActionMap("CustomInput", throwIfNotFound: true);
		m_CustomInput_Escape = m_CustomInput.FindAction("Escape", throwIfNotFound: true);
		m_CustomInput_Next = m_CustomInput.FindAction("Next", throwIfNotFound: true);
		m_CustomInput_Accept = m_CustomInput.FindAction("Accept", throwIfNotFound: true);
		m_CustomInput_Up = m_CustomInput.FindAction("Up", throwIfNotFound: true);
		m_CustomInput_Down = m_CustomInput.FindAction("Down", throwIfNotFound: true);
		m_CustomInput_Left = m_CustomInput.FindAction("Left", throwIfNotFound: true);
		m_CustomInput_Right = m_CustomInput.FindAction("Right", throwIfNotFound: true);
		m_CustomInput_Find = m_CustomInput.FindAction("Find", throwIfNotFound: true);
		m_CustomInput_AltViewOpen = m_CustomInput.FindAction("AltViewOpen", throwIfNotFound: true);
		m_CustomInput_AltViewClose = m_CustomInput.FindAction("AltViewClose", throwIfNotFound: true);
	}

	~MTGAInput()
	{
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
