using System.Collections.Generic;
using WorkflowVisuals;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class ConfirmChoice_Widget : WorkflowVariant
{
	private readonly CardHolderReference<StackCardHolder> _stack;

	private readonly ConfirmWidget _widget;

	private readonly IEntityView _sourceEntity;

	private readonly ConfirmWidget.Option[] _options;

	private readonly Dictionary<ConfirmWidget.Option, Action> _optionToActionMap = new Dictionary<ConfirmWidget.Option, Action>();

	public ConfirmChoice_Widget(ICardHolderProvider cardHolderProvider, ConfirmWidget widget, IEntityView sourceEntity, params (Action, ConfirmWidget.Option)[] actionOptionPairings)
	{
		_stack = CardHolderReference<StackCardHolder>.Stack(cardHolderProvider);
		_widget = widget;
		_sourceEntity = sourceEntity;
		int num = actionOptionPairings.Length;
		_options = new ConfirmWidget.Option[num];
		for (int i = 0; i < num; i++)
		{
			var (value, option) = actionOptionPairings[i];
			_options[i] = option;
			_optionToActionMap[option] = value;
		}
	}

	public override void Open()
	{
		_widget.Open(_sourceEntity as DuelScene_CDC, _options);
		_widget.OptionSelected += OnOptionSelected;
		if (_options.Length == 1)
		{
			_widget.Cancelled += Cancelled;
		}
		UpdateHighlights();
		UpdateButtons();
		_stack.Get().TryAutoDock(_widget.Buttons);
	}

	public override void Close()
	{
		_widget.OptionSelected -= OnOptionSelected;
		_widget.Cancelled -= Cancelled;
		if (_widget.IsOpen)
		{
			_widget.Close();
		}
		_stack.Get().ResetAutoDock();
		_stack.ClearCache();
		Submitted = null;
		Cancelled = null;
	}

	private void OnOptionSelected(ConfirmWidget.Option option)
	{
		if (_optionToActionMap.TryGetValue(option, out var value))
		{
			SelectedActions.Add(value);
			Submitted?.Invoke();
		}
	}

	private void OnCancelled()
	{
		Cancelled?.Invoke();
	}

	protected override void UpdateHighlights()
	{
		_highlights.Clear();
		_highlights.IdToHighlightType_Workflow[_sourceEntity.InstanceId] = HighlightType.Selected;
		if (_options.Length == 1 && _optionToActionMap.TryGetValue(_options[0], out var value))
		{
			foreach (uint item in value.AutoTapInstanceIds())
			{
				_highlights.IdToHighlightType_Workflow[item] = HighlightType.AutoPay;
			}
		}
		base.UpdateHighlights();
	}

	protected override void UpdateButtons()
	{
		_buttons = new Buttons
		{
			CancelData = new PromptButtonData
			{
				ButtonText = Utils.GetCancelLocKey(AllowCancel.Abort),
				Style = ButtonStyle.StyleType.Secondary,
				ButtonCallback = OnCancelled,
				ButtonSFX = WwiseEvents.sfx_ui_cancel.EventName,
				ClearsInteractions = false
			}
		};
		base.UpdateButtons();
	}
}
