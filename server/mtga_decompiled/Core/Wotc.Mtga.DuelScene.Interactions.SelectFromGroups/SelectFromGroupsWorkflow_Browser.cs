using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectFromGroups;

public class SelectFromGroupsWorkflow_Browser : SelectCardsWorkflow<SelectFromGroupsRequest>
{
	private readonly ICardViewProvider _cardViewProvider;

	private readonly IBrowserController _browserController;

	private readonly IBrowserHeaderTextProvider _headerTextProvider;

	private GroupNode _rootNode;

	private List<uint> _lockedIds = new List<uint>();

	public int SelectableCount => _rootNode?.SelectableIds.Count ?? 0;

	public int SelectionCount => _rootNode?.SelectedIds.Count ?? 0;

	public override DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.SelectCards;
	}

	public SelectFromGroupsWorkflow_Browser(SelectFromGroupsRequest request, ICardViewProvider cardViewProvider, IBrowserController browserController, IBrowserHeaderTextProvider headerTextProvider)
		: base(request)
	{
		_cardViewProvider = cardViewProvider;
		_browserController = browserController;
		_headerTextProvider = headerTextProvider;
	}

	protected override void ApplyInteractionInternal()
	{
		_lockedIds = SelectFromGroupsRequest.GetUniqueGroupIds(_request.Groups);
		_rootNode = new GroupNode(_request);
		foreach (uint lockedId in _lockedIds)
		{
			_rootNode.Branch(lockedId);
		}
		_cardsToDisplay = _cardViewProvider.GetCardViews(_rootNode.Ids);
		foreach (uint unfilteredId in _request.UnfilteredIds)
		{
			if (_cardViewProvider.TryGetCardView(unfilteredId, out var cardView) && !_cardsToDisplay.Contains(cardView))
			{
				_cardsToDisplay.Add(cardView);
			}
		}
		SortCards();
		UpdateSelectable();
		SetHeaderAndSubheader();
		_buttonStateData = GenerateDefaultButtonStates(_rootNode.SelectedIds.Count, (int)_request.MinSelections, (int)_request.MaxSelections, _request.CancellationType);
		SetOpenedBrowser(_browserController.OpenBrowser(this));
	}

	private void SetHeaderAndSubheader()
	{
		_headerTextProvider.ClearParams();
		_headerTextProvider.SetMinMax((int)_request.MinSelections, _request.MaxSelections);
		_headerTextProvider.SetWorkflow(this);
		_headerTextProvider.SetRequest(_request);
		_headerTextProvider.SetBrowserType(DuelSceneBrowserType.SelectCards);
		_header = _headerTextProvider.GetHeaderText();
		_subHeader = _headerTextProvider.GetSubHeaderText(_prompt);
		_headerTextProvider.ClearParams();
	}

	private void SortCards()
	{
		if (_cardViewProvider.TryGetCardView(_request.SourceId, out var cardView) && cardView.Model.Instance.TitleId == 3858)
		{
			_cardsToDisplay.Sort((DuelScene_CDC lhs, DuelScene_CDC rhs) => lhs.Model.ConvertedManaCost.CompareTo(rhs.Model.ConvertedManaCost));
			return;
		}
		_cardsToDisplay.Sort(delegate(DuelScene_CDC lhs, DuelScene_CDC rhs)
		{
			uint instanceId = lhs.InstanceId;
			uint instanceId2 = rhs.InstanceId;
			bool flag = _rootNode.Ids.Contains(instanceId);
			bool flag2 = _rootNode.Ids.Contains(instanceId2);
			if (flag && !flag2)
			{
				return -1;
			}
			if (flag2 && !flag)
			{
				return 1;
			}
			bool flag3 = _lockedIds.Contains(instanceId);
			bool value = _lockedIds.Contains(instanceId2);
			return flag3.CompareTo(value);
		});
	}

	private void UpdateSelectable()
	{
		selectable.Clear();
		nonSelectable.Clear();
		currentSelections.Clear();
		foreach (DuelScene_CDC item in _cardsToDisplay)
		{
			uint instanceId = item.InstanceId;
			if (_rootNode.SelectedIds.Contains(instanceId))
			{
				currentSelections.Add(item);
				selectable.Add(item);
			}
			else if (_rootNode.SelectableIds.Contains(instanceId))
			{
				selectable.Add(item);
			}
			else
			{
				nonSelectable.Add(item);
			}
		}
	}

	protected override void Browser_OnBrowserButtonPressed(string buttonKey)
	{
		if (buttonKey == "SubmitButton")
		{
			_request.Submit(_rootNode.Submit());
		}
		else if (buttonKey == "CancelButton")
		{
			_request.Cancel();
		}
	}

	protected override void CardBrowser_OnCardViewSelected(DuelScene_CDC cardView)
	{
		uint instanceId = cardView.InstanceId;
		if (!_lockedIds.Contains(instanceId))
		{
			if (_rootNode.SelectedIds.Contains(instanceId))
			{
				_rootNode.Prune(instanceId);
			}
			else if (_rootNode.SelectableIds.Contains(instanceId))
			{
				_rootNode.Branch(instanceId);
			}
			UpdateSelectable();
			_buttonStateData = GenerateDefaultButtonStates(_rootNode.SelectedIds.Count, (int)_request.MinSelections, (int)_request.MaxSelections, _request.CancellationType);
			_openedBrowser.UpdateButtons();
		}
	}

	protected override Dictionary<string, ButtonStateData> GenerateDefaultButtonStates(int currentSelectionCount, int minSelections, int maxSelections, AllowCancel cancelType)
	{
		bool canSubmit = _rootNode.CanSubmit;
		Dictionary<string, ButtonStateData> dictionary = new Dictionary<string, ButtonStateData>();
		MTGALocalizedString mTGALocalizedString = "DuelScene/ClientPrompt/Submit_N";
		mTGALocalizedString.Parameters = new Dictionary<string, string> { 
		{
			"submitCount",
			_rootNode.SelectedIds.Count.ToString()
		} };
		ButtonStyle.StyleType styleType = ((_request.Style == GroupingStyle.AllGroups) ? ((!canSubmit) ? ButtonStyle.StyleType.Tepid : ButtonStyle.StyleType.Secondary) : ((_rootNode.SelectedIds.Count < minSelections) ? ButtonStyle.StyleType.Tepid : (_rootNode.MaximizedSelection ? ButtonStyle.StyleType.Main : ButtonStyle.StyleType.Secondary)));
		ButtonStateData value = new ButtonStateData
		{
			BrowserElementKey = "SubmitButton",
			Enabled = canSubmit,
			LocalizedString = mTGALocalizedString,
			StyleType = styleType
		};
		dictionary.Add("SubmitButton", value);
		return dictionary;
	}
}
