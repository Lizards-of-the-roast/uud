using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace Wotc.Mtga.DuelScene.Browsers;

public class InformationalBrowserProvider : IDuelSceneBrowserProvider, IBrowserHeaderProvider
{
	private string _headerText;

	private string _subHeaderText;

	public InformationalBrowserProvider(string headerText, string subHeaderText)
	{
		_headerText = headerText;
		_subHeaderText = subHeaderText;
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.Informational;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return new Dictionary<string, ButtonStateData>();
	}

	public string GetHeaderText()
	{
		return _headerText;
	}

	public string GetSubHeaderText()
	{
		return _subHeaderText;
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}
}
