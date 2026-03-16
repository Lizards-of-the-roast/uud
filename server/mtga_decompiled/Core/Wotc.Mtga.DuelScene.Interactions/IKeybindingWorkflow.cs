using UnityEngine;

namespace Wotc.Mtga.DuelScene.Interactions;

public interface IKeybindingWorkflow
{
	bool CanKeyUp(KeyCode key);

	void OnKeyUp(KeyCode key);

	bool CanKeyDown(KeyCode key);

	void OnKeyDown(KeyCode key);

	bool CanKeyHeld(KeyCode key, float holdDuration);

	void OnKeyHeld(KeyCode key, float holdDuration);
}
