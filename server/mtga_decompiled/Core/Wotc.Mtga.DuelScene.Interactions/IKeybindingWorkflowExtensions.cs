using UnityEngine;

namespace Wotc.Mtga.DuelScene.Interactions;

public static class IKeybindingWorkflowExtensions
{
	public static bool CanKeyUp(this WorkflowBase workflow, KeyCode key)
	{
		if (workflow is IKeybindingWorkflow keybindingWorkflow && keybindingWorkflow.CanKeyUp(key))
		{
			return true;
		}
		if (workflow is IParentWorkflow parentWorkflow)
		{
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanKeyUp(key))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void OnKeyUp(this WorkflowBase workflow, KeyCode key)
	{
		if (workflow is IKeybindingWorkflow keybindingWorkflow && keybindingWorkflow.CanKeyUp(key))
		{
			keybindingWorkflow.OnKeyUp(key);
		}
		else
		{
			if (!(workflow is IParentWorkflow parentWorkflow))
			{
				return;
			}
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanKeyUp(key))
				{
					childWorkflow.OnKeyUp(key);
					break;
				}
			}
		}
	}

	public static bool CanKeyDown(this WorkflowBase workflow, KeyCode key)
	{
		if (workflow is IKeybindingWorkflow keybindingWorkflow && keybindingWorkflow.CanKeyDown(key))
		{
			return true;
		}
		if (workflow is IParentWorkflow parentWorkflow)
		{
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanKeyDown(key))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void OnKeyDown(this WorkflowBase workflow, KeyCode key)
	{
		if (workflow is IKeybindingWorkflow keybindingWorkflow && keybindingWorkflow.CanKeyDown(key))
		{
			keybindingWorkflow.OnKeyDown(key);
		}
		else
		{
			if (!(workflow is IParentWorkflow parentWorkflow))
			{
				return;
			}
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanKeyDown(key))
				{
					childWorkflow.OnKeyDown(key);
					break;
				}
			}
		}
	}

	public static bool CanKeyHeld(this WorkflowBase workflow, KeyCode key, float holdDuration)
	{
		if (workflow is IKeybindingWorkflow keybindingWorkflow && keybindingWorkflow.CanKeyHeld(key, holdDuration))
		{
			return true;
		}
		if (workflow is IParentWorkflow parentWorkflow)
		{
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanKeyHeld(key, holdDuration))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void OnKeyHeld(this WorkflowBase workflow, KeyCode key, float holdDuration)
	{
		if (workflow is IKeybindingWorkflow keybindingWorkflow && keybindingWorkflow.CanKeyHeld(key, holdDuration))
		{
			keybindingWorkflow.OnKeyHeld(key, holdDuration);
		}
		else
		{
			if (!(workflow is IParentWorkflow parentWorkflow))
			{
				return;
			}
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				if (childWorkflow.CanKeyHeld(key, holdDuration))
				{
					childWorkflow.OnKeyHeld(key, holdDuration);
					break;
				}
			}
		}
	}
}
