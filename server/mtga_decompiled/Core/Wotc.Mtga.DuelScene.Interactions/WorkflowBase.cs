using System;
using System.Collections.Generic;
using GreClient.Rules;
using WorkflowVisuals;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions;

public abstract class WorkflowBase<T> : WorkflowBase where T : BaseUserRequest
{
	protected T _request;

	protected Prompt _prompt;

	public override BaseUserRequest BaseRequest => _request;

	public override Prompt Prompt => _prompt;

	public override RequestType Type => _request?.Type ?? RequestType.None;

	public override uint SourceId => _request?.SourceId ?? 0;

	protected WorkflowBase(T req)
	{
		_request = req;
		_prompt = req?.Prompt;
		_workflowPrompt.GrePrompt = _prompt;
	}

	public override void TryUndo()
	{
		_request?.Undo();
	}
}
public abstract class WorkflowBase
{
	protected IHighlightsGenerator _highlightsGenerator = NullHighlightsGenerator.Default;

	protected readonly WorkflowPrompt _workflowPrompt = new WorkflowPrompt();

	public InteractionAppliedState AppliedState { get; private set; }

	public abstract BaseUserRequest BaseRequest { get; }

	public abstract RequestType Type { get; }

	public abstract uint SourceId { get; }

	public abstract Prompt Prompt { get; }

	public Highlights Highlights { get; private set; } = new Highlights();

	public Arrows Arrows { get; protected set; } = new Arrows();

	public Dimming Dimming { get; protected set; } = new Dimming();

	public Buttons Buttons { get; protected set; } = new Buttons();

	public event Action<Highlights> UpdateHighlights;

	public event Action<Arrows> UpdateArrows;

	public event Action<Dimming> UpdateDimming;

	public event Action<Buttons> UpdateButtons;

	public event Action<WorkflowPrompt> UpdatePrompt;

	public virtual bool CanApply(List<UXEvent> events)
	{
		return !events.Exists((UXEvent x) => x.HasWeight);
	}

	public void ApplyInteraction()
	{
		AppliedState = InteractionAppliedState.Applied;
		ApplyInteractionInternal();
		SetPrompt();
		SetHighlights();
		SetDimming();
		SetArrows();
	}

	public abstract void TryUndo();

	public virtual void CleanUp()
	{
		Buttons?.Cleanup();
		_workflowPrompt?.Reset();
	}

	protected abstract void ApplyInteractionInternal();

	protected void UpdateHighlightsAndDimming()
	{
		SetHighlights();
		SetDimming();
	}

	protected virtual void SetPrompt()
	{
		_workflowPrompt.Reset();
		_workflowPrompt.GrePrompt = Prompt;
		OnUpdatePrompt(_workflowPrompt);
	}

	protected void SetHighlights()
	{
		Highlights = _highlightsGenerator.GetHighlights();
		OnUpdateHighlights(Highlights);
	}

	protected virtual void SetDimming()
	{
		OnUpdateDimming(Dimming);
	}

	protected virtual void SetArrows()
	{
		OnUpdateArrows(Arrows);
	}

	protected virtual void SetButtons()
	{
		this.UpdateButtons?.Invoke(Buttons);
	}

	protected void OnUpdateHighlights(Highlights highlights)
	{
		this.UpdateHighlights?.Invoke(highlights);
	}

	protected void OnUpdateDimming(Dimming dimming)
	{
		this.UpdateDimming?.Invoke(dimming);
	}

	protected void OnUpdateArrows(Arrows arrows)
	{
		this.UpdateArrows?.Invoke(arrows);
	}

	protected void OnUpdateButtons(Buttons buttons)
	{
		this.UpdateButtons?.Invoke(buttons);
	}

	protected void OnUpdatePrompt(WorkflowPrompt prompt)
	{
		this.UpdatePrompt?.Invoke(prompt);
	}

	public void UpdateArrowsPublic()
	{
		SetArrows();
	}

	protected static bool TryRerouteClick(uint instanceId, IBattlefieldCardHolder battlefieldCardHolder, bool isSelecting, out IEntityView reroutedEntityView)
	{
		bool result = false;
		IBattlefieldStack stackForInstanceId = battlefieldCardHolder.GetStackForInstanceId(instanceId);
		reroutedEntityView = null;
		if (CanRerouteClick(stackForInstanceId))
		{
			if (isSelecting)
			{
				DuelScene_CDC oldestCard = stackForInstanceId.OldestCard;
				if ((object)oldestCard != null)
				{
					reroutedEntityView = oldestCard;
					result = true;
					goto IL_003b;
				}
			}
			if (!isSelecting)
			{
				DuelScene_CDC youngestCard = stackForInstanceId.YoungestCard;
				if ((object)youngestCard != null)
				{
					reroutedEntityView = youngestCard;
					result = true;
				}
			}
		}
		goto IL_003b;
		IL_003b:
		return result;
	}

	public static bool CanRerouteClick(IBattlefieldStack battlefieldStack)
	{
		if (battlefieldStack == null)
		{
			return false;
		}
		if (battlefieldStack.HasAttachmentOrExile)
		{
			return false;
		}
		if (battlefieldStack.IsBlockStack)
		{
			return false;
		}
		if (StackParentIsDistinguished(battlefieldStack))
		{
			return false;
		}
		return true;
	}

	private static bool StackParentIsDistinguished(IBattlefieldStack stack)
	{
		if (stack == null)
		{
			return false;
		}
		return stack.StackParentModel?.Instance?.DistinguishedByIds.Count > 0;
	}
}
