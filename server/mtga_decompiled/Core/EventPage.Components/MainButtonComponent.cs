using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Event;
using Core.Meta.MainNavigation.EventPageV2;
using Core.Meta.Tokens;
using UnityEngine;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class MainButtonComponent : EventComponent
{
	[Header("Button References")]
	[SerializeField]
	private CustomButtonWithTooltip _playButton;

	public const string ViewState_Play = "PlayState";

	public const string ViewState_Start = "StartState";

	public const string ViewState_Gems = "GemsState";

	public const string ViewState_Gold = "GoldState";

	public const string ViewState_Event = "EventState";

	[SerializeField]
	private CustomButtonWithTooltip _startButton;

	[SerializeField]
	private CustomButtonWithTooltip _payWithGemsButton;

	[SerializeField]
	private CustomButtonWithTooltip _payWithGoldButton;

	[SerializeField]
	private CustomButtonWithTooltip _payWithEventTokenButton;

	[SerializeField]
	private ContainerProxy _payWithEventTokenIconContainer;

	private Dictionary<string, MainButtonComponentViewModel> ViewModels;

	private MainButtonComponentViewModel currentViewModel;

	public Action<EventEntryFeeInfo> PayJoinButton_OnClick;

	public Action PlayButton_OnClick;

	private void Awake()
	{
		_playButton.OnClick.AddListener(delegate
		{
			SetButtonState(_playButton, PlayButtonState.Disabled);
			PlayButton_OnClick?.Invoke();
		});
		ViewModels = new Dictionary<string, MainButtonComponentViewModel>
		{
			{
				"PlayState",
				new MainButtonComponentViewModel
				{
					ViewState = "PlayState",
					ButtonWithToolTip = _playButton,
					AudioEventName = null
				}
			},
			{
				"StartState",
				new MainButtonComponentViewModel
				{
					ViewState = "StartState",
					ButtonWithToolTip = _startButton,
					AudioEventName = null
				}
			},
			{
				"GemsState",
				new MainButtonComponentViewModel
				{
					ViewState = "GemsState",
					ButtonWithToolTip = _payWithGemsButton,
					AudioEventName = WwiseEvents.sfx_ui_gems_payment
				}
			},
			{
				"GoldState",
				new MainButtonComponentViewModel
				{
					ViewState = "GoldState",
					ButtonWithToolTip = _payWithGoldButton,
					AudioEventName = WwiseEvents.sfx_ui_gold_payout
				}
			},
			{
				"EventState",
				new MainButtonComponentViewModel
				{
					ViewState = "EventState",
					ButtonWithToolTip = _payWithEventTokenButton,
					AudioEventName = null,
					IconContainer = _payWithEventTokenIconContainer
				}
			}
		};
	}

	public void ResetButtons()
	{
		foreach (KeyValuePair<string, MainButtonComponentViewModel> viewModel in ViewModels)
		{
			viewModel.Value.ButtonWithToolTip.Hide();
		}
	}

	public MainButtonComponentViewModel ViewModelForState(string viewState)
	{
		return ViewModels[viewState];
	}

	public void SetButtonForViewState(string viewState, PlayButtonState state, bool showTooltip = false)
	{
		currentViewModel = ViewModelForState(viewState);
		SetButtonState(currentViewModel.ButtonWithToolTip, state, showTooltip);
	}

	public static void SetButtonState(CustomButtonWithTooltip button, PlayButtonState state, bool showTooltip = false)
	{
		if (state == PlayButtonState.Hidden)
		{
			button.Hide();
		}
		else
		{
			button.Show(state == PlayButtonState.Enabled, showTooltip);
		}
	}

	public void SetViewStateInteractable(string viewState, bool interactable)
	{
		currentViewModel = ViewModelForState(viewState);
		currentViewModel.ButtonWithToolTip.Show(interactable, showTooltip: false);
	}

	public bool IsViewStateHidden(string viewState)
	{
		currentViewModel = ViewModelForState(viewState);
		return currentViewModel.ButtonWithToolTip.IsHidden();
	}

	public void SetStateAndShowButton(string viewState, EventEntryFeeInfo entryFee, bool interactable, bool showTooltip)
	{
		currentViewModel = ViewModelForState(viewState);
		ShowButton(entryFee, interactable, showTooltip);
	}

	public void ShowButton(EventEntryFeeInfo entryFee, bool interactable, bool showTooltip)
	{
		CustomButtonWithTooltip buttonWithToolTip = currentViewModel.ButtonWithToolTip;
		MainButtonComponentViewModel capturedViewModel = currentViewModel;
		buttonWithToolTip.Show(interactable, showTooltip);
		buttonWithToolTip.OnClick.RemoveAllListeners();
		buttonWithToolTip.OnClick.AddListener(delegate
		{
			if (capturedViewModel.AudioEventName != null)
			{
				AudioManager.PlayAudio(capturedViewModel.AudioEventName, base.gameObject);
			}
			PayJoinButton_OnClick?.Invoke(entryFee);
		});
	}

	public void UpdateTextWithQuantity(int feeQuantity)
	{
		currentViewModel.ButtonWithToolTip.SetText(feeQuantity.ToString());
	}

	public void UpdateTextWithQuantity(string locKey, int feeQuantity)
	{
		currentViewModel.ButtonWithToolTip.GetComponentInChildren<Localize>().SetText(locKey, new Dictionary<string, string> { 
		{
			"quantity",
			feeQuantity.ToString()
		} });
	}

	public void UpdateText(string locKey)
	{
		currentViewModel.ButtonWithToolTip.GetComponentInChildren<Localize>().SetText(locKey);
	}

	public void SetIconForButton(AssetLookupSystem assetLookupSystem, string lookupString)
	{
		if (currentViewModel.IconContainer != null)
		{
			AltAssetReference<TokenView> instance = TokenViewUtilities.TokenRefForId<EventViewTokenPayload, TokenView>(assetLookupSystem, lookupString);
			currentViewModel.IconContainer.SetInstance(instance);
		}
	}
}
