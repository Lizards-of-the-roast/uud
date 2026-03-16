using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public interface IBrowserHeaderTextProvider
{
	void SetMinMax(int min, uint max);

	void SetSourceModel(ICardDataAdapter sourceModel);

	void SetWorkflow(WorkflowBase workflow);

	void SetRequest(BaseUserRequest request);

	void SetBrowserType(DuelSceneBrowserType browserType);

	void SetParams(int? min = null, uint? max = null, ICardDataAdapter sourceModel = null, WorkflowBase workflow = null, BaseUserRequest request = null, DuelSceneBrowserType? browserType = null)
	{
		ClearParams();
		SetMinMax(min.GetValueOrDefault(), max.GetValueOrDefault());
		SetSourceModel(sourceModel);
		SetWorkflow(workflow);
		SetRequest(request);
		SetBrowserType(browserType.GetValueOrDefault());
	}

	void ClearParams();

	string GetHeaderText();

	string GetSubHeaderText(Prompt prompt = null, string defaultKey = "DuelScene/Browsers/Choose_Option_Text");
}
