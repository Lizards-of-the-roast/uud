using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene;

public interface IGameStartController
{
	void UpdateCurrentWorkflow(WorkflowBase workflow);
}
