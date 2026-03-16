using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class RevealedCardSelection : BrowserWorkflowBase<SelectNRequest>, ISelectCardsBrowserProvider, IBasicBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider
{
	private const string REVEALED_CARD_ID_FORMAT = "RevealedInstance_{0}";

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IFakeCardViewController _fakeCardViewController;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IGameplaySettingsProvider _gameplaySettingsProvider;

	private readonly IBrowserController _browserController;

	private readonly IBrowserHeaderTextProvider _headerTextProvider;

	private readonly Dictionary<DuelScene_CDC, uint> _cardToIdMap = new Dictionary<DuelScene_CDC, uint>();

	private readonly HashSet<uint> _selections = new HashSet<uint>();

	private readonly Dictionary<DuelScene_CDC, HighlightType> _cardHighlights = new Dictionary<DuelScene_CDC, HighlightType>(3);

	public bool AllowKeyboardSelection => false;

	private bool CanSubmit
	{
		get
		{
			if (_request.MinSel <= _selections.Count)
			{
				return _selections.Count <= _request.MaxSel;
			}
			return false;
		}
	}

	public override DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.SelectCards;
	}

	public override string GetCardHolderLayoutKey()
	{
		return "Modal";
	}

	public RevealedCardSelection(SelectNRequest request, ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IGameplaySettingsProvider gameplaySettings, IFakeCardViewController fakeCardViewController, IBrowserController browserController, IBrowserHeaderTextProvider headerTextProvider)
		: base(request)
	{
		_cardDatabase = cardDatabase;
		_gameStateProvider = gameStateProvider;
		_gameplaySettingsProvider = gameplaySettings;
		_fakeCardViewController = fakeCardViewController;
		_browserController = browserController;
		_headerTextProvider = headerTextProvider;
		_buttonStateData = new Dictionary<string, ButtonStateData> { ["DoneButton"] = new ButtonStateData
		{
			Enabled = false,
			BrowserElementKey = "DoneButton",
			LocalizedString = "DuelScene/Browsers/ViewDismiss_Done",
			StyleType = ButtonStyle.StyleType.Main
		} };
	}

	private void UpdateDoneButton()
	{
		_buttonStateData["DoneButton"].Enabled = CanSubmit;
		_openedBrowser?.UpdateButtons();
	}

	protected override void ApplyInteractionInternal()
	{
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		foreach (uint id in _request.Ids)
		{
			MtgCardInstance cardById = mtgGameState.GetCardById(id);
			if (cardById != null)
			{
				DuelScene_CDC duelScene_CDC = _fakeCardViewController.CreateFakeCard($"RevealedInstance_{id}", cardById.ToCardData(_cardDatabase), isVisible: true);
				_cardToIdMap[duelScene_CDC] = id;
				_cardsToDisplay.Add(duelScene_CDC);
			}
		}
		ICardDataAdapter headerAndSubheader = mtgGameState.GetCardById(_request.SourceId).ToCardData(_cardDatabase);
		SetHeaderAndSubheader(headerAndSubheader);
		SetOpenedBrowser(_browserController.OpenBrowser(this));
	}

	private void SetHeaderAndSubheader(ICardDataAdapter sourceModel)
	{
		_headerTextProvider.ClearParams();
		_headerTextProvider.SetMinMax(_request.MinSel, _request.MaxSel);
		_headerTextProvider.SetSourceModel(sourceModel);
		_headerTextProvider.SetWorkflow(this);
		_headerTextProvider.SetRequest(_request);
		_headerTextProvider.SetBrowserType(DuelSceneBrowserType.SelectCards);
		_header = _headerTextProvider.GetHeaderText();
		_subHeader = _headerTextProvider.GetSubHeaderText(_request.Prompt);
		_headerTextProvider.ClearParams();
	}

	protected override void CardBrowser_OnCardViewSelected(DuelScene_CDC cardView)
	{
		if (CanClick(cardView))
		{
			OnClick(cardView);
		}
	}

	private bool CanClick(DuelScene_CDC cardView)
	{
		return _cardToIdMap.ContainsKey(cardView);
	}

	private void OnClick(DuelScene_CDC cardView)
	{
		if (_cardToIdMap.TryGetValue(cardView, out var value))
		{
			(_selections.Contains(value) ? new Func<uint, bool>(_selections.Remove) : new Func<uint, bool>(_selections.Add))(value);
			(_gameplaySettingsProvider.GameplaySettings.FullControlEnabled ? new Action(UpdateDoneButton) : new Action(SubmitCurrentSelections))();
		}
	}

	protected override void Browser_OnBrowserButtonPressed(string buttonKey)
	{
		if (!(buttonKey != "DoneButton"))
		{
			(CanSubmit ? new Action(SubmitCurrentSelections) : new Action(UpdateDoneButton))();
		}
	}

	private void SubmitCurrentSelections()
	{
		_request.SubmitSelection(_selections);
	}

	public override void CleanUp()
	{
		foreach (KeyValuePair<DuelScene_CDC, uint> item in _cardToIdMap)
		{
			_fakeCardViewController.DeleteFakeCard($"RevealedInstance_{item.Value}");
		}
		_cardToIdMap.Clear();
		_selections.Clear();
		base.CleanUp();
	}

	public IEnumerable<DuelScene_CDC> GetSelectableCdcs()
	{
		foreach (KeyValuePair<DuelScene_CDC, uint> item in _cardToIdMap)
		{
			yield return item.Key;
		}
	}

	public IEnumerable<DuelScene_CDC> GetNonSelectableCdcs()
	{
		return Array.Empty<DuelScene_CDC>();
	}

	public Dictionary<DuelScene_CDC, HighlightType> GetBrowserHighlights()
	{
		_cardHighlights.Clear();
		foreach (KeyValuePair<DuelScene_CDC, uint> item in _cardToIdMap)
		{
			HighlightType value = (_selections.Contains(item.Value) ? HighlightType.Selected : HighlightType.Hot);
			_cardHighlights[item.Key] = value;
		}
		return _cardHighlights;
	}
}
