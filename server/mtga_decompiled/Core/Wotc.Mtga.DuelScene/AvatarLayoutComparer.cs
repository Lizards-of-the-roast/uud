using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class AvatarLayoutComparer : IComparer<IAvatarView>
{
	private readonly IComparer<uint> _relativeTeamComparer;

	private readonly IComparer<uint> _relativeSeatComparer;

	public AvatarLayoutComparer(uint myTeamId, uint mySeatId)
	{
		_relativeTeamComparer = new RelativeIdComparer(myTeamId);
		_relativeSeatComparer = new RelativeIdComparer(mySeatId);
	}

	public AvatarLayoutComparer(MatchManager.PlayerInfo localPlayerInfo)
		: this(localPlayerInfo.TeamId, localPlayerInfo.SeatId)
	{
	}

	public int Compare(IAvatarView x, IAvatarView y)
	{
		MtgPlayer model = x.Model;
		uint teamId = model.Team.TeamId;
		MtgPlayer model2 = y.Model;
		uint teamId2 = model2.Team.TeamId;
		int num = _relativeTeamComparer.Compare(teamId2, teamId);
		if (num != 0)
		{
			return num;
		}
		return _relativeSeatComparer.Compare(model2.InstanceId, model.InstanceId);
	}
}
