using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Browser;

public class NameACardPrepopulationLogic : IPayload
{
	public bool IncludeBackOfDoubleFaceCards = true;

	public bool IncludeSplitChildren = true;

	public bool IncludeAdventureChildren = true;

	public bool IncludeLands = true;

	public bool IncludeNonVisibleSpecializeChildren = true;

	public bool IncludeNonOpponentCards = true;

	public bool SetSourceGrpIdsToDeckGrpIds = true;

	public bool IncludeControlledCreatures;

	public bool FallbackOnDeckGrpIds;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
