using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Browser;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public class BrowserHeaderTextProvider : IBrowserHeaderTextProvider
{
	private readonly IHeaderTextProvider _headerTextProvider;

	private readonly ISubHeaderTextProvider _subHeaderTextProvider;

	private readonly IBlackboard _blackboard;

	public BrowserHeaderTextProvider(AssetLookupSystem assetLookupSystem, IClientLocProvider clientLocProvider, IPromptTextProvider promptTextProvider)
		: this(new HeaderTextProvider(clientLocProvider, new BrowserLocKeyBaseTextProvider<Header>(clientLocProvider, assetLookupSystem.Blackboard, new PayloadProvider<Header>(assetLookupSystem))), new SubHeaderTextProvider(clientLocProvider, promptTextProvider, new BrowserLocKeyBaseTextProvider<SubHeader>(clientLocProvider, assetLookupSystem.Blackboard, new PayloadProvider<SubHeader>(assetLookupSystem))), assetLookupSystem.Blackboard)
	{
	}

	private BrowserHeaderTextProvider(IHeaderTextProvider headerTextProvider, ISubHeaderTextProvider subHeaderTextProvider, IBlackboard blackboard)
	{
		_headerTextProvider = headerTextProvider ?? NullHeaderTextProvider.Default;
		_subHeaderTextProvider = subHeaderTextProvider ?? NullSubHeaderTextProvider.Default;
		_blackboard = blackboard ?? new Blackboard();
	}

	public void SetMinMax(int min, uint max)
	{
		_blackboard.SelectCardBrowserMinMax = (min, max);
	}

	public void SetSourceModel(ICardDataAdapter sourceModel)
	{
		_blackboard.SetCardDataExtensive(sourceModel);
	}

	public void SetWorkflow(WorkflowBase workflow)
	{
		_blackboard.Interaction = workflow;
	}

	public void SetRequest(BaseUserRequest request)
	{
		_blackboard.Request = request;
	}

	public void SetBrowserType(DuelSceneBrowserType browserType)
	{
		_blackboard.CardBrowserType = browserType;
	}

	public void ClearParams()
	{
		_blackboard.Clear();
	}

	public string GetHeaderText()
	{
		return _headerTextProvider.GetText();
	}

	public string GetSubHeaderText(Prompt prompt = null, string defaultKey = null)
	{
		return _subHeaderTextProvider.GetText(prompt, defaultKey);
	}
}
