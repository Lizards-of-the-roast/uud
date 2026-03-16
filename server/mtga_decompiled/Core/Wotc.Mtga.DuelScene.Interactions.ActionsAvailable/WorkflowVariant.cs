using System;
using System.Collections.Generic;
using WorkflowVisuals;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public abstract class WorkflowVariant
{
	public readonly List<Wotc.Mtgo.Gre.External.Messaging.Action> SelectedActions = new List<Wotc.Mtgo.Gre.External.Messaging.Action>();

	protected Highlights _highlights = new Highlights();

	protected Buttons _buttons = new Buttons();

	protected WorkflowPrompt _prompt = new WorkflowPrompt();

	public Action<Highlights> HighlightsUpdated;

	public Action<Arrows> ArrowsUpdated;

	public Action<Buttons> ButtonsUpdated;

	public Action<WorkflowPrompt> PromptUpdated;

	public System.Action Submitted;

	public System.Action Cancelled;

	public abstract void Open();

	public abstract void Close();

	public Highlights GetHighlights()
	{
		return _highlights;
	}

	public Buttons GetButtons()
	{
		return _buttons;
	}

	protected virtual void UpdateHighlights()
	{
		HighlightsUpdated?.Invoke(_highlights);
	}

	protected virtual void UpdateButtons()
	{
		ButtonsUpdated?.Invoke(_buttons);
	}

	protected virtual void UpdatePrompt()
	{
		PromptUpdated?.Invoke(_prompt);
	}
}
