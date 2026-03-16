using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class GenericInfoModule : DebugModule
{
	public override string Name => "Hotkey Information";

	public override string Description => string.Empty;

	public override void Render()
	{
		GUILayout.Label("ActionsAvailable Hotkeys");
		GUILayout.Space(3f);
		GUILayout.Label("W: Waive Cost (When paying costs)");
		GUILayout.Label("W: Wish");
		GUILayout.Label("A: Add Colors");
		GUILayout.Label("D: Draw Card");
		GUILayout.Label("H: Gain Haste");
		GUILayout.Label("N: Donate");
		GUILayout.Label("B: Bounce to Hand");
		GUILayout.Label("P: Put top of library");
		GUILayout.Label("T: Tap/Untap");
		GUILayout.Label("G: Gild");
	}
}
