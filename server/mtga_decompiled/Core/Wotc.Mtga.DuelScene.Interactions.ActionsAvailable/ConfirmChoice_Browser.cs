using System.Collections.Generic;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ConfirmChoice_Browser : WorkflowVariant
{
	private readonly IAbilityDataProvider _abilityProvider;

	private readonly IClientLocProvider _clientLocProvider;

	private readonly IBrowserController _browserController;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly ICardMovementController _cardMovementController;

	private readonly IEntityView _sourceEntity;

	private readonly Action _action;

	private IBrowser _browser;

	private OptionalActionBrowserProvider_ClientSide _provider;

	public ConfirmChoice_Browser(IContext context, IEntityView sourceEntity, Action action)
		: this(context.Get<IAbilityDataProvider>(), context.Get<IClientLocProvider>(), context.Get<IBrowserController>(), context.Get<ICardHolderProvider>(), context.Get<ICardMovementController>(), sourceEntity, action)
	{
	}

	public ConfirmChoice_Browser(IAbilityDataProvider abilityProvider, IClientLocProvider clientLocProvider, IBrowserController browserController, ICardHolderProvider cardHolderProvider, ICardMovementController cardMovementController, IEntityView sourceEntity, Action action)
	{
		_abilityProvider = abilityProvider ?? NullAbilityDataProvider.Default;
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
		_browserController = browserController ?? NullBrowserManager.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_cardMovementController = cardMovementController ?? NullCardMovementController.Default;
		_sourceEntity = sourceEntity;
		_action = action;
	}

	public override void Open()
	{
		if (_sourceEntity is DuelScene_CDC cdc)
		{
			_provider = Provider(_action, cdc, _abilityProvider, _clientLocProvider);
			_browser = _browserController.OpenBrowser(_provider);
			_provider.SetOpenedBrowser(_browser);
		}
		else
		{
			Cancelled?.Invoke();
		}
	}

	private OptionalActionBrowserProvider_ClientSide Provider(Action action, DuelScene_CDC cdc, IAbilityDataProvider abilityProvider, IClientLocProvider locManager)
	{
		OptionalActionBrowserProvider_ClientSide.OptionalActionBrowserData optionalActionBrowserData = new OptionalActionBrowserProvider_ClientSide.OptionalActionBrowserData();
		optionalActionBrowserData.CardViews = new List<DuelScene_CDC> { cdc };
		optionalActionBrowserData.Header = locManager.GetLocalizedText("DuelScene/ClientPrompt/Are_You_Sure_Title");
		optionalActionBrowserData.SubHeader = locManager.GetLocalizedText(cdc?.Model?.Instance?.PlayWarnings?.GetSubHeaderKey());
		optionalActionBrowserData.NoText = "DuelScene/ClientPrompt/ClientPrompt_Button_No";
		optionalActionBrowserData.OnNoAction = Cancel;
		optionalActionBrowserData.YesText = "DuelScene/ClientPrompt/ClientPrompt_Button_Yes";
		optionalActionBrowserData.OnYesAction = SubmitAction;
		optionalActionBrowserData.AbilityByCardView[cdc] = abilityProvider.GetAbilityPrintingById(action.AbilityGrpId);
		optionalActionBrowserData.GreActionByCardView[cdc] = action;
		return new OptionalActionBrowserProvider_ClientSide(optionalActionBrowserData);
	}

	private void SubmitAction()
	{
		SelectedActions.Add(_action);
		Submitted?.Invoke();
	}

	private void Cancel()
	{
		if (_sourceEntity is DuelScene_CDC duelScene_CDC)
		{
			ICardHolder cardHolderByZoneId = _cardHolderProvider.GetCardHolderByZoneId(duelScene_CDC.Model.Zone.Id);
			if (duelScene_CDC.CurrentCardHolder != cardHolderByZoneId)
			{
				_cardMovementController.MoveCard(duelScene_CDC, cardHolderByZoneId);
			}
		}
		Cancelled?.Invoke();
	}

	public override void Close()
	{
	}
}
