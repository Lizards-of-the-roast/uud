using System.Collections.Generic;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene;

public abstract class DebugModule
{
	public abstract string Name { get; }

	public abstract string Description { get; }

	public abstract void Render();

	public static IEnumerable<DebugModule> BasicModules(GameManager gameManager, IGameStateProvider gameStateProvider, MatchManager matchManager)
	{
		yield return new ConnectionModule(new ConnectionModule.DebugConnection(gameManager.GreConnection, matchManager.ConnectionConfig));
		yield return new TimerDebugModule(gameStateProvider);
		yield return new UXEventDebuggerModule(gameManager.UXEventQueue);
		yield return new ReplayExportModule(matchManager.MessageHistory);
		yield return new CardUtilityModule();
		yield return new GenericInfoModule();
		yield return PlayerControlDebugModule(gameStateProvider, gameManager.WorkflowController, gameManager.MatchManager, gameManager.CardDatabase);
		yield return new DebugSettingsModule(gameManager.SplineMovementSystem);
		yield return new LanguagesModule();
	}

	private static DebugModule PlayerControlDebugModule(IGameStateProvider gameStateProvider, WorkflowController workflowController, MatchManager matchManager, ICardDatabaseAdapter cdb)
	{
		if (matchManager.Familiars.Count > 0)
		{
			List<DebugModule> list = new List<DebugModule>();
			list.Add(new LocalPlayerControlModule("LocalPlayer", "Controls for LocalPlayer", gameStateProvider, workflowController, new ImGUIRequestView(cdb)));
			int num = 1;
			foreach (HeadlessClient familiar in matchManager.Familiars)
			{
				string text = ((matchManager.Familiars.Count == 1) ? "Familiar" : $"Familiar_{num++}");
				string description = ((matchManager.Familiars.Count == 1) ? "Control your AI Opponent" : ("Control your AI Opponent ($" + text + ")"));
				list.Add(new FamiliarControlModule(text, description, new MatchRenderer(familiar, new ImGUIMatchView(familiar, cdb))));
			}
			return new AggregateTabModule("Player Controls", "Used for granular control of any player (or ai) in this match", list);
		}
		return new LocalPlayerControlModule("Player Controls", "Used for granular control of the local player in this match", gameStateProvider, workflowController, new ImGUIRequestView(cdb));
	}
}
