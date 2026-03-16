using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.DuelScene.Interactions;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Interactions;

public abstract class SurveilStyleBrowserWorkflow<T> : BrowserWorkflowBase<T> where T : BaseUserRequest
{
	private readonly ICardViewProvider _cardViewProvider;

	protected IClientLocProvider _clientLocManager;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardDatabaseAdapter _cardDatabaseAdapter;

	private readonly IBrowserController _browserController;

	public SurveilStyleBrowserWorkflow(T request, IClientLocProvider clientLocProvider, IGameStateProvider gameStateProvider, ICardDatabaseAdapter cardDatabaseAdapter, IBrowserController browserController, ICardViewProvider cardViewProvider)
		: base(request)
	{
		_cardViewProvider = cardViewProvider;
		_clientLocManager = clientLocProvider;
		_gameStateProvider = gameStateProvider;
		_cardDatabaseAdapter = cardDatabaseAdapter;
		_browserController = browserController;
	}

	protected abstract IEnumerable<uint> GetCardIds();

	protected abstract void OnDoneButtonPressed(SurveilBrowser browser);

	protected virtual bool IsDoneButtonEnabled(SurveilBrowser browser)
	{
		return true;
	}

	protected virtual void SetHeader()
	{
		MtgCardInstance resolvingCardInstance = _gameStateProvider.LatestGameState.Value.ResolvingCardInstance;
		if (resolvingCardInstance != null)
		{
			_header = _cardDatabaseAdapter.GreLocProvider.GetLocalizedText(resolvingCardInstance.TitleId);
		}
	}

	protected virtual void SetSubHeader()
	{
		_subHeader = _clientLocManager.GetLocalizedText("DuelScene/Browsers/BrowserSubheader_DragToLibraryOrGraveyard");
	}

	public override DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.Surveil;
	}

	protected sealed override void ApplyInteractionInternal()
	{
		SetHeader();
		SetSubHeader();
		_cardsToDisplay = _cardViewProvider.GetCardViews(GetCardIds());
		IBrowser browser = _browserController.OpenBrowser(this);
		SurveilBrowser surveilBrowser = browser as SurveilBrowser;
		surveilBrowser.DragReleaseEvent = (Action)Delegate.Combine(surveilBrowser.DragReleaseEvent, new Action(UpdateDoneButton));
		_buttonStateData = new Dictionary<string, ButtonStateData>();
		ButtonStateData value = new ButtonStateData
		{
			LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_Done",
			Enabled = IsDoneButtonEnabled(surveilBrowser),
			BrowserElementKey = "SubmitButton",
			StyleType = ButtonStyle.StyleType.Main
		};
		_buttonStateData.Add("DoneButton", value);
		surveilBrowser.UpdateButtons();
		SetOpenedBrowser(browser);
	}

	public override void CleanUp()
	{
		if (_openedBrowser is SurveilBrowser surveilBrowser)
		{
			surveilBrowser.DragReleaseEvent = (Action)Delegate.Remove(surveilBrowser.DragReleaseEvent, new Action(UpdateDoneButton));
		}
		base.CleanUp();
	}

	private void UpdateDoneButton()
	{
		SurveilBrowser surveilBrowser = _openedBrowser as SurveilBrowser;
		_buttonStateData["DoneButton"].Enabled = IsDoneButtonEnabled(surveilBrowser);
		surveilBrowser.UpdateButtons();
	}

	protected override void Browser_OnBrowserButtonPressed(string buttonKey)
	{
		if (buttonKey == "DoneButton")
		{
			SurveilBrowser browser = _openedBrowser as SurveilBrowser;
			OnDoneButtonPressed(browser);
		}
	}

	public override void SetFxBlackboardData(IBlackboard bb)
	{
		base.SetFxBlackboardData(bb);
		SetBlackboard(_request, _gameStateProvider.LatestGameState, bb);
	}

	private static void SetBlackboard(T request, MtgGameState gameState, IBlackboard blackboard)
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

	public static bool UseSurveilStyleBrowser(T request, AssetLookupSystem assetLookupSystem, MtgGameState gameState)
	{
		SetBlackboard(request, gameState, assetLookupSystem.Blackboard);
		bool result = false;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<UseSurveilStyleBrowserPayload> loadedTree))
		{
			result = loadedTree.GetPayload(assetLookupSystem.Blackboard) != null;
		}
		return result;
	}
}
