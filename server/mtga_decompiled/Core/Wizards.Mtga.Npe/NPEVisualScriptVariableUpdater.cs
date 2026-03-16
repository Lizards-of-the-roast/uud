using System.Collections.Generic;
using Core.Code.Promises;
using Unity.VisualScripting;
using UnityEngine;
using Wizards.GeneralUtilities;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.PlayBlade;
using Wotc.Mtga.Events;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga.Npe;

[RequireComponent(typeof(StateMachine))]
public class NPEVisualScriptVariableUpdater : MonoBehaviour
{
	private StateMachine _stateMachine;

	private const string _playerGoldAmountVariableName = "Player Gold Amount";

	private const string _playerGemAmountVariableName = "Player Gems Amount";

	private const string _playerBoostPackCountVariableName = "Player Booster Pack Count";

	private const string _playerColorMasteryEventsCompletedName = "Finished Color Mastery Events";

	private const string _playBladeLasteSelectedFormatVariableName = "Last Selected Play Blade Format";

	private const string _playBladeLastSelectedFormatWasRankedVariableName = "Last Selected Play Blade Format Was Ranked";

	private const string _playerRankVariableName = "Player Current Rank";

	private const string _playerBoosterPackCountChangedEventName = "Booster Pack Count Changed";

	private IInventoryServiceWrapper _inventoryServiceWrapper;

	private IPlayerRankServiceWrapper _rankServiceWrapper;

	private IColorChallengeStrategy _colorChallengeStrategy;

	private void Awake()
	{
		_stateMachine = GetComponent<StateMachine>();
	}

	private void Start()
	{
		GameEvent.OnGameEvent += ObjectVisibilityEvent;
		_inventoryServiceWrapper = Pantry.Get<IInventoryServiceWrapper>();
		Variables.Application.Set("Player Gold Amount", _inventoryServiceWrapper.Inventory.gold);
		Variables.Application.Set("Player Gems Amount", _inventoryServiceWrapper.Inventory.gems);
		Variables.Application.Set("Player Booster Pack Count", _inventoryServiceWrapper.Inventory.BoosterPackCount);
		_inventoryServiceWrapper.GoldChanged += OnGoldChanged;
		_inventoryServiceWrapper.GemsChanged += OnGemsChanged;
		_inventoryServiceWrapper.BoostersChanged += OnBoostersChanged;
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		sceneLoader.SceneLoaded += OnPageChanged;
		sceneLoader.PlayBladeQueueSelected += OnPlayBladeQueueSelected;
		_rankServiceWrapper = Pantry.Get<IPlayerRankServiceWrapper>();
		Variables.Application.Set("Player Current Rank", _rankServiceWrapper.CombinedRank.constructed.rankClass);
		_colorChallengeStrategy = Pantry.Get<IColorChallengeStrategy>();
		Variables.Application.Set("Finished Color Mastery Events", _colorChallengeStrategy.CompletedGames);
		_colorChallengeStrategy.OnCompletedGamesChanged += OnColorChallengeGamesCompletedCountUpdated;
		_rankServiceWrapper.OnCombinedRankUpdated += OnCombinedRankUpdated;
	}

	private void OnDestroy()
	{
		GameEvent.OnGameEvent -= ObjectVisibilityEvent;
		if (_inventoryServiceWrapper != null)
		{
			_inventoryServiceWrapper.GoldChanged -= OnGoldChanged;
			_inventoryServiceWrapper.GemsChanged -= OnGemsChanged;
			_inventoryServiceWrapper.BoostersChanged -= OnBoostersChanged;
			_colorChallengeStrategy.OnCompletedGamesChanged -= OnColorChallengeGamesCompletedCountUpdated;
		}
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if (sceneLoader != null)
		{
			sceneLoader.SceneLoaded -= OnPageChanged;
			sceneLoader.PlayBladeQueueSelected -= OnPlayBladeQueueSelected;
		}
		if (_rankServiceWrapper != null)
		{
			_rankServiceWrapper.OnCombinedRankUpdated -= OnCombinedRankUpdated;
		}
	}

	private void OnColorChallengeGamesCompletedCountUpdated(int completedGamesCount)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			Variables.Application.Set("Finished Color Mastery Events", completedGamesCount);
		});
	}

	private void OnGoldChanged(int goldAmount)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			Variables.Application.Set("Player Gold Amount", goldAmount);
		});
	}

	private void OnGemsChanged(int gemsAmount)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			Variables.Application.Set("Player Gems Amount", gemsAmount);
		});
	}

	private void OnBoostersChanged(List<ClientBoosterInfo> boostersChanged)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			Variables.Application.Set("Player Booster Pack Count", ClientPlayerInventory.GetBoosterPackCount(boostersChanged));
			CustomEvent.Trigger(base.gameObject, "Booster Pack Count Changed");
		});
	}

	private void OnPageChanged()
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			EventBus.Trigger("PageChangedEvent", SceneLoader.GetSceneLoader().CurrentContentType);
		});
	}

	private void OnPlayBladeQueueSelected(BladeEventInfo bladeEventInfo)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			Variables.Application.Set("Last Selected Play Blade Format", GameFormatUtilities.FormatStringToGameFormat(bladeEventInfo.FormatName));
			Variables.Application.Set("Last Selected Play Blade Format Was Ranked", bladeEventInfo.IsRanked);
			EventBus.Trigger("PlayBladeSelectionEvent", bladeEventInfo);
		});
	}

	private void OnCombinedRankUpdated(CombinedRankInfo combinedRankUpdated)
	{
		MainThreadDispatcher.Instance.Add(delegate
		{
			Variables.Application.Set("Player Current Rank", combinedRankUpdated.constructed.rankClass);
		});
	}

	private void ObjectVisibilityEvent(GameEventDetails visibilityEvent)
	{
		EventBus.Trigger("ObjectEventVisibilityRaised", base.gameObject, visibilityEvent);
	}
}
