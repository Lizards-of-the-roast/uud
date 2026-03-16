using System.Collections.Generic;
using AssetLookupTree.Blackboard;

public interface IDuelSceneBrowserProvider
{
	DuelSceneBrowserType GetBrowserType();

	Dictionary<string, ButtonStateData> GetButtonStateData();

	void SetFxBlackboardData(IBlackboard bb);
}
