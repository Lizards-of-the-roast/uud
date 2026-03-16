using System;
using MovementSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Cards.Parts.Textbox;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.Universal;
using Wotc.Mtgo.Gre.External.Messaging;

namespace InteractionSystem;

public class GameInteractionSystem : IDisposable
{
	private class InteractionData
	{
		public readonly DuelScene_CDC ClickedCard;

		public readonly SimpleInteractionType InteractionType;

		public InteractionData(DuelScene_CDC clickedCard, SimpleInteractionType interactionType)
		{
			ClickedCard = clickedCard;
			InteractionType = interactionType;
		}
	}

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IHighlightController _highlightController;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IWorkflowProvider _workflowProvider;

	private readonly StaticZoomController _staticHoverCardHolder;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly ISplineMovementSystem _splineMovementSystem;

	private readonly CombatAnimationPlayer _combatAnimationPlayer;

	private readonly ExamineViewCardHolder _examineCardController;

	private readonly BrowserManager _browserManager;

	private readonly HangerController _hangerController;

	private readonly CardHolderReference<IBattlefieldCardHolder> _battlefield;

	private InteractionData _currentInteraction;

	private bool _shouldDismissExamineView;

	public readonly CardHoverController HoverController;

	public readonly CardDragController DragController;

	public GameInteractionSystem(ICardDatabaseAdapter cardDatabase, IHighlightController highlightController, IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider, ICardHolderProvider cardHolderProvider, StaticZoomController staticZoomController, ICardViewProvider cardViewProvider, CombatAnimationPlayer combatAnimationPlayer, ExamineViewCardHolder examineCardHolder, ISplineMovementSystem splineMovementSystem, BrowserManager browserManager, CardHoverController cardHoverController, CardDragController cardDragController, HangerController hangerController)
	{
		_cardDatabase = cardDatabase;
		_highlightController = highlightController;
		_gameStateProvider = gameStateProvider;
		_workflowProvider = workflowProvider;
		_staticHoverCardHolder = staticZoomController;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_combatAnimationPlayer = combatAnimationPlayer;
		_examineCardController = examineCardHolder;
		_splineMovementSystem = splineMovementSystem;
		_browserManager = browserManager;
		HoverController = cardHoverController;
		DragController = cardDragController;
		_hangerController = hangerController;
		_battlefield = CardHolderReference<IBattlefieldCardHolder>.Battlefield(cardHolderProvider);
	}

	private void OpenAttachmentStackOverflowViewer(BattlefieldCardHolder.BattlefieldStack cardStack, Action<DuelScene_CDC> onCardSelected)
	{
		if (cardStack?.StackParentModel?.Instance != null && cardStack.StackParentModel.Instance.AttachedWithIds.Count != 0)
		{
			_examineCardController.Dismiss();
			AttachmentAndExileStackBrowserProvider attachmentAndExileStackBrowserProvider = new AttachmentAndExileStackBrowserProvider(cardStack.StackParentModel, _workflowProvider.GetCurrentWorkflow(), _cardViewProvider, _cardDatabase.GreLocProvider, _highlightController, HoverController, _gameStateProvider.LatestGameState, onCardSelected);
			IBrowser openedBrowser = _browserManager.OpenBrowser(attachmentAndExileStackBrowserProvider);
			attachmentAndExileStackBrowserProvider.SetOpenedBrowser(openedBrowser);
		}
	}

	public void OnCardDown(DuelScene_CDC cardView, PointerEventData eventData)
	{
		if ((cardView.HolderType == CardHolderType.Hand && cardView.CurrentCardHolder.PlayerNum == GREPlayerNum.Opponent && !cardView.Model.IsDisplayedFaceDown) || cardView.HolderType != CardHolderType.Hand || cardView.CurrentCardHolder.CardHolderType == CardHolderType.CardBrowserDefault || cardView.CurrentCardHolder.CardHolderType == CardHolderType.CardBrowserViewDismiss)
		{
			_staticHoverCardHolder?.OnCardDown(cardView, eventData);
		}
		if (PlatformUtils.IsHandheld() && cardView.HolderType != CardHolderType.Hand && cardView.Model.IsDisplayedFaceDown)
		{
			HoverController.HandleMobileTap(cardView);
		}
	}

	public void OnCardUp()
	{
		_staticHoverCardHolder?.ClearClickAndHoldProperties();
	}

	public void OnCardClicked(DuelScene_CDC cardView, SimpleInteractionType interactionType)
	{
		_shouldDismissExamineView = false;
		if (_currentInteraction == null)
		{
			_currentInteraction = new InteractionData(cardView, interactionType);
		}
	}

	public void ProcessInteraction()
	{
		if (_currentInteraction != null)
		{
			HandleCardViewClick(_currentInteraction.ClickedCard, _currentInteraction.InteractionType);
			_currentInteraction = null;
		}
		else if (_shouldDismissExamineView)
		{
			_examineCardController?.Dismiss();
			_shouldDismissExamineView = false;
		}
	}

	public void OnStackClicked(CdcStackCounterView cdcStackCounterView, SimpleInteractionType interactionType)
	{
		_examineCardController.Dismiss();
		WorkflowBase currentWorkflow = _workflowProvider.GetCurrentWorkflow();
		if (currentWorkflow != null)
		{
			if (currentWorkflow.CanClickStack(cdcStackCounterView, interactionType))
			{
				currentWorkflow.OnClickStack(cdcStackCounterView, interactionType);
			}
			else
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid.EventName, cdcStackCounterView.gameObject);
			}
		}
	}

	private void HandleCardViewClick(DuelScene_CDC cardView, SimpleInteractionType interactionType, bool automaticEvent = false)
	{
		if (!cardView || cardView.Model == null)
		{
			_examineCardController.Dismiss();
			return;
		}
		if (_browserManager != null && _browserManager.IsBrowserVisible && (interactionType == SimpleInteractionType.Primary || interactionType == SimpleInteractionType.DoublePrimary))
		{
			_examineCardController.Dismiss();
			(_browserManager.CurrentBrowser as ICardBrowser)?.OnCardViewSelected(cardView);
			return;
		}
		if (cardView.CurrentCardHolder != null && cardView.CurrentCardHolder.CardHolderType == CardHolderType.Reveal && cardView.CurrentCardHolder is RevealCardHolder revealCardHolder)
		{
			_examineCardController.Dismiss();
			revealCardHolder.SwitchToBrowser();
			return;
		}
		if (CardDragController.DraggedCard == cardView && interactionType == SimpleInteractionType.Primary)
		{
			_examineCardController.Dismiss();
			ExecuteEndDrag(cardView, cardView);
			return;
		}
		IBattlefieldCardHolder battlefieldCardHolder = _battlefield.Get();
		if ((interactionType == SimpleInteractionType.Primary || interactionType == SimpleInteractionType.DoublePrimary) && battlefieldCardHolder is UniversalBattlefieldCardHolder universalBattlefieldCardHolder && universalBattlefieldCardHolder.HandleCardClick(cardView))
		{
			_examineCardController.Dismiss();
			return;
		}
		WorkflowBase currentWorkflow = _workflowProvider.GetCurrentWorkflow();
		if ((interactionType == SimpleInteractionType.Primary || interactionType == SimpleInteractionType.DoublePrimary) && battlefieldCardHolder?.GetStackForCard(cardView) is BattlefieldCardHolder.BattlefieldStack battlefieldStack && battlefieldStack.StackParent != cardView && battlefieldStack.StackParentModel.Instance.BlockState != BlockState.Blocking && battlefieldStack.AttachmentCount + battlefieldStack.ExileCount > 1 && !(currentWorkflow is IAttachmentWorkflow))
		{
			_examineCardController.Dismiss();
			OpenAttachmentStackOverflowViewer(battlefieldStack, HandleViewDismissCardClick);
			return;
		}
		if (currentWorkflow.CanClick(cardView, interactionType))
		{
			if (CardHoverController.HoveredCard == cardView)
			{
				HoverController.EndHover();
			}
			if (DragController.IsDragging && CardDragController.DraggedCard == cardView)
			{
				ExecuteEndDrag(cardView, cardView);
			}
			currentWorkflow.OnClick(cardView, interactionType);
			_examineCardController.Dismiss();
			return;
		}
		switch (interactionType)
		{
		case SimpleInteractionType.Primary:
			_examineCardController.Dismiss();
			if (cardView.CurrentCardHolder != null)
			{
				if (!automaticEvent && !PlatformUtils.IsHandheld() && CanBeginDrag(cardView))
				{
					ExecuteBeginDrag(cardView);
					return;
				}
				if (cardView.CurrentCardHolder.CardHolderType == CardHolderType.Graveyard)
				{
					(cardView.CurrentCardHolder as GraveyardCardHolder).ViewGraveyard(HandleViewDismissCardClick);
					return;
				}
				if (cardView.CurrentCardHolder.CardHolderType == CardHolderType.Library)
				{
					(cardView.CurrentCardHolder as LibraryCardHolder).ViewLibrary(HandleViewDismissCardClick);
					return;
				}
			}
			break;
		case SimpleInteractionType.Secondary:
			if (_examineCardController.ClonedCardView == cardView)
			{
				_examineCardController.CycleFaceHangers();
			}
			else
			{
				_examineCardController.ExamineCard(cardView);
			}
			return;
		}
		_examineCardController.Dismiss();
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid.EventName, cardView.gameObject);
	}

	public void HandleViewDismissCardClick(DuelScene_CDC cardView)
	{
		WorkflowBase currentWorkflow = _workflowProvider.GetCurrentWorkflow();
		if (currentWorkflow != null)
		{
			if (currentWorkflow.CanClick(cardView, SimpleInteractionType.Primary))
			{
				currentWorkflow.OnClick(cardView, SimpleInteractionType.Primary);
			}
			else
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid.EventName, cardView.gameObject);
			}
		}
	}

	public void OnBattlefieldClicked()
	{
		if (DragController != null && DragController.IsDragging)
		{
			DragController.EndDrag();
		}
		_workflowProvider.GetCurrentWorkflow().OnBattlefieldClick();
		_examineCardController.Dismiss();
	}

	public bool CanBeginDrag(DuelScene_CDC cardView)
	{
		if (DragController.IsDragging)
		{
			return false;
		}
		if (cardView.Model.IsDisplayedFaceDown)
		{
			return false;
		}
		if (cardView.CurrentCardHolder.PlayerNum == GREPlayerNum.Opponent)
		{
			return false;
		}
		switch (cardView.CurrentCardHolder.CardHolderType)
		{
		case CardHolderType.Hand:
			return true;
		case CardHolderType.CardBrowserDefault:
		case CardHolderType.CardBrowserViewDismiss:
			if (_browserManager.IsAnyBrowserOpen && _browserManager.CurrentBrowser is CardBrowserBase cardBrowserBase)
			{
				return cardBrowserBase.AllowsDragInteractions(cardView);
			}
			return false;
		case CardHolderType.Library:
		{
			if (TryGetActionsAvailableWorkflow(_workflowProvider.GetCurrentWorkflow(), out var result))
			{
				return result.GetActionsForId(cardView.InstanceId).Count > 0;
			}
			return false;
		}
		default:
			if (_workflowProvider.GetCurrentWorkflow() is IDraggableWorkflow draggableWorkflow)
			{
				return draggableWorkflow.CanCommenceDrag(cardView);
			}
			return false;
		}
		static bool TryGetActionsAvailableWorkflow(WorkflowBase workflow, out ActionsAvailableWorkflow reference)
		{
			if (workflow is ActionsAvailableWorkflow actionsAvailableWorkflow)
			{
				reference = actionsAvailableWorkflow;
				return true;
			}
			if (workflow is IParentWorkflow parentWorkflow)
			{
				foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
				{
					if (TryGetActionsAvailableWorkflow(childWorkflow, out reference))
					{
						return true;
					}
				}
			}
			reference = null;
			return false;
		}
	}

	public void ExecuteBeginDrag(DuelScene_CDC cardView)
	{
		_examineCardController.Dismiss();
		if (!(_examineCardController.ClonedCardView == cardView) && _splineMovementSystem.InteractionsAreAllowed(cardView.Root))
		{
			if (!_browserManager.IsAnyBrowserOpen && _workflowProvider.GetCurrentWorkflow() is IDraggableWorkflow draggableWorkflow && draggableWorkflow.CanCommenceDrag(cardView))
			{
				draggableWorkflow.OnDragCommenced(cardView);
			}
			DragController.BeginDrag(cardView);
		}
	}

	public void ExecuteEndDrag(DuelScene_CDC cardView, IEntityView endEntityView)
	{
		if (!(_examineCardController.ClonedCardView == cardView) && DragController.IsDragging && !(CardDragController.DraggedCard != cardView) && (!_browserManager.IsBrowserVisible || !(_browserManager.CurrentBrowser is CardBrowserBase cardBrowserBase) || cardBrowserBase.AllowsDragInteractions(cardView)))
		{
			DragController.EndDrag();
			if (_workflowProvider.GetCurrentWorkflow() is IDraggableWorkflow draggableWorkflow && draggableWorkflow.CanCompleteDrag(endEntityView))
			{
				draggableWorkflow.OnDragCompleted(cardView, endEntityView);
			}
		}
	}

	public void HandleHover(DuelScene_CDC cardView, PointerEventData eventData)
	{
		HandleHover(cardView, eventData.pointerPress);
	}

	public void HandleHover(DuelScene_CDC cardView, GameObject hoverTarget)
	{
		if ((_examineCardController.ClonedCardView == cardView || !_browserManager.IsBrowserVisible || ((!(_browserManager.CurrentBrowser is CardBrowserBase cardBrowserBase) || (cardBrowserBase.AllowsHoverInteractions && cardBrowserBase.GetCardViews().Contains(cardView))) && (cardView.HolderType != CardHolderType.Hand || !PlatformUtils.IsHandheld() || !(_browserManager.CurrentBrowser is ViewDismissBrowser { PlayerNum: GREPlayerNum.Opponent })))) && (cardView.HolderType != CardHolderType.Hand || cardView.CurrentCardHolder.PlayerNum != GREPlayerNum.Opponent || !PlatformUtils.IsHandheld()) && _splineMovementSystem.InteractionsAreAllowed(cardView.Root) && (!_combatAnimationPlayer.IsPlaying || !_combatAnimationPlayer.SourceTransformsInCombat.Contains(cardView.Root)) && (!(cardView.CurrentCardHolder is LibraryCardHolder) || !((cardView.CurrentCardHolder as LibraryCardHolder).TopCard != cardView)) && (!(hoverTarget != null) || !(hoverTarget != cardView.gameObject) || cardView.CurrentCardHolder.CardHolderType != CardHolderType.Hand) && !DragController.IsDragging && (_staticHoverCardHolder == null || cardView.HolderType == CardHolderType.Hand))
		{
			HoverController.BeginHover(cardView);
		}
	}

	public void HandleHoverEnd(DuelScene_CDC cardView)
	{
		if (HoverController.IsHovering && !(CardHoverController.HoveredCard != cardView))
		{
			bool flag = cardView.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield && cardView == CardDragController.DraggedCard;
			if (!DragController.IsDragging || flag)
			{
				HoverController.EndHover();
			}
		}
	}

	public void HandleScroll(DuelScene_CDC cardView, PointerEventData eventData)
	{
		if (cardView == null || !eventData.IsScrolling())
		{
			return;
		}
		BASE_CDC bASE_CDC = ((!(cardView == CardHoverController.HoveredCard)) ? cardView : ((HoverController.HoverCardCopy != null) ? HoverController.HoverCardCopy : CardHoverController.HoveredCard));
		if ((bool)bASE_CDC)
		{
			foreach (CDCPart_Textbox_SuperBase item in bASE_CDC.FindAllParts<CDCPart_Textbox_SuperBase>(AnchorPointType.Invalid))
			{
				item.ScrollTextbox(eventData.scrollDelta);
			}
		}
		if (_hangerController.ActiveCard == cardView || _hangerController.ActiveCard == CardHoverController.HoveredCard || _hangerController.ActiveCard == HoverController.HoverCardCopy)
		{
			_hangerController.Scroll(eventData.scrollDelta);
		}
		if (_examineCardController.ClonedCardView == cardView)
		{
			_examineCardController.HangerController.Scroll(eventData.scrollDelta);
		}
	}

	public bool CancelAnyDrag()
	{
		bool result = false;
		if (DragController.IsDragging)
		{
			DragController.EndDrag();
			result = true;
		}
		return result;
	}

	public void FlagExamineViewForDismissal()
	{
		_shouldDismissExamineView = true;
	}

	public void Dispose()
	{
		_battlefield.ClearCache();
	}
}
