using System.Collections.Generic;
using GreClient.Network;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class GameVariantValidator : IMatchConfigValidator
{
	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(GreClient.Network.MatchConfig matchConfig)
	{
		switch (matchConfig.GameVariant)
		{
		case GameVariant.Planechase:
		case GameVariant.Vanguard:
		case GameVariant.Archenemy:
			yield return (resultType: IMatchConfigValidator.Result.Warning, reason: $"{matchConfig.GameVariant} selected but format rules aren't implemented on the GRE");
			break;
		case GameVariant.Commander:
		case GameVariant.TwoHeadedGiant:
			yield return (resultType: IMatchConfigValidator.Result.Warning, reason: $"{matchConfig.GameVariant} format selected but not supported. Your milage may vary");
			break;
		}
	}
}
