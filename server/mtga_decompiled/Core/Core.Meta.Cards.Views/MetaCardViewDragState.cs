using System;
using Core.Code.Decks;
using Core.Shared.Code.CardFilters;
using Wizards.Mtga;

namespace Core.Meta.Cards.Views;

public class MetaCardViewDragState
{
	private MetaCardView _draggingCard;

	private bool _companionDragging;

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	private DeckBuilderCardFilterProvider FilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	public MetaCardView DraggingCard
	{
		get
		{
			return _draggingCard;
		}
		set
		{
			if (_draggingCard != value)
			{
				_draggingCard = value;
				this.OnDragStateChange?.Invoke(_draggingCard);
				OnDraggedCardChange(_draggingCard);
			}
		}
	}

	public event Action<MetaCardView> OnDragStateChange;

	public event Action<bool> CompanionAddButtonStateChange;

	public static MetaCardViewDragState Create()
	{
		return new MetaCardViewDragState();
	}

	private void OnDraggedCardChange(MetaCardView draggingCard)
	{
		bool? flag = null;
		if (draggingCard != null && !_companionDragging && CompanionUtil.CardCanBeCompanion(draggingCard.Card.Printing) && draggingCard.Card.Printing.GrpId != ModelProvider.Model.GetCompanion()?.GrpId)
		{
			_companionDragging = true;
			VisualsUpdater.UpdateCompanionView(_companionDragging);
			flag = true;
		}
		else if (draggingCard == null && _companionDragging)
		{
			_companionDragging = false;
			VisualsUpdater.UpdateCompanionView(_companionDragging);
			bool value = FilterProvider.Filter.IsSet(CardFilterType.Companions);
			flag = value;
		}
		if (flag.HasValue)
		{
			this.CompanionAddButtonStateChange?.Invoke(flag.Value);
		}
	}

	private MetaCardViewDragState()
	{
	}
}
