using System;
using System.Collections.Generic;
using WorkflowVisuals;

namespace Wotc.Mtga.DuelScene.Interactions.AssignDamage;

public class AssignDamageHighlightsGenerator : IHighlightsGenerator, IDisposable
{
	private readonly Highlights _highlights = new Highlights();

	private readonly ICardViewProvider _cardProvider;

	private IReadOnlyList<MtgDamageAssignment> _damageAssignments = Array.Empty<MtgDamageAssignment>();

	private bool _coldManaHighlightOverride;

	public AssignDamageHighlightsGenerator(ICardViewProvider cardProvider)
	{
		_cardProvider = cardProvider ?? NullCardViewProvider.Default;
	}

	public void SetDamageAssignments(IReadOnlyList<MtgDamageAssignment> damageAssignments)
	{
		_damageAssignments = damageAssignments ?? Array.Empty<MtgDamageAssignment>();
	}

	public void SetColdManaHighlightOverride(bool coldManaHighlightOverride)
	{
		_coldManaHighlightOverride = coldManaHighlightOverride;
	}

	public Highlights GetHighlights()
	{
		_highlights.Clear();
		foreach (MtgDamageAssignment damageAssignment in _damageAssignments)
		{
			if (_cardProvider.TryGetCardView(damageAssignment.InstanceId, out var _))
			{
				_highlights.IdToHighlightType_Workflow[damageAssignment.InstanceId] = GetHighlightForAssignment(damageAssignment, _coldManaHighlightOverride);
			}
		}
		return _highlights;
	}

	private static HighlightType GetHighlightForAssignment(MtgDamageAssignment assignment, bool coldManaHighlightOverride = false)
	{
		if (assignment.AssignedDamage == 0)
		{
			return HighlightType.None;
		}
		if (coldManaHighlightOverride)
		{
			return HighlightType.ColdMana;
		}
		if (assignment.AssignedDamage >= assignment.LethalDamage)
		{
			return HighlightType.Selected;
		}
		return HighlightType.Hot;
	}

	public void Dispose()
	{
		_damageAssignments = Array.Empty<MtgDamageAssignment>();
	}
}
