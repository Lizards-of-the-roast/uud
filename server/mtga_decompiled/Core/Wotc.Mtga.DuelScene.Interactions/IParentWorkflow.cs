using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions;

public interface IParentWorkflow
{
	IEnumerable<WorkflowBase> ChildWorkflows { get; }
}
