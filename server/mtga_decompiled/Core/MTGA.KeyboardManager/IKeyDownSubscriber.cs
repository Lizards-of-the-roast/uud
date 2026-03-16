using UnityEngine;

namespace MTGA.KeyboardManager;

public interface IKeyDownSubscriber : IKeySubscriber
{
	bool HandleKeyDown(KeyCode curr, Modifiers mods);
}
