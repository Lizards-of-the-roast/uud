using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;
using Wotc.Mtgo.Gre.External.Messaging;

public class ModalBrowserProvider_ClientSide : SelectCardsWorkflow<BaseUserRequest>
{
	public class ModalBrowserData
	{
		public readonly List<DuelScene_CDC> Selectable = new List<DuelScene_CDC>();

		public readonly List<DuelScene_CDC> NonSelectable = new List<DuelScene_CDC>();

		public readonly List<DuelScene_CDC> SortedCardList = new List<DuelScene_CDC>();

		public string Header;

		public string SubHeader;

		public Dictionary<DuelScene_CDC, HighlightType> Highlights;

		public string CancelText;

		public bool CanCancel;

		public GetModalBrowserCardHeaderDelegate GetBrowserCardHeaderData;

		public Action<DuelScene_CDC> OnSelectionMade;

		public System.Action NoInteractionPerformedCloseAction;

		public string CardHolderKey = "Modal";

		public readonly Dictionary<DuelScene_CDC, AbilityPrintingData> AbilityByCardView = new Dictionary<DuelScene_CDC, AbilityPrintingData>();

		public readonly Dictionary<DuelScene_CDC, Wotc.Mtgo.Gre.External.Messaging.Action> GreActionByCardView = new Dictionary<DuelScene_CDC, Wotc.Mtgo.Gre.External.Messaging.Action>();

		public List<string> FakeCardKeyStrings = new List<string>();
	}

	private readonly ModalBrowserData _browserData;

	private bool _actionTaken;

	private readonly IFakeCardViewController _fakeCardController;

	public override DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.SelectCards;
	}

	public override string GetCardHolderLayoutKey()
	{
		return _browserData.CardHolderKey;
	}

	public ModalBrowserProvider_ClientSide(ModalBrowserData data, IFakeCardViewController fakeCardController)
		: base((BaseUserRequest)null)
	{
		_browserData = data;
		_fakeCardController = fakeCardController;
		if (data.SortedCardList.Count > 0)
		{
			_cardsToDisplay = data.SortedCardList;
		}
		else
		{
			_cardsToDisplay = new List<DuelScene_CDC>();
			_cardsToDisplay.AddRange(data.Selectable);
			_cardsToDisplay.AddRange(data.NonSelectable);
		}
		selectable.AddRange(data.Selectable);
		nonSelectable.AddRange(data.NonSelectable);
		_header = data.Header;
		_subHeader = data.SubHeader;
		if (selectable.Count > 0 && selectable.TrueForAll((DuelScene_CDC x) => x.Model.ObjectType == GameObjectType.Ability))
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_walker_prep_ability_gotostack, selectable[0].gameObject);
		}
		_buttonStateData = new Dictionary<string, ButtonStateData>();
		if (data.CanCancel)
		{
			ButtonStateData value = new ButtonStateData
			{
				LocalizedString = "DuelScene/ClientPrompt/ClientPrompt_Button_Cancel",
				Enabled = true,
				BrowserElementKey = "SingleButton",
				StyleType = ButtonStyle.StyleType.Main
			};
			_buttonStateData.Add("CancelButton", value);
		}
	}

	protected override void ApplyInteractionInternal()
	{
	}

	protected override void CardBrowser_OnPreReleaseCardViews()
	{
		if (!_actionTaken)
		{
			_browserData.NoInteractionPerformedCloseAction?.Invoke();
		}
	}

	protected override void CardBrowser_OnCardViewSelected(DuelScene_CDC cardView)
	{
		if (_browserData.OnSelectionMade != null)
		{
			_actionTaken = true;
			_browserData.OnSelectionMade(cardView);
			AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_hightlight_on_selection, AudioManager.Default);
		}
	}

	protected override void Browser_OnBrowserButtonPressed(string buttonKey)
	{
		if (buttonKey == "CancelButton" && _browserData.OnSelectionMade != null)
		{
			_actionTaken = true;
			_browserData.OnSelectionMade(null);
		}
	}

	public override BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return _browserData.GetBrowserCardHeaderData?.Invoke(cardView.Model, _browserData.GreActionByCardView[cardView]);
	}

	public override Dictionary<DuelScene_CDC, HighlightType> GetBrowserHighlights()
	{
		if (_browserData?.Highlights != null)
		{
			return _browserData.Highlights;
		}
		return base.GetBrowserHighlights();
	}

	public override void SetFxBlackboardData(IBlackboard bb)
	{
		base.SetFxBlackboardData(bb);
		bb.Ability = null;
		foreach (AbilityPrintingData value in _browserData.AbilityByCardView.Values)
		{
			if (value != null)
			{
				bb.Ability = value;
				break;
			}
		}
		bb.GreActionType = ActionType.None;
		foreach (Wotc.Mtgo.Gre.External.Messaging.Action value2 in _browserData.GreActionByCardView.Values)
		{
			if (value2.ActionType > bb.GreActionType)
			{
				bb.GreActionType = value2.ActionType;
				bb.GreAction = value2;
			}
		}
	}

	public override void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
		base.SetFxBlackboardDataForCard(cardView, bb);
		bb.Ability = (_browserData.AbilityByCardView.TryGetValue(cardView, out var value) ? value : null);
		bb.GreActionType = (_browserData.GreActionByCardView.TryGetValue(cardView, out var value2) ? value2.ActionType : ActionType.None);
		bb.GreAction = value2;
	}

	protected override void Browser_OnBrowserClosed()
	{
		foreach (string fakeCardKeyString in _browserData.FakeCardKeyStrings)
		{
			_fakeCardController.DeleteFakeCard(fakeCardKeyString);
		}
		base.Browser_OnBrowserClosed();
	}
}
