using System.Collections.Generic;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene;

public interface IMatchConfigValidator
{
	public enum Result
	{
		None,
		Warning,
		Error
	}

	IEnumerable<(Result resultType, string reason)> GetResults(MatchConfig matchConfig);
}
