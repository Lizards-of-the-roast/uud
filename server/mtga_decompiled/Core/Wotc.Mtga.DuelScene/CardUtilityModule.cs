using System;
using System.Linq;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class CardUtilityModule : DebugModule
{
	public override string Name => "Card Utility";

	public override string Description => "Exposed controls for exposing certain behaviors on cards";

	public override void Render()
	{
		ForceUpdateCDCButton();
		ForceCDCHighlightsButton();
	}

	private static void ForceCDCHighlightsButton()
	{
		GUILayout.Label("Force CDC Highlights:");
		GUILayout.BeginVertical(GUI.skin.box);
		HighlightType[] array = Enum.GetValues(typeof(HighlightType)).Cast<HighlightType>().ToArray();
		string[] texts = array.Select((HighlightType x) => x.ToString()).ToArray();
		int num = GUILayout.SelectionGrid(-1, texts, 4);
		if (num >= 0)
		{
			HighlightType highlightType = array[num];
			BASE_CDC[] array2 = UnityEngine.Object.FindObjectsOfType<BASE_CDC>();
			foreach (BASE_CDC bASE_CDC in array2)
			{
				if (bASE_CDC.gameObject.activeInHierarchy)
				{
					bASE_CDC.UpdateHighlight(highlightType);
				}
			}
		}
		GUILayout.EndVertical();
	}

	private static void ForceUpdateCDCButton()
	{
		if (!GUILayout.Button("Force CDC Update"))
		{
			return;
		}
		BASE_CDC[] array = UnityEngine.Object.FindObjectsOfType<BASE_CDC>();
		foreach (BASE_CDC bASE_CDC in array)
		{
			if (bASE_CDC.gameObject.activeInHierarchy)
			{
				bASE_CDC.UpdateVisuals();
			}
		}
	}
}
