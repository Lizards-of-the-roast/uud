using Wotc.Mtga.DuelScene.Browsers;

namespace Wotc.Mtga.DuelScene;

public class DragEndHandler : IUpdate
{
	private readonly CardDragController _dragController;

	private readonly ManaColorSelector _manaColorSelector;

	private readonly CombatAnimationPlayer _combatAnimationPlayer;

	private readonly IBrowserProvider _browserProvider;

	public DragEndHandler(CardDragController dragController, ManaColorSelector manaColorSelector, CombatAnimationPlayer combatAnimationPlayer, IBrowserProvider browserProvider)
	{
		_dragController = dragController;
		_manaColorSelector = manaColorSelector;
		_combatAnimationPlayer = combatAnimationPlayer;
		_browserProvider = browserProvider;
	}

	public void OnUpdate(float time)
	{
		if (!_dragController.IsDragging)
		{
			return;
		}
		if (_manaColorSelector.IsOpen || _combatAnimationPlayer.IsPlaying)
		{
			_dragController.EndDrag();
		}
		else if (_browserProvider.IsBrowserVisible && _browserProvider.CurrentBrowser is CardBrowserBase cardBrowserBase)
		{
			DuelScene_CDC draggedCard = _dragController.GetDraggedCard();
			if (!cardBrowserBase.GetCardViews().Contains(draggedCard))
			{
				_dragController.EndDrag();
			}
		}
	}
}
