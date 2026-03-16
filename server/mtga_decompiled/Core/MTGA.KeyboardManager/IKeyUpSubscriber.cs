using UnityEngine;

namespace MTGA.KeyboardManager;

public interface IKeyUpSubscriber : IKeySubscriber
{
	bool HandleKeyUp(KeyCode curr, Modifiers mods);
}
