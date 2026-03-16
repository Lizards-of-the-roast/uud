using System;
using System.Collections.Generic;
using Wotc.Mtga.DuelScene.Interactions;

namespace WorkflowVisuals;

public class HighlightGeneratorAggregate : IHighlightsGenerator
{
	private readonly IReadOnlyCollection<WorkflowBase> _childWorkflows;

	public HighlightGeneratorAggregate(IReadOnlyCollection<WorkflowBase> childWorkflows)
	{
		_childWorkflows = (IReadOnlyCollection<WorkflowBase>)(((object)childWorkflows) ?? ((object)Array.Empty<WorkflowBase>()));
	}

	public Highlights GetHighlights()
	{
		Highlights highlights = new Highlights();
		foreach (WorkflowBase childWorkflow in _childWorkflows)
		{
			highlights.Merge(childWorkflow.Highlights);
		}
		return highlights;
	}
}
