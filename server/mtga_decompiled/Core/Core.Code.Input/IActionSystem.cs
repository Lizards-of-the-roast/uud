using System;

namespace Core.Code.Input;

public interface IActionSystem : IDisposable
{
	public class Debug
	{
		public Action DebugToggle;

		public Action DebugOpen;

		public Action DebugClose;
	}

	public enum Priority
	{
		SystemMessage = -3,
		Settings,
		PopUp,
		Normal
	}

	Debug DebugActions { get; }

	void PushFocus(object obj, Priority priority = Priority.Normal);

	void PopFocus(object obj);

	bool IsCurrentFocus(object obj);

	void Update();

	void DisableLogs();
}
