using System.Collections.Generic;
using GreClient.Network;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class GameTypetValidator : IMatchConfigValidator
{
	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(GreClient.Network.MatchConfig matchConfig)
	{
		if (matchConfig.GameType == GameType.Solitaire)
		{
			yield return (resultType: IMatchConfigValidator.Result.Warning, reason: "GameType Solitaire selected but not supported");
		}
	}
}
