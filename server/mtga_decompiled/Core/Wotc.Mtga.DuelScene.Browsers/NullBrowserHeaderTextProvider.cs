using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public class NullBrowserHeaderTextProvider : IBrowserHeaderTextProvider
{
	public static readonly IBrowserHeaderTextProvider Default = new NullBrowserHeaderTextProvider();

	public void SetMinMax(int min, uint max)
	{
	}

	public void SetSourceModel(ICardDataAdapter sourceModel)
	{
	}

	public void SetWorkflow(WorkflowBase workflow)
	{
	}

	public void SetRequest(BaseUserRequest request)
	{
	}

	public void SetBrowserType(DuelSceneBrowserType browserType)
	{
	}

	public void ClearParams()
	{
	}

	public string GetHeaderText()
	{
		return string.Empty;
	}

	public string GetSubHeaderText(Prompt prompt = null, string defaultKey = "DuelScene/Browsers/Choose_Option_Text")
	{
		return string.Empty;
	}
}
