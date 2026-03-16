using System.Collections.Generic;

namespace Core.MainNavigation.RewardTrack;

public class ProgressionTrackLevel
{
	public ClientTrackLevelInfo ServerLevel;

	public List<ClientTrackRewardInfo> ServerRewardTiers;

	public int Index;

	public int RawLevel;

	public int EXPProgressIfIsCurrent;

	public bool IsProgressionComplete;

	public bool IsRepeatable;

	public string LevelId;
}
