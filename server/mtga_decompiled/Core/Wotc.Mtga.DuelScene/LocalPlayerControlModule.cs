using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene;

public class LocalPlayerControlModule : DebugModule
{
	private readonly string _name;

	private readonly string _description;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly WorkflowController _workflowController;

	private readonly ImGUIRequestView _requestRenderer;

	public override string Name => _name;

	public override string Description => _description;

	public LocalPlayerControlModule(string name, string description, IGameStateProvider gameStateProvider, WorkflowController workflowController, ImGUIRequestView requestRenderer)
	{
		_name = name ?? "Local Player Controls";
		_description = description ?? string.Empty;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_workflowController = workflowController;
		_requestRenderer = requestRenderer ?? new ImGUIRequestView(NullCardDatabaseAdapter.Default);
	}

	public override void Render()
	{
		RenderWorkflowDetails();
		GUILayout.Space(3f);
		_requestRenderer.Render(_workflowController?.CurrentWorkflow?.BaseRequest, _gameStateProvider.CurrentGameState);
	}

	private void RenderWorkflowDetails()
	{
		FontStyle fontStyle = GUI.skin.label.fontStyle;
		GUILayout.BeginHorizontal();
		(string, string) workflowLabels = GetWorkflowLabels(_workflowController);
		GUIContent content = new GUIContent(workflowLabels.Item1);
		GUI.skin.label.fontStyle = FontStyle.Bold;
		GUILayout.Label(content, GUILayout.Width(GUI.skin.label.CalcSize(content).x));
		GUI.skin.label.fontStyle = FontStyle.Normal;
		GUILayout.Label(workflowLabels.Item2);
		GUILayout.EndHorizontal();
		GUI.skin.label.fontStyle = fontStyle;
	}

	private static (string, string) GetWorkflowLabels(WorkflowController workflowController)
	{
		if (workflowController.CurrentWorkflow == null)
		{
			if (workflowController.PendingWorkflow == null)
			{
				return ("Current Workflow: ", "NONE");
			}
			return ("Pending Workflow: ", workflowController.PendingWorkflow.ToString());
		}
		return ("Current Workflow: ", workflowController.CurrentWorkflow.ToString());
	}
}
