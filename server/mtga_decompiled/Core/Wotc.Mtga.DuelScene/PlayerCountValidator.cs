using System.Collections.Generic;
using System.Linq;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene;

public class PlayerCountValidator : IMatchConfigValidator
{
	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(MatchConfig matchConfig)
	{
		int count = matchConfig.Teams.Count;
		if (count < 2)
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: $"Invalid team count ({count})");
		}
		int num = matchConfig.Teams.Sum((TeamConfig x) => x.Players.Count);
		if (num < 2)
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: $"Invalid player count ({num})");
		}
	}
}
