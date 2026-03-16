using System.Collections.Generic;
using System.Linq;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class IgnorableInactiveActionsCalculator : IAutoSubmitActionCalculator
{
	private readonly IObjectPool _objectPool;

	private readonly IGameplaySettingsProvider _gameplaySettings;

	private readonly IAbilityDataProvider _abilityDataProvider;

	public IgnorableInactiveActionsCalculator(IObjectPool objectPool, IGameplaySettingsProvider gameplaySettings, IAbilityDataProvider abilityDataProvider)
	{
		_objectPool = objectPool ?? NullObjectPool.Default;
		_gameplaySettings = gameplaySettings ?? NullGameplaySettingsProvider.Default;
		_abilityDataProvider = abilityDataProvider ?? NullAbilityDataProvider.Default;
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
		List<GreInteraction> list3 = _objectPool.PopObject<List<GreInteraction>>();
		foreach (GreInteraction action in actions)
		{
			if (action.IsActive)
			{
				list3.Add(action);
			}
			else if (IsIgnorableAction(action.GreAction) && !action.IsActive)
			{
				list2.Add(action);
			}
			else
			{
				list.Add(action);
			}
		}
		if (list3.Count == 1 && list.Count == 0 && list2.Count > 0)
		{
			result = list3[0];
		}
		list.Clear();
		_objectPool.PushObject(list, tryClear: false);
		list2.Clear();
		_objectPool.PushObject(list2, tryClear: false);
		list3.Clear();
		_objectPool.PushObject(list3, tryClear: false);
		return result;
	}

	private bool IsIgnorableAction(Action action)
	{
		if (_abilityDataProvider.TryGetAbilityPrintingById(action.AbilityGrpId, out var ability))
		{
			return ability.Tags.Contains(MetaDataTag.IgnoreInactiveActionInBrowser);
		}
		return false;
	}
}
