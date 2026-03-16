using System;
using System.Collections.Generic;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene;

public class NullMatchConfigValidator : IMatchConfigValidator
{
	public static IMatchConfigValidator Default = new NullMatchConfigValidator();

	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(MatchConfig matchConfig)
	{
		return Array.Empty<(IMatchConfigValidator.Result, string)>();
	}
}
