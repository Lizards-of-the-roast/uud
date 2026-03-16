using UnityEngine;

namespace Core.Shared.Code.DebugTools;

public static class RecordHistoryUtils
{
	public static bool ShouldRecordHistory
	{
		get
		{
			if (!Debug.isDebugBuild)
			{
				return MDNPlayerPrefs.RecordServerMessageHistory;
			}
			return true;
		}
	}
}
