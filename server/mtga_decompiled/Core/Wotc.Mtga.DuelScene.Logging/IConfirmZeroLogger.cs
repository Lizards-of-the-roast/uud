namespace Wotc.Mtga.DuelScene.Logging;

public interface IConfirmZeroLogger
{
	void ConfirmZeroDisplayed(uint sourceId);

	void ConfirmZeroSelected();

	void BackSelected();

	void UndoSelected();

	void WorkflowCleanup();
}
