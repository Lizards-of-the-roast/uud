using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;
using Wotc.Mtgo.Gre.External.Messaging;

public class OptionalActionBrowserProvider_ClientSide : OptionalActionBrowserWorkflow
{
	public class OptionalActionBrowserData
	{
		public List<DuelScene_CDC> CardViews;

		public string Header;

		public string SubHeader;

		public string YesText;

		public System.Action OnYesAction;

		public string NoText;

		public System.Action OnNoAction;

		public System.Action NoInteractionPerformedCloseAction;

		public GetModalBrowserCardHeaderDelegate GetBrowserCardHeaderData;

		public readonly Dictionary<DuelScene_CDC, AbilityPrintingData> AbilityByCardView = new Dictionary<DuelScene_CDC, AbilityPrintingData>();

		public readonly Dictionary<DuelScene_CDC, Wotc.Mtgo.Gre.External.Messaging.Action> GreActionByCardView = new Dictionary<DuelScene_CDC, Wotc.Mtgo.Gre.External.Messaging.Action>();
	}

	private readonly OptionalActionBrowserData _browserData;

	private bool _anyInteractionPerformed;

	public OptionalActionBrowserProvider_ClientSide(OptionalActionBrowserData data)
		: base(null)
	{
		_browserData = data;
		_cardsToDisplay = data.CardViews;
		_header = data.Header;
		_subHeader = data.SubHeader;
		SetupButtons(data.YesText, data.NoText);
	}

	protected override void ApplyInteractionInternal()
	{
	}

	protected override void OnBrowserActionSelected(OptionResponse response)
	{
		System.Action action = null;
		switch (response)
		{
		case OptionResponse.AllowYes:
			action = _browserData.OnYesAction;
			break;
		case OptionResponse.CancelNo:
			action = _browserData.OnNoAction;
			break;
		}
		action?.Invoke();
		_anyInteractionPerformed = true;
		_openedBrowser.Close();
	}

	protected override void CardBrowser_OnPreReleaseCardViews()
	{
		if (!_anyInteractionPerformed && _browserData.NoInteractionPerformedCloseAction != null)
		{
			_browserData.NoInteractionPerformedCloseAction();
		}
	}

	public override BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return _browserData.GetBrowserCardHeaderData?.Invoke(cardView.Model, _browserData.GreActionByCardView[cardView]);
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
			}
		}
	}

	public override void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
		base.SetFxBlackboardDataForCard(cardView, bb);
		bb.Ability = (_browserData.AbilityByCardView.TryGetValue(cardView, out var value) ? value : null);
		bb.GreActionType = (_browserData.GreActionByCardView.TryGetValue(cardView, out var value2) ? value2.ActionType : ActionType.None);
	}
}
