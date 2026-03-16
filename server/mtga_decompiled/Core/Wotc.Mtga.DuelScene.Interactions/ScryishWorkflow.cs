using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;

namespace Wotc.Mtga.DuelScene.Interactions;

public abstract class ScryishWorkflow<T> : BrowserWorkflowBase<T>, IScryishBrowserProvider, IBasicBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider where T : BaseUserRequest
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly IBrowserController _browserController;

	public virtual int NthFromTop => 1;

	public virtual int NthFromBot => 1;

	public ScryishWorkflow(T request, ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, ICardViewProvider cardViewProvider, IBrowserController browserController)
		: base(request)
	{
		_cardDatabase = cardDatabase;
		_gameStateProvider = gameStateProvider;
		_cardViewProvider = cardViewProvider;
		_browserController = browserController;
	}

	protected abstract IEnumerable<uint> GetCardIds();

	protected abstract void OnDoneButtonPressed(ScryBrowser browser);

	protected void SetHeader()
	{
		MtgCardInstance resolvingCardInstance = ((MtgGameState)_gameStateProvider.LatestGameState).ResolvingCardInstance;
		if (resolvingCardInstance != null)
		{
			_header = _cardDatabase.GreLocProvider.GetLocalizedText(resolvingCardInstance.TitleId);
		}
	}

	protected virtual void SetSubHeader()
	{
		_subHeader = _cardDatabase.ClientLocProvider.GetLocalizedText("DuelScene/Browsers/ScryBrowser_SubHeader");
	}

	public override DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.Scryish;
	}

	protected sealed override void ApplyInteractionInternal()
	{
		SetHeader();
		SetSubHeader();
		_cardsToDisplay = _cardViewProvider.GetCardViews(GetCardIds());
		IBrowser browser = _browserController.OpenBrowser(this);
		ScryishBrowser obj = browser as ScryishBrowser;
		_buttonStateData = new Dictionary<string, ButtonStateData>();
		ButtonStateData value = new ButtonStateData
		{
			LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_Done",
			Enabled = true,
			BrowserElementKey = "SubmitButton",
			StyleType = ButtonStyle.StyleType.Main
		};
		_buttonStateData.Add("DoneButton", value);
		obj.UpdateButtons();
		SetOpenedBrowser(browser);
	}

	protected override void Browser_OnBrowserButtonPressed(string buttonKey)
	{
		if (buttonKey == "DoneButton")
		{
			ScryBrowser browser = _openedBrowser as ScryBrowser;
			OnDoneButtonPressed(browser);
		}
	}

	public override void SetFxBlackboardData(IBlackboard bb)
	{
		base.SetFxBlackboardData(bb);
		SetBlackboard(_request, _gameStateProvider.LatestGameState, bb);
	}

	protected static void SetBlackboard(T request, MtgGameState gameState, IBlackboard blackboard)
	{
		if (gameState.GetCardById(request.SourceId) != null)
		{
			blackboard.Clear();
			blackboard.Request = request;
			blackboard.Prompt = request.Prompt;
			MtgCardInstance resolvingCardInstance = gameState.ResolvingCardInstance;
			if (resolvingCardInstance != null)
			{
				blackboard.SetCardDataExtensive(resolvingCardInstance);
			}
		}
	}
}
