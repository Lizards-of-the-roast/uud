using System.Collections.Generic;
using GreClient.Network;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class CommanderDeckValidator : IMatchConfigValidator
{
	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(GreClient.Network.MatchConfig matchConfig)
	{
		bool flag = matchConfig.GameVariant == GameVariant.Brawl || matchConfig.GameVariant == GameVariant.Commander;
		if (flag && !AllPlayersHaveCommanders(matchConfig.Teams))
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: "Not all players are using a deck with a specified commander");
		}
		else if (!flag && AnyPlayerHasACommander(matchConfig.Teams))
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: "Commander present in a non-commander game variant");
		}
	}

	private bool AllPlayersHaveCommanders(IReadOnlyList<GreClient.Network.TeamConfig> teams)
	{
		foreach (GreClient.Network.TeamConfig team in teams)
		{
			foreach (GreClient.Network.PlayerConfig player in team.Players)
			{
				if (player.Deck.Commander.Count == 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool AnyPlayerHasACommander(IReadOnlyList<GreClient.Network.TeamConfig> teams)
	{
		foreach (GreClient.Network.TeamConfig team in teams)
		{
			foreach (GreClient.Network.PlayerConfig player in team.Players)
			{
				if (player.Deck.Commander.Count > 0)
				{
					return true;
				}
			}
		}
		return false;
	}
}
