using System.Collections.Generic;

namespace WorkflowVisuals;

public class Buttons
{
	public List<PromptButtonData> WorkflowButtons = new List<PromptButtonData>();

	public PromptButtonData CancelData;

	public PromptButtonData UndoData;

	public bool DisplayUndo = true;

	public static void Merge(Buttons lhs, Buttons rhs)
	{
		lhs.WorkflowButtons.AddRange(rhs.WorkflowButtons);
		lhs.CancelData = rhs.CancelData;
		lhs.UndoData = rhs.UndoData;
		lhs.DisplayUndo &= rhs.DisplayUndo;
	}

	public void Cleanup()
	{
		DisplayUndo = true;
		if (WorkflowButtons != null)
		{
			foreach (PromptButtonData workflowButton in WorkflowButtons)
			{
				workflowButton.ButtonCallback = null;
			}
			WorkflowButtons.Clear();
		}
		if (CancelData != null)
		{
			CancelData.ButtonCallback = null;
			CancelData = null;
		}
		if (UndoData != null)
		{
			UndoData.ButtonCallback = null;
			UndoData = null;
		}
	}
}
