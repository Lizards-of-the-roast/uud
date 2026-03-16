using System.Collections.Generic;
using System.Text;
using GreClient.Rules;
using WorkflowVisuals;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Interactions.CastingTimeOption;

public class CastingTimeOptionWorkflow : WorkflowBase<CastingTimeOptionRequest>, IParentWorkflow, IAutoRespondWorkflow
{
	private readonly IAbilityDataProvider _abilityProvider;

	private readonly List<WorkflowBase> _children = new List<WorkflowBase>();

	public IEnumerable<WorkflowBase> ChildWorkflows => _children;

	public CastingTimeOptionWorkflow(CastingTimeOptionRequest request, IAbilityDataProvider abilityDataProvider, IWorkflowTranslation<CastingTimeOptionRequest> childTranslation)
		: base(request)
	{
		_abilityProvider = abilityDataProvider ?? NullAbilityDataProvider.Default;
		WorkflowBase workflowBase = childTranslation.Translate(request);
		if (workflowBase != null)
		{
			workflowBase.UpdateHighlights += OnUpdateChildHighlights;
			workflowBase.UpdateDimming += OnUpdateChildDimming;
			workflowBase.UpdateArrows += OnUpdateChildArrows;
			workflowBase.UpdateButtons += OnUpdateChildButtons;
			_prompt = workflowBase.Prompt;
			_children.AddIfNotNull(workflowBase);
		}
		_highlightsGenerator = new HighlightGeneratorAggregate(_children);
	}

	private void OnUpdateChildHighlights(Highlights highlights)
	{
		SetHighlights();
	}

	private void OnUpdateChildArrows(Arrows arrows)
	{
		SetArrows();
	}

	private void OnUpdateChildDimming(Dimming dimming)
	{
		SetDimming();
	}

	private void OnUpdateChildButtons(Buttons buttons)
	{
		SetButtons();
	}

	public override bool CanApply(List<UXEvent> events)
	{
		foreach (WorkflowBase child in _children)
		{
			if (!child.CanApply(events))
			{
				return false;
			}
		}
		return true;
	}

	public bool TryAutoRespond()
	{
		foreach (BaseUserRequest childRequest in _request.ChildRequests)
		{
			if (childRequest is CastingTimeOption_SelectNRequest castingTimeOption_SelectNRequest && _abilityProvider.TryGetAbilityPrintingById(castingTimeOption_SelectNRequest.GrpId, out var ability) && ability.BaseId == 147)
			{
				return false;
			}
		}
		foreach (WorkflowBase childWorkflow in ChildWorkflows)
		{
			if (childWorkflow is IAutoRespondWorkflow autoRespondWorkflow && autoRespondWorkflow.TryAutoRespond())
			{
				return true;
			}
		}
		return false;
	}

	protected override void ApplyInteractionInternal()
	{
		_children.ForEach(delegate(WorkflowBase x)
		{
			x.ApplyInteraction();
		});
	}

	protected override void SetDimming()
	{
		base.Dimming.IdToIsDimmed.Clear();
		foreach (WorkflowBase child in _children)
		{
			Dimming.Merge(base.Dimming, child.Dimming);
		}
		OnUpdateDimming(base.Dimming);
	}

	protected override void SetArrows()
	{
		base.Arrows.ClearLines();
		base.Arrows.ClearCtMLines();
		base.Arrows = new Arrows();
		foreach (WorkflowBase child in _children)
		{
			Arrows.Merge(base.Arrows, child.Arrows);
		}
		OnUpdateArrows(base.Arrows);
	}

	protected override void SetButtons()
	{
		base.Buttons.Cleanup();
		foreach (WorkflowBase child in _children)
		{
			Buttons.Merge(base.Buttons, child.Buttons);
		}
		if (_request.CanCancel && base.Buttons.CancelData == null)
		{
			ButtonStyle.StyleType style = ((base.Buttons.WorkflowButtons.Count == 0) ? ButtonStyle.StyleType.Main : ButtonStyle.StyleType.Secondary);
			base.Buttons.CancelData = new PromptButtonData
			{
				ButtonText = Utils.GetCancelLocKey(_request.CancellationType),
				Style = style,
				ButtonCallback = delegate
				{
					_request.Cancel();
				},
				ButtonSFX = WwiseEvents.sfx_ui_cancel.EventName
			};
		}
		if (_request.AllowUndo && base.Buttons.UndoData == null && base.Buttons.DisplayUndo)
		{
			base.Buttons.UndoData = new PromptButtonData
			{
				ButtonCallback = delegate
				{
					_request.Undo();
				}
			};
		}
		OnUpdateButtons(base.Buttons);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("CastTimeOptionWorkflow");
		stringBuilder.AppendLine();
		stringBuilder.Append("-Children:");
		foreach (WorkflowBase child in _children)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("--");
			stringBuilder.Append(child.ToString());
		}
		return stringBuilder.ToString();
	}

	public override void CleanUp()
	{
		while (_children.Count > 0)
		{
			WorkflowBase workflowBase = _children[0];
			workflowBase.CleanUp();
			workflowBase.UpdateHighlights -= OnUpdateChildHighlights;
			workflowBase.UpdateDimming -= OnUpdateChildDimming;
			workflowBase.UpdateArrows -= OnUpdateChildArrows;
			workflowBase.UpdateButtons -= OnUpdateChildButtons;
			_children.RemoveAt(0);
		}
		base.CleanUp();
	}
}
