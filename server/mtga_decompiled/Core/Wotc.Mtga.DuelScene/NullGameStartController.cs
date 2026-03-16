using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene;

public class NullGameStartController : IGameStartController
{
	public static readonly IGameStartController Default = new NullGameStartController();

	public void UpdateCurrentWorkflow(WorkflowBase workflow)
	{
	}
}
