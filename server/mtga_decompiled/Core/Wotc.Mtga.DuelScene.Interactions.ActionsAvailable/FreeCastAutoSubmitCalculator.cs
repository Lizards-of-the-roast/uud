using System.Collections.Generic;
using Pooling;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class FreeCastAutoSubmitCalculator : IAutoSubmitActionCalculator
{
	private readonly IObjectPool _objectPool;

	private readonly IGameplaySettingsProvider _gameplaySettings;

	public FreeCastAutoSubmitCalculator(IObjectPool objectPool, IGameplaySettingsProvider gameplaySettings)
	{
		_objectPool = objectPool ?? NullObjectPool.Default;
		_gameplaySettings = gameplaySettings ?? NullGameplaySettingsProvider.Default;
	}

	public GreInteraction GetAutoSubmitAction(IEnumerable<GreInteraction> actions)
	{
		if (_gameplaySettings.FullControlEnabled)
		{
			return null;
		}
		GreInteraction result = null;
		List<GreInteraction> list = _objectPool.PopObject<List<GreInteraction>>();
		List<GreInteraction> list2 = _objectPool.PopObject<List<GreInteraction>>();
		foreach (GreInteraction action in actions)
		{
			(action.GreAction.IsFreeCast() ? list : list2).Add(action);
		}
		if (list.Count == 1 && list2.Count == 0)
		{
			result = list[0];
		}
		list.Clear();
		_objectPool.PushObject(list, tryClear: false);
		list2.Clear();
		_objectPool.PushObject(list2, tryClear: false);
		return result;
	}
}
