using System;
using System.Collections.Generic;
using GreClient.Rules;
using MovementSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.Interactions.SelectN;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class GraveyardCardHolder : ZoneCardHolderBase, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, IHoverableZone
{
	[SerializeField]
	private BoxCollider _inputCollider;

	[Header("Graveyard VFX")]
	[SerializeField]
	private Transform _effectRoot;

	[SerializeField]
	private ParticleSystem _effectSystem;

	private bool _updateStackedCardScaleOnNextPostLayout;

	public DuelScene_CDC TopCard { get; private set; }

	public event Action<MtgZone> Hovered;

	private void Awake()
	{
		if (_effectSystem != null)
		{
			_effectSystem.Stop(withChildren: true);
		}
	}

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		_orderType = IdOrderType.Reversed;
		CardLayout_General cardLayout_General = new CardLayout_General();
		cardLayout_General.Direction = CardLayout_General.SplayDirection.None;
		cardLayout_General.StrictSplayOffset = 0.02f;
		cardLayout_General.MinDegreesRotationOffset = -2f;
		cardLayout_General.MaxDegreesRotationOffset = 2f;
		base.Layout = cardLayout_General;
	}

	protected override void OnDestroy()
	{
		this.Hovered = null;
		base.OnDestroy();
	}

	protected override void OnPreLayout()
	{
		base.OnPreLayout();
		TopCard = null;
		if (_zoneModel != null)
		{
			_inputCollider.enabled = _zoneModel.CardIds.Count > 0 && base.CardViews.Count == 0;
		}
	}

	protected override void OnPostLayout()
	{
		base.OnPostLayout();
		UpdateEffectVFX();
		if (_updateStackedCardScaleOnNextPostLayout)
		{
			UpdateStackedCardScale();
			_updateStackedCardScaleOnNextPostLayout = false;
		}
	}

	protected override void HandleAddedCard(DuelScene_CDC cardView)
	{
		base.HandleAddedCard(cardView);
		if (cardView.PreviousCardHolder is CardBrowserCardHolder)
		{
			_updateStackedCardScaleOnNextPostLayout = true;
		}
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		if (_cardViews.Contains(cardView))
		{
			cardView.PartsRoot.localScale = Vector3.one;
			base.RemoveCard(cardView);
			UpdateStackedCardScale();
		}
	}

	protected override SplineEventData GetLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData layoutSplineEvents = base.GetLayoutSplineEvents(data);
		layoutSplineEvents.Events.Add(new SplineEventCallback(1f, OnSplineEventComplete));
		layoutSplineEvents.Events.Add(new SplineEventAudio(1f, new List<AudioEvent>
		{
			new AudioEvent(WwiseEvents.sfx_basicloc_discard.EventName)
		}));
		return layoutSplineEvents;
	}

	private void OnSplineEventComplete(float f)
	{
		UpdateStackedCardScale();
	}

	private void UpdateStackedCardScale()
	{
		for (int num = base.CardViews.Count - 1; num >= 0; num--)
		{
			if (num == base.CardViews.Count - 1)
			{
				_cardViews[num].PartsRoot.localScale = Vector3.one;
			}
			else
			{
				_cardViews[num].PartsRoot.localScale = new Vector3(1f, 1f, 0.1f);
			}
		}
	}

	private void UpdateEffectVFX()
	{
		if (_effectSystem == null || _effectRoot == null)
		{
			return;
		}
		int count = base.CardViews.Count;
		_effectRoot.transform.localPosition = Vector3.back * (count + 1) * (base.Layout as CardLayout_General).CardThickness;
		if (count == 0)
		{
			if (_effectSystem.isPlaying)
			{
				_effectSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
			}
		}
		else if (_effectSystem.isStopped)
		{
			_effectSystem.Play(withChildren: true);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		ViewGraveyard(_gameManager.InteractionSystem.HandleViewDismissCardClick);
	}

	public void ViewGraveyard(Action<DuelScene_CDC> onCardClicked)
	{
		List<DuelScene_CDC> graveyardCardViews = GetCardsInGraveyard();
		BaseUserRequest baseUserRequest = _gameManager.WorkflowController.CurrentWorkflow?.BaseRequest;
		ViewDismissBrowserProvider viewDismissBrowserProvider;
		if (baseUserRequest is PayCostsRequest payCostsRequest)
		{
			List<uint> list = payCostsRequest.EffectCost?.CostSelection?.Ids;
			if (list != null && list.Count > 0)
			{
				viewDismissBrowserProvider = GenerateProviderWithCardIds(list);
				goto IL_0088;
			}
		}
		viewDismissBrowserProvider = GenerateProvider();
		goto IL_0088;
		IL_0088:
		if (IsDelveAction(out var actionsAvailableReq, out var delveAction))
		{
			actionsAvailableReq.SubmitAction(delveAction);
			return;
		}
		if (_gameManager.LatestGameState.ActivePlayer.ClientPlayerEnum == playerNum && TryGetForageWorkflow(baseUserRequest, out var forageWorkflow))
		{
			forageWorkflow.GraveyardSelectCardModalBrowser();
			return;
		}
		IBrowser openedBrowser = _gameManager.BrowserManager.OpenBrowser(viewDismissBrowserProvider);
		viewDismissBrowserProvider.SetOpenedBrowser(openedBrowser);
		ViewDismissBrowserProvider GenerateProvider()
		{
			return new ViewDismissBrowserProvider(graveyardCardViews, null, getBrowserHeader2(), onCardClicked);
		}
		ViewDismissBrowserProvider GenerateProviderWithCardIds(List<uint> idList)
		{
			return new ViewDismissBrowserProvider(graveyardCardViews, null, getBrowserHeader(), onCardClicked, null, GREPlayerNum.Invalid, idList);
		}
		string getBrowserHeader()
		{
			IClientLocProvider activeLocProvider = Languages.ActiveLocProvider;
			return playerNum switch
			{
				GREPlayerNum.LocalPlayer => activeLocProvider.GetLocalizedText("Enum/ZoneType/ZoneType_Graveyard"), 
				GREPlayerNum.Opponent => activeLocProvider.GetLocalizedText("ZoneType_Opponent_Graveyard"), 
				_ => string.Empty, 
			};
		}
		string getBrowserHeader2()
		{
			IClientLocProvider activeLocProvider = Languages.ActiveLocProvider;
			return playerNum switch
			{
				GREPlayerNum.LocalPlayer => activeLocProvider.GetLocalizedText("Enum/ZoneType/ZoneType_Graveyard"), 
				GREPlayerNum.Opponent => activeLocProvider.GetLocalizedText("ZoneType_Opponent_Graveyard"), 
				_ => string.Empty, 
			};
		}
	}

	private bool TryGetForageWorkflow(BaseUserRequest req, out SelectNWorkflow_Selection_Weighted forageWorkflow)
	{
		if (req == null || req.Prompt == null)
		{
			forageWorkflow = null;
			return false;
		}
		WorkflowBase currentWorkflow = _gameManager.WorkflowController.CurrentWorkflow;
		forageWorkflow = IsForageWorkflow(currentWorkflow, req.Prompt.PromptId);
		return forageWorkflow != null;
	}

	private static SelectNWorkflow_Selection_Weighted IsForageWorkflow(WorkflowBase workflow, uint promptId)
	{
		if (workflow is SelectNWorkflow_Selection_Weighted result && SelectNWorkflow_Selection_Weighted.IsForage(promptId))
		{
			return result;
		}
		if (workflow is IParentWorkflow parentWorkflow)
		{
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				SelectNWorkflow_Selection_Weighted selectNWorkflow_Selection_Weighted = IsForageWorkflow(childWorkflow, promptId);
				if (selectNWorkflow_Selection_Weighted != null)
				{
					return selectNWorkflow_Selection_Weighted;
				}
			}
		}
		return null;
	}

	private List<DuelScene_CDC> GetCardsInGraveyard()
	{
		List<DuelScene_CDC> list = new List<DuelScene_CDC>();
		if (_zoneModel != null)
		{
			foreach (uint cardId in _zoneModel.CardIds)
			{
				if (_cardViewProvider.TryGetCardView(cardId, out var cardView))
				{
					list.Add(cardView);
				}
			}
		}
		return list;
	}

	private bool IsDelveAction(out ActionsAvailableRequest actionsAvailableReq, out Wotc.Mtgo.Gre.External.Messaging.Action delveAction)
	{
		if (_gameManager.WorkflowController?.CurrentWorkflow?.BaseRequest is PayCostsRequest payCostsRequest && payCostsRequest.ChildRequests.Count > 0)
		{
			foreach (BaseUserRequest childRequest in payCostsRequest.ChildRequests)
			{
				if (childRequest is ActionsAvailableRequest actionsAvailableRequest)
				{
					Wotc.Mtgo.Gre.External.Messaging.Action action = actionsAvailableRequest.Actions.Find((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.InstanceId == _zoneModel.Id && x.ActionType == ActionType.MakePayment);
					if (action != null)
					{
						actionsAvailableReq = actionsAvailableRequest;
						delveAction = action;
						return true;
					}
				}
			}
		}
		actionsAvailableReq = null;
		delveAction = null;
		return false;
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		this.Hovered?.Invoke(_zoneModel);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		this.Hovered?.Invoke(null);
	}
}
