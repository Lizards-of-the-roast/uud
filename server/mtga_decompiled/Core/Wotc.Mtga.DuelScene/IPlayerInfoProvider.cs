using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface IPlayerInfoProvider
{
	MatchManager.PlayerInfo GetPlayerInfo(uint seatId);

	IEnumerable<MatchManager.PlayerInfo> GetAllPlayerInfo();

	bool TryGetPlayerInfo(uint seatId, out MatchManager.PlayerInfo result)
	{
		result = GetPlayerInfo(seatId);
		return result != null;
	}

	string GetAvatarSelectionForPlayer(uint seatId)
	{
		if (!TryGetPlayerInfo(seatId, out var result))
		{
			return string.Empty;
		}
		return result.AvatarSelection;
	}

	string GetSleeveForPlayer(uint seatId)
	{
		if (!TryGetPlayerInfo(seatId, out var result))
		{
			return string.Empty;
		}
		return result.SleeveSelection;
	}

	string GetScreenNameForPlayer(uint seatId)
	{
		if (!TryGetPlayerInfo(seatId, out var result))
		{
			return string.Empty;
		}
		return result.ScreenName;
	}
}
