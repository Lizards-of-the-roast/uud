namespace Wotc.Mtga.DuelScene.Interactions;

public interface IDraggableWorkflow
{
	bool CanCommenceDrag(IEntityView beginningEntityView);

	void OnDragCommenced(IEntityView beginningEntityView);

	bool CanCompleteDrag(IEntityView endingEntityView);

	void OnDragCompleted(IEntityView beginningEntityView, IEntityView endingEntityView);
}
