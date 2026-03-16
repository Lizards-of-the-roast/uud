using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ManaColorSelection : WorkflowVariant
{
	private readonly IAbilityDataProvider _abilityDatabase;

	private readonly ManaColorSelector _manaColorSelector;

	private readonly CardHolderManager _cardHolderManager;

	public ManaColorSelection(IAbilityDataProvider abilityDatabase, ManaColorSelector manaColorSelector, CardHolderManager cardHolderManager)
	{
		_abilityDatabase = abilityDatabase ?? NullAbilityDataProvider.Default;
		_manaColorSelector = manaColorSelector;
		_cardHolderManager = cardHolderManager;
		manaColorSelector.TryCloseSelector();
	}

	public bool UseColorPicker(params Action[] actions)
	{
		return ActionsAvailableWorkflowUtils_ColorPicker.CanUseColorPicker(_abilityDatabase, actions);
	}

	public void ShowColorSelection(DuelScene_CDC cdc, params Action[] actions)
	{
		SelectedActions.Clear();
		ActionsAvailableWorkflowUtils_ColorPicker.ShowManaColorSelection(cdc, actions, OnColorPickerSubmit, _manaColorSelector, _abilityDatabase, _cardHolderManager);
	}

	private void OnColorPickerSubmit(GreInteraction submittedAction)
	{
		Action greAction = submittedAction.GreAction;
		SelectedActions.Add(greAction);
		Submitted?.Invoke();
		if (_manaColorSelector.IsOpen)
		{
			_manaColorSelector.CloseSelector();
		}
	}

	public override void Open()
	{
	}

	public override void Close()
	{
		if (_manaColorSelector.IsOpen)
		{
			_manaColorSelector.CloseSelector();
		}
		Submitted = null;
		Cancelled = null;
	}
}
