using System;

namespace Core.Code.Input;

public interface IInputHandler : IDisposable
{
	event Action? Accept;

	event Action? Next;

	event Action? Previous;

	event Action? Back;

	event Action? Find;

	event Action? AltViewOpen;

	event Action? AltViewClose;

	event Action<Direction>? Navigate;

	event Action<char>? TextInput;

	event Action DebugToggle;

	event Action DebugOpen;

	event Action DebugClose;

	void Update();
}
