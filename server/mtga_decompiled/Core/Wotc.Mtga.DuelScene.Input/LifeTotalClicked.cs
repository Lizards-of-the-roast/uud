using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Input;

public class LifeTotalClicked : IEntityInputEvent<IAvatarView>
{
	private readonly IClickableWorkflowProvider _workflowProvider;

	public LifeTotalClicked(IClickableWorkflowProvider workflowProvider)
	{
		_workflowProvider = workflowProvider ?? new NullClickableWorkflowProvider();
	}

	public void Execute(IAvatarView avatar)
	{
		if (_workflowProvider.CanClick(avatar, SimpleInteractionType.Primary))
		{
			_workflowProvider.OnClick(avatar, SimpleInteractionType.Primary);
		}
	}
}
