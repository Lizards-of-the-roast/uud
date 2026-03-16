using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Logging;

public class ConfirmZeroLogger : IConfirmZeroLogger
{
	private readonly IBILoggerAdapter _internalLoggerAdapter;

	private readonly IGameStateProvider _gameStateProvider;

	private MtgCardInstance _source;

	private bool _isActive;

	public ConfirmZeroLogger(IBILoggerAdapter internalLoggerAdapter, IGameStateProvider gameStateProvider)
	{
		_internalLoggerAdapter = internalLoggerAdapter;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public void ConfirmZeroDisplayed(uint sourceId)
	{
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		_source = mtgGameState.GetCardById(sourceId);
		_isActive = true;
	}

	public void ConfirmZeroSelected()
	{
		LogActionTaken("ConfirmZero");
	}

	public void BackSelected()
	{
		LogActionTaken("Back");
	}

	public void UndoSelected()
	{
		LogActionTaken("Undo");
	}

	public void WorkflowCleanup()
	{
		if (_isActive)
		{
			LogActionTaken("NULL");
		}
	}

	private void LogActionTaken(string actionTaken)
	{
		_internalLoggerAdapter.Log(ToPayload(_source, actionTaken));
		_source = null;
		_isActive = false;
	}

	private static (string, string)[] ToPayload(MtgCardInstance sourceInstance, string actionTaken)
	{
		if (sourceInstance != null && sourceInstance.GrpId != 0)
		{
			string item = ((sourceInstance.ObjectType == GameObjectType.Ability) ? "Ability GrpId" : "Card GrpId");
			return new(string, string)[2]
			{
				(item, sourceInstance.GrpId.ToString()),
				("Action Taken", actionTaken)
			};
		}
		return new(string, string)[1] { ("Action Taken", actionTaken) };
	}
}
