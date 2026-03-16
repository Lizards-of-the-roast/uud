using System;

public interface IBrowser
{
	event Action ClosedHandlers;

	event Action ShownHandlers;

	event Action HiddenHandlers;

	event Action<string> ButtonPressedHandlers;

	void Close();
}
