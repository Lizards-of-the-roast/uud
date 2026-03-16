namespace Wotc.Mtga.DuelScene.Interactions;

public interface IClickableWorkflow
{
	bool CanClick(IEntityView entity, SimpleInteractionType clickType);

	void OnClick(IEntityView entity, SimpleInteractionType clickType);

	bool CanClickStack(CdcStackCounterView entity, SimpleInteractionType clickType);

	void OnClickStack(CdcStackCounterView entity);

	void OnBattlefieldClick();
}
