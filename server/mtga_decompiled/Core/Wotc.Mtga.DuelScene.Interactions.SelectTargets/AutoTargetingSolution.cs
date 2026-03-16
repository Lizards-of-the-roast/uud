using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectTargets;

public class AutoTargetingSolution : IAutoTargetingSolution
{
	private readonly IGameplaySettingsProvider _gameplaySettingsProvider;

	private readonly IAbilityDataProvider _abilityDataProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IAutoTargetingSolution _singleTarget = new StackTargetTargetingSolution();

	private readonly IAutoTargetingSolution _triggeredAbility = new TriggeredAbilityTargetingSolution();

	private readonly IAutoTargetingSolution _null = new NullAutoTargetingSolution();

	public AutoTargetingSolution(IContext context)
		: this(context.Get<IGameplaySettingsProvider>(), context.Get<IAbilityDataProvider>(), context.Get<IGameStateProvider>())
	{
	}

	public AutoTargetingSolution(IGameplaySettingsProvider gameplaySettingsProvider, IAbilityDataProvider abilityDataProvider, IGameStateProvider gameStateProvider)
	{
		_gameplaySettingsProvider = gameplaySettingsProvider ?? NullGameplaySettingsProvider.Default;
		_abilityDataProvider = abilityDataProvider ?? NullAbilityDataProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public bool TryAutoTarget(TargetSelection targetSelection, TargetSource targetSource, out Target target)
	{
		if (_gameplaySettingsProvider.GameplaySettings.FullControlEnabled)
		{
			return _null.TryAutoTarget(targetSelection, targetSource, out target);
		}
		if (UseTriggeredAbilitySolution(targetSource.Instance))
		{
			return _triggeredAbility.TryAutoTarget(targetSelection, targetSource, out target);
		}
		if (UseStackTargetSolution(targetSource.Instance, targetSelection.Targets, _gameStateProvider.LatestGameState))
		{
			return _singleTarget.TryAutoTarget(targetSelection, targetSource, out target);
		}
		return _null.TryAutoTarget(targetSelection, targetSource, out target);
	}

	private bool UseTriggeredAbilitySolution(MtgCardInstance sourceInstance)
	{
		if (SourceIsTriggeredAbility(sourceInstance, out var ability))
		{
			return !ability.IsModalAbility();
		}
		return false;
	}

	private bool SourceIsTriggeredAbility(MtgCardInstance sourceInstance, out AbilityPrintingData ability)
	{
		if (SourceIsAbility(sourceInstance, out ability))
		{
			return ability.Category == AbilityCategory.Triggered;
		}
		return false;
	}

	private bool SourceIsAbility(MtgCardInstance sourceInstance, out AbilityPrintingData abilityData)
	{
		abilityData = null;
		if (sourceInstance != null && sourceInstance.ObjectType == GameObjectType.Ability)
		{
			if (sourceInstance.Abilities.Count > 0)
			{
				abilityData = sourceInstance.Abilities[0];
			}
			else
			{
				abilityData = _abilityDataProvider.GetAbilityPrintingById(sourceInstance.GrpId);
			}
			return true;
		}
		return false;
	}

	private bool UseStackTargetSolution(MtgCardInstance sourceInstance, IReadOnlyCollection<Target> availableTargets, MtgGameState gameState)
	{
		if (!SourceIsCard(sourceInstance))
		{
			return false;
		}
		return TargetsExistAndAllTargetsOnStack(availableTargets, gameState);
	}

	private bool SourceIsCard(MtgCardInstance sourceInstance)
	{
		if (sourceInstance != null)
		{
			return sourceInstance.ObjectType != GameObjectType.Ability;
		}
		return false;
	}

	private bool TargetsExistAndAllTargetsOnStack(IReadOnlyCollection<Target> availableTargets, MtgGameState latestGameState)
	{
		foreach (Target availableTarget in availableTargets)
		{
			if (!latestGameState.TryGetCard(availableTarget.TargetInstanceId, out var card))
			{
				return false;
			}
			if (card.Zone == null || card.Zone.Type != ZoneType.Stack)
			{
				return false;
			}
		}
		return availableTargets.Count > 0;
	}
}
