using System.Collections.Generic;
using System.Linq;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene;

public class YouCountConfigValidator : IMatchConfigValidator
{
	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(MatchConfig matchConfig)
	{
		int num = matchConfig.Teams.Sum((TeamConfig teamConfig) => teamConfig.Players.Count((PlayerConfig playerConfig) => playerConfig.PlayerType == PlayerType.You));
		if (num > 1)
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: "Too many players with setup with 'PlayerType.You'.  There must be exactly 1 of these players to create a match");
		}
		else if (num == 0)
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: "No players are listed as 'PlayerType.You'.  There must be exactly 1 of these players to create a match");
		}
	}
}
