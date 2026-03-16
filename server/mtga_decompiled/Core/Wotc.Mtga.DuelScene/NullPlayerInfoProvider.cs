using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullPlayerInfoProvider : IPlayerInfoProvider
{
	public static readonly IPlayerInfoProvider Default = new NullPlayerInfoProvider();

	public MatchManager.PlayerInfo GetPlayerInfo(uint seatId)
	{
		return null;
	}

	public IEnumerable<MatchManager.PlayerInfo> GetAllPlayerInfo()
	{
		return Array.Empty<MatchManager.PlayerInfo>();
	}
}
