using UnityEngine;

namespace Wotc.Mtga.Extensions;

public static class CanvasGroupExtensions
{
	public static void UpdateActive(this CanvasGroup canvasGroup, bool active)
	{
		canvasGroup.alpha = (active ? 1f : 0f);
		bool blocksRaycasts = (canvasGroup.interactable = active);
		canvasGroup.blocksRaycasts = blocksRaycasts;
	}
}
