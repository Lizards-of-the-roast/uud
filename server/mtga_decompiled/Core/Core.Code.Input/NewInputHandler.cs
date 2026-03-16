using System;
using Core.Code.Input.Generated;
using UnityEngine.InputSystem;

namespace Core.Code.Input;

public class NewInputHandler : IInputHandler, IDisposable, MTGAInput.IDebugActions, MTGAInput.ICustomInputActions
{
	private readonly MTGAInput _MTGAInputs;

	public event Action? Accept;

	public event Action? Next;

	public event Action? Previous;

	public event Action? Back;

	public event Action? Find;

	public event Action? AltViewOpen;

	public event Action? AltViewClose;

	public event Action<Direction>? Navigate;

	public event Action<char>? TextInput;

	public event Action? DebugToggle;

	public event Action? DebugOpen;

	public event Action? DebugClose;

	public NewInputHandler()
	{
		_MTGAInputs = new MTGAInput();
		_MTGAInputs.Debug.Enable();
		_MTGAInputs.Debug.SetCallbacks(this);
		_MTGAInputs.CustomInput.Enable();
		_MTGAInputs.CustomInput.SetCallbacks(this);
		if (Keyboard.current != null)
		{
			Keyboard.current.onTextInput += OnTextInput;
		}
	}

	public void Update()
	{
	}

	public void Dispose()
	{
		if (Keyboard.current != null)
		{
			Keyboard.current.onTextInput -= OnTextInput;
		}
		_MTGAInputs.Dispose();
	}

	public void OnToggleDebugMenu(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			this.DebugToggle?.Invoke();
		}
	}

	public void OnOpenDebugMenu(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			this.DebugOpen?.Invoke();
		}
	}

	public void OnCloseDebugMenu(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			this.DebugClose?.Invoke();
		}
	}

	public void OnTextInput(char c)
	{
		this.TextInput?.Invoke(c);
	}

	public void OnEscape(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			this.Back?.Invoke();
		}
	}

	public void OnNext(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Keyboard current = Keyboard.current;
			if (current != null && current.shiftKey.isPressed)
			{
				this.Previous?.Invoke();
			}
			else
			{
				this.Next?.Invoke();
			}
		}
	}

	public void OnAccept(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			this.Accept?.Invoke();
		}
	}

	private void Internal_OnNavigate(InputAction.CallbackContext context, Direction dir)
	{
		if (context.performed)
		{
			this.Navigate?.Invoke(dir);
		}
	}

	public void OnUp(InputAction.CallbackContext context)
	{
		Internal_OnNavigate(context, Direction.Up);
	}

	public void OnDown(InputAction.CallbackContext context)
	{
		Internal_OnNavigate(context, Direction.Down);
	}

	public void OnLeft(InputAction.CallbackContext context)
	{
		Internal_OnNavigate(context, Direction.Left);
	}

	public void OnRight(InputAction.CallbackContext context)
	{
		Internal_OnNavigate(context, Direction.Right);
	}

	public void OnFind(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			this.Find?.Invoke();
		}
	}

	public void OnAltViewOpen(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			this.AltViewOpen?.Invoke();
		}
	}

	public void OnAltViewClose(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			this.AltViewClose?.Invoke();
		}
	}
}
