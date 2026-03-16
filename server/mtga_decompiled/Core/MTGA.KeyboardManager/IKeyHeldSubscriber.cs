using UnityEngine;

namespace MTGA.KeyboardManager;

public interface IKeyHeldSubscriber : IKeySubscriber
{
	bool HandleKeyHeld(KeyCode key, float holdDuration);
}
