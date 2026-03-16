using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class AttachmentAndExileStackBrowserProvider : ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider
{
	private AttachmentAndExileStackBrowser _attachmentAndExileBrowser;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly IGreLocProvider _localizationManager;

	private readonly IHighlightController _highlightController;

	private readonly WorkflowBase _currentWorkflow;

	private readonly ICardHoverController _cardHoverController;

	private readonly MtgGameState _latestGameState;

	private readonly List<AttachmentAndExileStackGroupData> _groups;

	private readonly Dictionary<string, ButtonStateData> _buttonStateData;

	private readonly Dictionary<DuelScene_CDC, HighlightType> _browserHighlights = new Dictionary<DuelScene_CDC, HighlightType>();

	private readonly Dictionary<DuelScene_CDC, HighlightType> _selectedHighlights = new Dictionary<DuelScene_CDC, HighlightType>();

	private readonly Dictionary<DuelScene_CDC, DuelScene_CDC> _cardToParent = new Dictionary<DuelScene_CDC, DuelScene_CDC>();

	protected readonly SpinnerController _spinnerController;

	public bool ApplyTargetOffset => false;

	public bool ApplySourceOffset => false;

	public bool ApplyControllerOffset => false;

	private Action<DuelScene_CDC> CardSelectedCallback { get; }

	public AttachmentAndExileStackBrowserProvider(ICardDataAdapter stackParent, WorkflowBase currentWorkflow, ICardViewProvider cardViewProvider, IGreLocProvider locManager, IHighlightController highlightController, ICardHoverController cardHoverController, MtgGameState latestGameState, Action<DuelScene_CDC> onCardSelected)
	{
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_localizationManager = locManager ?? NullGreLocManager.Default;
		_highlightController = highlightController ?? NullHighlightController.Default;
		_currentWorkflow = currentWorkflow;
		_cardHoverController = cardHoverController;
		_latestGameState = latestGameState;
		_buttonStateData = new Dictionary<string, ButtonStateData>();
		ButtonStateData value = new ButtonStateData
		{
			BrowserElementKey = "DismissButton",
			LocalizedString = "DuelScene/Browsers/ViewDismiss_Done",
			Enabled = true,
			StyleType = ButtonStyle.StyleType.Main
		};
		_buttonStateData.Add("DismissButton", value);
		MapParents(stackParent.Instance, setParent: false);
		_groups = AttachmentAndExileStackGroupData.GenerateGroups(stackParent.Instance, _latestGameState, _cardViewProvider, setParent: false);
		PopulateHeaderData(_groups);
		if (_selectedHighlights.Count > 0)
		{
			_highlightController.SetBrowserHighlights(_selectedHighlights, ignoreWorkflowHighlights: false);
		}
		CardSelectedCallback = onCardSelected;
	}

	public AttachmentAndExileStackBrowserProvider(ICardDataAdapter stackParent, ICardViewProvider cardViewProvider, IGreLocProvider locManager, IHighlightController highlightController, ICardHoverController cardHoverController, IWorkflowProvider workflowProvider, IGameStateProvider gameStateProvider, SpinnerController spinnerController, Action<DuelScene_CDC> onCardSelected)
		: this(stackParent, workflowProvider.GetCurrentWorkflow(), cardViewProvider, locManager, highlightController, cardHoverController, gameStateProvider.LatestGameState, onCardSelected)
	{
		_spinnerController = spinnerController;
	}

	public List<AttachmentAndExileStackGroupData> GetGroupData()
	{
		return _groups;
	}

	private void MapParents(MtgCardInstance instance, bool setParent = true)
	{
		if (instance.AttachedWithIds.Count == 0)
		{
			return;
		}
		DuelScene_CDC cardView = _cardViewProvider.GetCardView(instance.InstanceId);
		foreach (uint attachedWithId in instance.AttachedWithIds)
		{
			if (!_latestGameState.VisibleCards.TryGetValue(attachedWithId, out var value))
			{
				continue;
			}
			if (_cardViewProvider.TryGetCardView(attachedWithId, out var cardView2))
			{
				if (setParent)
				{
					_cardToParent.Add(cardView2, cardView);
				}
				MtgGameState latestGameState = _latestGameState;
				if (latestGameState != null && latestGameState.Stack?.CardIds?.Count > 0)
				{
					foreach (uint cardId in _latestGameState.Stack.CardIds)
					{
						if (cardView2.Model.TargetedBy.Exists(cardId, (MtgEntity a, uint t) => a.InstanceId == t))
						{
							_selectedHighlights.Add(cardView2, HighlightType.Selected);
							break;
						}
					}
				}
			}
			MapParents(value);
		}
	}

	private void PopulateHeaderData(List<AttachmentAndExileStackGroupData> groupDataList)
	{
		foreach (AttachmentAndExileStackGroupData groupData in groupDataList)
		{
			string empty = string.Empty;
			empty = ((groupData.Type != AttachmentAndExileStackGroupData.GroupType.Exile) ? ((groupData.Cards.Count > 1) ? "DuelScene/Browsers/Stack_Browser_Attached_Cards" : "DuelScene/Browsers/Stack_Browser_Attached_Card") : ((groupData.Cards.Count > 1) ? "DuelScene/Browsers/Stack_Browser_Exiled_Cards" : "DuelScene/Browsers/Stack_Browser_Exiled_Card"));
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText(empty);
			string subheaderText = string.Empty;
			if (groupData.Parent != null)
			{
				subheaderText = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/Stack_Browser_ParentCard", ("cardname", _localizationManager.GetLocalizedText(groupData.Parent.Model.TitleId)));
			}
			groupData.HeaderData = new BrowserCardHeader.BrowserCardHeaderData(localizedText, subheaderText);
			if (groupData.Children != null)
			{
				PopulateHeaderData(groupData.Children);
			}
		}
	}

	public string GetHeaderText()
	{
		return Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/Stack_Browser_Title");
	}

	public string GetSubHeaderText()
	{
		return string.Empty;
	}

	public string GetCardHolderLayoutKey()
	{
		return "AttachmentAndExileStack";
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.AttachmentAndExileStack;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return _buttonStateData;
	}

	private void OnButtonPressed(string buttonKey)
	{
		if (buttonKey == "DismissButton")
		{
			_attachmentAndExileBrowser.Close();
		}
	}

	private void SetupSpinnersRoot()
	{
		_spinnerController.SetBrowserRoot(_attachmentAndExileBrowser.GetBrowserRoot());
	}

	public void SetOpenedBrowser(IBrowser openedBrowser)
	{
		_cardHoverController.EndHover();
		_attachmentAndExileBrowser = (AttachmentAndExileStackBrowser)openedBrowser;
		_attachmentAndExileBrowser.ClosedHandlers += OnBrowserClosed;
		_attachmentAndExileBrowser.ButtonPressedHandlers += OnButtonPressed;
		_attachmentAndExileBrowser.CardViewSelectedHandlers += CardSelectedCallback;
		CardHoverController.OnHoveredCardUpdated += OnBrowserCardHovered;
		if (_spinnerController != null)
		{
			SetupSpinnersRoot();
		}
	}

	private void OnBrowserCardHovered(DuelScene_CDC duelScene_CDC)
	{
		_browserHighlights.Clear();
		if (duelScene_CDC != null)
		{
			HighlightType value = HighlightType.None;
			if (_currentWorkflow != null)
			{
				_currentWorkflow.Highlights.IdToHighlightType_Workflow.TryGetValue(duelScene_CDC.InstanceId, out value);
			}
			_browserHighlights.Add(duelScene_CDC, value);
			DuelScene_CDC value2 = null;
			if (_cardToParent.TryGetValue(duelScene_CDC, out value2))
			{
				_browserHighlights.Add(value2, HighlightType.Selected);
			}
		}
		if (_selectedHighlights.Count > 0)
		{
			foreach (KeyValuePair<DuelScene_CDC, HighlightType> selectedHighlight in _selectedHighlights)
			{
				if (!_browserHighlights.ContainsKey(selectedHighlight.Key))
				{
					_browserHighlights.Add(selectedHighlight.Key, selectedHighlight.Value);
				}
			}
		}
		_highlightController.SetBrowserHighlights(_browserHighlights, ignoreWorkflowHighlights: false);
	}

	private void OnBrowserClosed()
	{
		_highlightController.SetBrowserHighlights(null);
		CardHoverController.OnHoveredCardUpdated -= OnBrowserCardHovered;
		_attachmentAndExileBrowser.ButtonPressedHandlers -= OnButtonPressed;
		_attachmentAndExileBrowser.ClosedHandlers -= OnBrowserClosed;
		_attachmentAndExileBrowser.CardViewSelectedHandlers -= CardSelectedCallback;
		_attachmentAndExileBrowser = null;
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
	}
}
