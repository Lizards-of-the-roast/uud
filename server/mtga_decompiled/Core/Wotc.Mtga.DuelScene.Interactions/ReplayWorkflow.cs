using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Interactions;

public class ReplayWorkflow : WorkflowBase<BaseUserRequest>
{
	public ReplayWorkflow(BaseUserRequest req)
		: base(req)
	{
	}

	protected override void ApplyInteractionInternal()
	{
	}
}
