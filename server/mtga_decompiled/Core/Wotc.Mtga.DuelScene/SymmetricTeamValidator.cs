using System.Collections.Generic;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene;

public class SymmetricTeamValidator : IMatchConfigValidator
{
	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(MatchConfig matchConfig)
	{
		if (matchConfig.Teams.Count <= 1)
		{
			yield break;
		}
		int count = matchConfig.Teams[0].Players.Count;
		for (int i = 1; i < matchConfig.Teams.Count; i++)
		{
			if (matchConfig.Teams[i].Players.Count != count)
			{
				yield return (resultType: IMatchConfigValidator.Result.Error, reason: "Asymmetric player count");
				break;
			}
		}
	}
}
