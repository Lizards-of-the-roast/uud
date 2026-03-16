using System.Collections.Generic;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene;

public class HoverEndHandler : IUpdate
{
	private readonly IWorkflowProvider _workflowProvider;

	private readonly ManaColorSelector _manaColorSelector;

	private readonly CombatAnimationPlayer _combatAnimationPlayer;

	private readonly ExamineViewCardHolder _examineCardController;

	private readonly IBrowserProvider _browserProvider;

	private readonly CardHoverController _hoverController;

	public HoverEndHandler(IWorkflowProvider workflowProvider, ManaColorSelector manaColorSelector, CombatAnimationPlayer combatAnimationPlayer, ExamineViewCardHolder examineCardController, IBrowserProvider browserProvider, CardHoverController hoverController)
	{
		_workflowProvider = workflowProvider;
		_manaColorSelector = manaColorSelector;
		_combatAnimationPlayer = combatAnimationPlayer;
		_examineCardController = examineCardController;
		_browserProvider = browserProvider;
		_hoverController = hoverController;
	}

	public void OnUpdate(float time)
	{
		if (!_hoverController.IsHovering)
		{
			return;
		}
		if (_manaColorSelector.IsOpen && !_manaColorSelector.CanHoverCards)
		{
			_hoverController.EndHover();
		}
		else if (_combatAnimationPlayer.IsPlaying)
		{
			_hoverController.EndHover();
		}
		else
		{
			if (!_browserProvider.IsBrowserVisible || !(_browserProvider.CurrentBrowser is CardBrowserBase cardBrowserBase))
			{
				return;
			}
			DuelScene_CDC hoveredCard = _hoverController.GetHoveredCard();
			if (!(hoveredCard == _examineCardController.ClonedCardView))
			{
				List<DuelScene_CDC> cardViews = cardBrowserBase.GetCardViews();
				SelectTargetsWorkflow selectTargetsWorkflow = _workflowProvider.GetCurrentWorkflow() as SelectTargetsWorkflow;
				if (!cardViews.Contains(hoveredCard) || (selectTargetsWorkflow != null && selectTargetsWorkflow.IsWaitingForRoundTrip()))
				{
					_hoverController.EndHover();
				}
			}
		}
	}
}
