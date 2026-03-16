using System;
using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class BatchManaSubmission : WorkflowVariant, IKeybindingWorkflow, IClickableWorkflow, ICardStackWorkflow
{
	private readonly ManaColorSelection _manaColorSelection;

	private readonly IBattlefieldCardHolder _battlefield;

	private readonly Dictionary<uint, List<Wotc.Mtgo.Gre.External.Messaging.Action>> _manaActions = new Dictionary<uint, List<Wotc.Mtgo.Gre.External.Messaging.Action>>();

	private readonly Dictionary<ManaPaymentOption, int> _mpoIdx = new Dictionary<ManaPaymentOption, int>();

	public BatchManaSubmission(ManaColorSelection manaColorSelection, IBattlefieldCardHolder battlefield, List<Wotc.Mtgo.Gre.External.Messaging.Action> manaActions)
	{
		_manaColorSelection = manaColorSelection;
		_battlefield = battlefield;
		foreach (Wotc.Mtgo.Gre.External.Messaging.Action manaAction in manaActions)
		{
			uint instanceId = manaAction.InstanceId;
			if (_manaActions.TryGetValue(instanceId, out var value))
			{
				value.Add(manaAction);
			}
			else
			{
				_manaActions[instanceId] = new List<Wotc.Mtgo.Gre.External.Messaging.Action> { manaAction };
			}
			IList<ManaPaymentOption> manaPaymentOptions = manaAction.ManaPaymentOptions;
			for (int i = 0; i < manaPaymentOptions.Count; i++)
			{
				_mpoIdx[manaPaymentOptions[i]] = i;
			}
		}
		Application.focusChanged += OnFocusChanged;
	}

	private void OnFocusChanged(bool focused)
	{
		Cancelled?.Invoke();
	}

	public override void Open()
	{
		UpdateHighlights();
		UpdateButtons();
	}

	public override void Close()
	{
		_manaColorSelection.Close();
		_manaActions.Clear();
		SelectedActions.Clear();
		Submitted = null;
		Cancelled = null;
		Application.focusChanged -= OnFocusChanged;
	}

	protected override void UpdateHighlights()
	{
		_highlights.Clear();
		foreach (uint key in _manaActions.Keys)
		{
			_highlights.IdToHighlightType_Workflow[key] = HighlightType.Hot;
		}
		foreach (Wotc.Mtgo.Gre.External.Messaging.Action selectedAction in SelectedActions)
		{
			_highlights.IdToHighlightType_Workflow[selectedAction.InstanceId] = HighlightType.Selected;
		}
		base.UpdateHighlights();
	}

	public bool CanKeyDown(KeyCode key)
	{
		return true;
	}

	public void OnKeyDown(KeyCode key)
	{
		if (key == KeyCode.Escape)
		{
			Cancelled?.Invoke();
		}
	}

	public bool CanKeyHeld(KeyCode key, float holdDuration)
	{
		return true;
	}

	public void OnKeyHeld(KeyCode key, float holdDuration)
	{
	}

	public bool CanKeyUp(KeyCode key)
	{
		return true;
	}

	public void OnKeyUp(KeyCode key)
	{
		if (key == KeyCode.Q)
		{
			Submitted?.Invoke();
		}
	}

	public bool CanClick(IEntityView entity, SimpleInteractionType clickType)
	{
		if (entity is DuelScene_CDC)
		{
			return _manaActions.ContainsKey(entity.InstanceId);
		}
		return false;
	}

	public void OnClick(IEntityView entity, SimpleInteractionType clickType)
	{
		uint instanceId = entity.InstanceId;
		IBattlefieldStack stackForInstanceId = _battlefield.GetStackForInstanceId(instanceId);
		if (stackForInstanceId == null)
		{
			return;
		}
		ICardDataAdapter stackParentModel = stackForInstanceId.StackParentModel;
		if (stackParentModel == null)
		{
			return;
		}
		uint parentId = stackParentModel.InstanceId;
		List<Wotc.Mtgo.Gre.External.Messaging.Action> value;
		if (SelectedActions.Exists((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.InstanceId == parentId))
		{
			foreach (DuelScene_CDC cdc in stackForInstanceId.AllCards)
			{
				if ((bool)cdc)
				{
					SelectedActions.RemoveAll((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.InstanceId == cdc.InstanceId);
				}
			}
			UpdateHighlights();
			UpdateButtons();
		}
		else if (_manaActions.TryGetValue(parentId, out value))
		{
			if (_manaColorSelection.UseColorPicker(value.ToArray()))
			{
				ManaColorSelection manaColorSelection = _manaColorSelection;
				manaColorSelection.Submitted = (System.Action)Delegate.Combine(manaColorSelection.Submitted, new System.Action(OnManaSelectionSubmitted));
				_manaColorSelection.ShowColorSelection(entity as DuelScene_CDC, value.ToArray());
			}
			else
			{
				BatchActionsForCardStack(stackForInstanceId, value[0]);
			}
		}
	}

	public bool CanClickStack(CdcStackCounterView entity, SimpleInteractionType clickType)
	{
		return false;
	}

	public void OnClickStack(CdcStackCounterView entity)
	{
	}

	public void OnBattlefieldClick()
	{
	}

	private void OnManaSelectionSubmitted()
	{
		ManaColorSelection manaColorSelection = _manaColorSelection;
		manaColorSelection.Submitted = (System.Action)Delegate.Remove(manaColorSelection.Submitted, new System.Action(OnManaSelectionSubmitted));
		List<Wotc.Mtgo.Gre.External.Messaging.Action> selectedActions = _manaColorSelection.SelectedActions;
		if (selectedActions.Count > 0)
		{
			Wotc.Mtgo.Gre.External.Messaging.Action action = selectedActions[0];
			IBattlefieldStack stackForInstanceId = _battlefield.GetStackForInstanceId(action.InstanceId);
			BatchActionsForCardStack(stackForInstanceId, action);
		}
	}

	private void BatchActionsForCardStack(IBattlefieldStack stack, Wotc.Mtgo.Gre.External.Messaging.Action srcAction)
	{
		if (stack == null || srcAction == null)
		{
			return;
		}
		IList<ManaPaymentOption> manaPaymentOptions = srcAction.ManaPaymentOptions;
		if (manaPaymentOptions == null || manaPaymentOptions.Count == 0)
		{
			return;
		}
		if (_mpoIdx.TryGetValue(manaPaymentOptions[0], out var value))
		{
			foreach (DuelScene_CDC allCard in stack.AllCards)
			{
				if (_manaActions.TryGetValue(allCard.InstanceId, out var value2))
				{
					Wotc.Mtgo.Gre.External.Messaging.Action action = value2.Find((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.AbilityGrpId == srcAction.AbilityGrpId);
					if (action != null)
					{
						SelectedActions.Add(BatchedAction(action, value));
					}
				}
			}
		}
		_battlefield.LayoutNow();
		UpdateHighlights();
		UpdateButtons();
	}

	private Wotc.Mtgo.Gre.External.Messaging.Action BatchedAction(Wotc.Mtgo.Gre.External.Messaging.Action original, int mpoIdx)
	{
		Wotc.Mtgo.Gre.External.Messaging.Action action = new Wotc.Mtgo.Gre.External.Messaging.Action(original);
		IList<ManaPaymentOption> manaPaymentOptions = action.ManaPaymentOptions;
		for (int num = manaPaymentOptions.Count - 1; num >= 0; num--)
		{
			if (num != mpoIdx)
			{
				manaPaymentOptions.RemoveAt(num);
			}
		}
		return action;
	}

	public bool CanStack(ICardDataAdapter lhs, ICardDataAdapter rhs)
	{
		bool num = SelectedActions.Exists((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.InstanceId == lhs.InstanceId);
		bool flag = SelectedActions.Exists((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.InstanceId == rhs.InstanceId);
		return num == flag;
	}
}
