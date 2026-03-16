using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using GreClient.Rules;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Providers;
using Wotc.Mtga.Unity;

namespace Wotc.Mtga.DuelScene.Emotes;

public class EmoteManager : IEmoteManager, IEmoteControllerProvider, IEntityDialogControllerProvider, IDisposable
{
	private readonly MutableEmoteControllerProvider _emoteProvider = new MutableEmoteControllerProvider();

	private readonly MutableEntityDialogControllerProvider _dialogueProvider = new MutableEntityDialogControllerProvider();

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly DuelSceneLogger _duelSceneLogger;

	private readonly IAvatarViewProvider _avatarProvider;

	private readonly IEmoteDataProvider _emoteDataProvider;

	private readonly IReadOnlyList<EmoteData> _equippedEmotes;

	private readonly UXEventQueue _eventQueue;

	private readonly UIMessageHandler _uiMessageHandler;

	private readonly Transform _emoteRoot;

	private readonly UIManager _uiManager;

	private FullControlToggle _fullControlToggle;

	public EmoteManager(AssetLookupSystem assetLookupSystem, DuelSceneLogger duelSceneLogger, IAvatarViewProvider avatarProvider, IEmoteDataProvider emoteDataProvider, IReadOnlyList<EmoteData> equippedEmotes, UXEventQueue eventQueue, UIMessageHandler uiMessageHandler, Transform emoteRoot, UIManager uiManager)
	{
		_assetLookupSystem = assetLookupSystem;
		_duelSceneLogger = duelSceneLogger;
		_avatarProvider = avatarProvider ?? NullAvatarViewProvider.Default;
		_emoteDataProvider = emoteDataProvider;
		_equippedEmotes = equippedEmotes ?? Array.Empty<EmoteData>();
		_eventQueue = eventQueue;
		_uiMessageHandler = uiMessageHandler;
		_emoteRoot = emoteRoot;
		_uiManager = uiManager;
	}

	public void MuteEmotes(bool isMuted)
	{
		foreach (EntityDialogController allDialogController in _dialogueProvider.GetAllDialogControllers())
		{
			allDialogController.UpdateIsMuted(isMuted);
		}
	}

	public IEmoteController GetEmoteControllerByPlayerType(GREPlayerNum playerType)
	{
		return _emoteProvider.GetEmoteControllerByPlayerType(playerType);
	}

	public IEmoteController GetEmoteControllerById(uint id)
	{
		return _emoteProvider.GetEmoteControllerById(id);
	}

	public IEnumerable<IEmoteController> GetAllEmoteControllers()
	{
		return _emoteProvider.GetAllEmoteControllers();
	}

	public EntityDialogController GetDialogControllerByPlayerType(GREPlayerNum playerType)
	{
		return _dialogueProvider.GetDialogControllerByPlayerType(playerType);
	}

	public EntityDialogController GetDialogControllerById(uint id)
	{
		return _dialogueProvider.GetDialogControllerById(id);
	}

	public IEnumerable<EntityDialogController> GetAllDialogControllers()
	{
		return _dialogueProvider.GetAllDialogControllers();
	}

	public void CreateEmotesForPlayer(MtgPlayer player)
	{
		if (player != null)
		{
			if (player.IsLocalPlayer)
			{
				SetUpLocalPlayerEmotePrefab(player);
			}
			else
			{
				SetUpOpponentEmotePrefab(player);
			}
		}
	}

	private void SetUpLocalPlayerEmotePrefab(MtgPlayer player)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.GREPlayerNum = player.ClientPlayerEnum;
		_assetLookupSystem.Blackboard.InDuelScene = true;
		EmoteViewPresenter emoteViewPresenter = AssetLoader.Instantiate<EmoteViewPresenter>(_assetLookupSystem.TreeLoader.LoadTree<EmoteViewPresenterPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard).PrefabPath, _emoteRoot);
		EmoteOptionsView emoteOptionsView = AssetLoader.Instantiate<EmoteOptionsView>(_assetLookupSystem.TreeLoader.LoadTree<EmoteOptionsViewPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard).PrefabPath, _emoteRoot);
		EmoteOptionsController emoteOptionsController = new EmoteOptionsController(_equippedEmotes, _assetLookupSystem, emoteOptionsView);
		LocalPlayerDialogController controller = new LocalPlayerDialogController(_emoteDataProvider, _uiMessageHandler, emoteViewPresenter, _assetLookupSystem, emoteOptionsController, _eventQueue, _duelSceneLogger);
		_dialogueProvider.Add(player, controller);
		_emoteProvider.Add(player, emoteOptionsController);
		GameObject gameObject = emoteOptionsView.gameObject;
		SelectionGroup selectionGroup = gameObject.AddComponent<SelectionGroup>();
		if (PlatformUtils.IsHandheld())
		{
			selectionGroup.AddSelectable(_avatarProvider.GetAvatarById(player.InstanceId).gameObject);
			selectionGroup.AddSelectable(gameObject);
			if (_uiManager.FullControl is FullControlToggle fullControlToggle)
			{
				_fullControlToggle = fullControlToggle;
				selectionGroup.AddSelectable(fullControlToggle.gameObject);
			}
			foreach (PhaseLadderButton phaseIcon in _uiManager.PhaseLadder.PhaseIcons)
			{
				selectionGroup.AddSelectable(phaseIcon.gameObject);
			}
			foreach (AvatarPhaseIcon item in _avatarProvider.GetAllAvatars().SelectMany((DuelScene_AvatarView avatar) => avatar.PhaseIcons))
			{
				selectionGroup.AddSelectable(item.gameObject);
			}
			selectionGroup.Deselected += HACK_LocalplayerDeselected_Handheld;
		}
		else
		{
			selectionGroup.AddSelectable(_avatarProvider.GetAvatarById(player.InstanceId).gameObject);
			selectionGroup.AddSelectable(gameObject);
			selectionGroup.Deselected += HACK_LocalplayerDeselected_Desktop;
		}
	}

	private void HACK_LocalplayerDeselected_Desktop()
	{
		if (((IEmoteControllerProvider)_emoteProvider).TryGetEmoteControllerByPlayerType(GREPlayerNum.LocalPlayer, out IEmoteController dialogController))
		{
			dialogController.Close();
		}
	}

	private void HACK_LocalplayerDeselected_Handheld()
	{
		if (((IEmoteControllerProvider)_emoteProvider).TryGetEmoteControllerByPlayerType(GREPlayerNum.LocalPlayer, out IEmoteController dialogController))
		{
			dialogController.Close();
		}
		if (!PlatformUtils.IsAspectRatio4x3())
		{
			_avatarProvider.GetAvatarByPlayerSide(GREPlayerNum.LocalPlayer).ShowPlayerNames(enabled: false);
		}
		if (!(_fullControlToggle == null))
		{
			_fullControlToggle.HideToggle();
		}
	}

	private void SetUpOpponentEmotePrefab(MtgPlayer player)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.GREPlayerNum = player.ClientPlayerEnum;
		_assetLookupSystem.Blackboard.InDuelScene = true;
		EmoteViewPresenter emoteViewPresenter = AssetLoader.Instantiate<EmoteViewPresenter>(_assetLookupSystem.TreeLoader.LoadTree<EmoteViewPresenterPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard).PrefabPath, _emoteRoot);
		CommunicationOptionsView communicationOptionsView = AssetLoader.Instantiate<CommunicationOptionsView>(_assetLookupSystem.TreeLoader.LoadTree<CommunicationOptionsViewPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard).PrefabPath, _emoteRoot);
		CommunicationOptionsController communicationOptionsController = new CommunicationOptionsController(_assetLookupSystem, communicationOptionsView);
		OpponentDialogController opponentDialogController = new OpponentDialogController(_emoteDataProvider, _uiMessageHandler, _assetLookupSystem, communicationOptionsController, emoteViewPresenter, _duelSceneLogger);
		opponentDialogController.IsMutedUpdated += delegate(bool isMuted)
		{
			foreach (EntityDialogController allDialogController in _dialogueProvider.GetAllDialogControllers())
			{
				allDialogController.UpdateIsMuted(isMuted);
			}
		};
		opponentDialogController.UpdateIsMuted(MDNPlayerPrefs.DisableEmotes);
		_dialogueProvider.Add(player, opponentDialogController);
		_emoteProvider.Add(player, communicationOptionsController);
		GameObject gameObject = communicationOptionsView.gameObject;
		SelectionGroup selectionGroup = gameObject.AddComponent<SelectionGroup>();
		if (PlatformUtils.IsHandheld() && !PlatformUtils.IsAspectRatio4x3())
		{
			selectionGroup.AddSelectable(_avatarProvider.GetAvatarByPlayerSide(GREPlayerNum.Opponent).gameObject);
			selectionGroup.AddSelectable(gameObject);
			selectionGroup.Deselected += HACK_OpponentDeselected_Handheld;
		}
		else
		{
			selectionGroup.AddSelectable(_avatarProvider.GetAvatarByPlayerSide(GREPlayerNum.Opponent).gameObject);
			selectionGroup.AddSelectable(gameObject);
			selectionGroup.Deselected += HACK_OpponentDeselected_Desktop;
		}
	}

	private void HACK_OpponentDeselected_Desktop()
	{
		if (((IEmoteControllerProvider)_emoteProvider).TryGetEmoteControllerByPlayerType(GREPlayerNum.Opponent, out IEmoteController dialogController))
		{
			dialogController.Close();
		}
	}

	private void HACK_OpponentDeselected_Handheld()
	{
		if (((IEmoteControllerProvider)_emoteProvider).TryGetEmoteControllerByPlayerType(GREPlayerNum.Opponent, out IEmoteController dialogController))
		{
			dialogController.Close();
		}
		_avatarProvider.GetAvatarByPlayerSide(GREPlayerNum.Opponent).ShowPlayerNames(enabled: false);
	}

	public void Dispose()
	{
		_emoteProvider.Dispose();
		_dialogueProvider.Dispose();
	}

	public static IReadOnlyList<EmoteData> GetEquippedEmotes(IEmoteDataProvider emoteDataProvider, MatchManager matchManager, CosmeticsProvider cosmeticsProvider)
	{
		return new List<EmoteData>(emoteDataProvider.GetEmoteData(GetEquippedEmotes(matchManager, cosmeticsProvider)));
	}

	private static IReadOnlyList<string> GetEquippedEmotes(MatchManager matchManager, CosmeticsProvider cosmeticsProvider)
	{
		if (matchManager != null && matchManager.LocalPlayerInfo?.EmoteSelection?.Count > 0)
		{
			return matchManager.LocalPlayerInfo.EmoteSelection;
		}
		if (cosmeticsProvider?.PlayerEmoteSelections != null)
		{
			return cosmeticsProvider.PlayerEmoteSelections;
		}
		return Array.Empty<string>();
	}
}
