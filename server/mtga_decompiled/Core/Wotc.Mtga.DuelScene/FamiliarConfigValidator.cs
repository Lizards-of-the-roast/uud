using System.Collections.Generic;
using System.Linq;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene;

public class FamiliarConfigValidator : IMatchConfigValidator
{
	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(MatchConfig matchConfig)
	{
		if (matchConfig.Teams.Sum((TeamConfig teamConfig) => teamConfig.Players.Count((PlayerConfig playerConfig) => playerConfig.PlayerType == PlayerType.Human)) > 0)
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: "Non-AI players present, cannot create a strictly AI match");
		}
		if (matchConfig.Teams.Sum((TeamConfig teamConfig) => teamConfig.Players.Count((PlayerConfig playerConfig) => playerConfig.PlayerType == PlayerType.Bot)) == 0)
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: "No AI players present, cannot play against familiar");
		}
	}
}
