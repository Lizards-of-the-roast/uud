using UnityEngine;

namespace Core.VideoScene;

public static class IosAudioSession
{
	public static void ForcePlayback()
	{
		Debug.Log("[IosAudioSession] ForcePlayback() called (non-iOS or Editor) — no-op.");
	}

	public static string GetCurrentCategory()
	{
		return "(Editor/Non-iOS)";
	}
}
