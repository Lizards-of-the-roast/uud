namespace Wotc.Mtga.DuelScene.Interactions;

public interface IClickableWorkflowProvider
{
	bool CanClick(IEntityView entity, SimpleInteractionType clickType);

	void OnClick(IEntityView entity, SimpleInteractionType clickType);
}
