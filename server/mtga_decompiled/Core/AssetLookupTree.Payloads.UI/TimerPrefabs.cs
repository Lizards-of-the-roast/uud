using System.Collections.Generic;

namespace AssetLookupTree.Payloads.UI;

public class TimerPrefabs : IPayload
{
	public readonly AltAssetReference<TimeoutNotificationDisplay> TimeoutVisualsRef = new AltAssetReference<TimeoutNotificationDisplay>();

	public readonly AltAssetReference<PlayerTimeoutDisplay> LocalPlayerTimeoutDisplayRef = new AltAssetReference<PlayerTimeoutDisplay>();

	public readonly AltAssetReference<MatchTimer> LocalPlayerMatchTimerRef = new AltAssetReference<MatchTimer>();

	public readonly AltAssetReference<PlayerTimeoutDisplay> OpponentTimeoutDisplayRef = new AltAssetReference<PlayerTimeoutDisplay>();

	public readonly AltAssetReference<MatchTimer> OpponentMatchTimerRef = new AltAssetReference<MatchTimer>();

	public readonly AltAssetReference<LowTimeWarning> LocalPlayerLowTimeWarningRef = new AltAssetReference<LowTimeWarning>();

	public readonly AltAssetReference<LowTimeWarning> OpponentLowTimeWarningRef = new AltAssetReference<LowTimeWarning>();

	public readonly AltAssetReference<LowTimeFrameGlow> LowTimeFrameGlowRef = new AltAssetReference<LowTimeFrameGlow>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return TimeoutVisualsRef.RelativePath;
		yield return LocalPlayerTimeoutDisplayRef.RelativePath;
		yield return LocalPlayerMatchTimerRef.RelativePath;
		yield return OpponentTimeoutDisplayRef.RelativePath;
		yield return OpponentMatchTimerRef.RelativePath;
		yield return OpponentLowTimeWarningRef.RelativePath;
		yield return LocalPlayerLowTimeWarningRef.RelativePath;
		yield return LowTimeFrameGlowRef.RelativePath;
	}
}
