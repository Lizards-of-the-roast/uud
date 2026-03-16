using System;
using UnityEngine;

namespace Core.Code.Input;

public class OldInputHandler : IInputHandler, IDisposable
{
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

	public void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.LeftAlt) || UnityEngine.Input.GetKeyDown(KeyCode.RightAlt))
		{
			this.DebugOpen?.Invoke();
		}
		if (UnityEngine.Input.GetKeyUp(KeyCode.LeftAlt) || UnityEngine.Input.GetKeyUp(KeyCode.RightAlt))
		{
			this.DebugClose?.Invoke();
		}
		if (UnityEngine.Input.touchCount >= 3 && UnityEngine.Input.touches[2].phase == TouchPhase.Began)
		{
			this.DebugToggle?.Invoke();
		}
		if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
		{
			this.Navigate?.Invoke(Direction.Left);
		}
		if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
		{
			this.Navigate?.Invoke(Direction.Right);
		}
		if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
		{
			this.Navigate?.Invoke(Direction.Up);
		}
		if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
		{
			this.Navigate?.Invoke(Direction.Down);
		}
		if (UnityEngine.Input.GetKeyDown(KeyCode.Return) || UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			this.Accept?.Invoke();
		}
		if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
		{
			if (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))
			{
				this.Previous?.Invoke();
			}
			else
			{
				this.Next?.Invoke();
			}
		}
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			this.Back?.Invoke();
		}
		if (UnityEngine.Input.anyKey && UnityEngine.Input.inputString.Length > 0)
		{
			string inputString = UnityEngine.Input.inputString;
			foreach (char obj in inputString)
			{
				this.TextInput?.Invoke(obj);
			}
		}
		bool flag = UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl);
		if (UnityEngine.Input.GetKeyDown(KeyCode.F3) || (flag && UnityEngine.Input.GetKeyDown(KeyCode.F)))
		{
			this.Find?.Invoke();
		}
		if (UnityEngine.Input.GetKeyDown(KeyCode.LeftAlt) || UnityEngine.Input.GetKeyDown(KeyCode.RightAlt))
		{
			this.AltViewOpen?.Invoke();
		}
		if (UnityEngine.Input.GetKeyUp(KeyCode.LeftAlt) || UnityEngine.Input.GetKeyUp(KeyCode.RightAlt))
		{
			this.AltViewClose?.Invoke();
		}
	}

	public void Dispose()
	{
	}
}
