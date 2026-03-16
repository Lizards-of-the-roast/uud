namespace Wotc.Mtga.DuelScene.Interactions;

public class NullClickableWorkflowProvider : IClickableWorkflowProvider
{
	public static readonly IClickableWorkflowProvider Default = new NullClickableWorkflowProvider();

	public bool CanClick(IEntityView entity, SimpleInteractionType clickType)
	{
		return false;
	}

	public void OnClick(IEntityView entity, SimpleInteractionType clickType)
	{
	}
}
