using System.Collections.Generic;
using System.Linq;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene;

public class PlayerVsPlayerConfigValidator : IMatchConfigValidator
{
	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(MatchConfig matchConfig)
	{
		if (matchConfig.Teams.Sum((TeamConfig teamConfig) => teamConfig.Players.Count((PlayerConfig x) => x.PlayerType == PlayerType.Human)) == 0)
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: "No Human players present, cannot create a human vs human match");
		}
	}
}
