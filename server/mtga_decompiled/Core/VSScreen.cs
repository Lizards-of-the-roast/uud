using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Event;
using AssetLookupTree.Payloads.UI.NPE;
using Core.Code.Input;
using Core.MatchScene.PreGame;
using Core.Meta.NewPlayerExperience.Graph;
using Core.Meta.Shared;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using MTGA.KeyboardManager;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

public class VSScreen : MonoBehaviour
{
	[Serializable]
	public struct PlayerBrawlElements
	{
		public TextMeshProUGUI[] CommanderFields;

		public GameObject DeckBox;

		public TextMeshProUGUI DeckBoxColors;

		public GameObject GoldManaSymbol;

		[NonSerialized]
		public MeshRendererReferenceLoader[] DeckMeshLoaders;
	}

	[Serializable]
	private struct PlayerElements
	{
		public Image Avatar;

		public TextMeshProUGUI PlayerName;

		public Localize PlayerTitle;

		public Localize PlayerDetails;

		public RankDisplay RankDisplayObject;

		public Sprite WinSprite;

		public Image[] WinPips;
	}

	[Serializable]
	private class VSScreenAnimator
	{
		private static readonly int ANIM_TRIGGER_READY = Animator.StringToHash("Ready");

		[SerializeField]
		private Animator _animator;

		[Header("Animator Controllers")]
		[SerializeField]
		private RuntimeAnimatorController _match;

		[SerializeField]
		private RuntimeAnimatorController _game;

		[SerializeField]
		private RuntimeAnimatorController _privateMatch;

		[SerializeField]
		private RuntimeAnimatorController _npe;

		[SerializeField]
		private RuntimeAnimatorController _npe_game_0;

		public event System.Action Completed;

		public void Init(NPEState npeState, bool continuedMatch, bool privateGame)
		{
			_animator.runtimeAnimatorController = getAnimator();
			AnimationComplete_SMB behaviour = _animator.GetBehaviour<AnimationComplete_SMB>();
			behaviour.OnAnimationComplete = (System.Action)Delegate.Combine(behaviour.OnAnimationComplete, new System.Action(animationComplete));
			void animationComplete()
			{
				AnimationComplete_SMB behaviour2 = _animator.GetBehaviour<AnimationComplete_SMB>();
				behaviour2.OnAnimationComplete = (System.Action)Delegate.Remove(behaviour2.OnAnimationComplete, new System.Action(animationComplete));
				this.Completed?.Invoke();
			}
			RuntimeAnimatorController getAnimator()
			{
				if (npeState != null && npeState.ActiveNPEGame != null)
				{
					if (npeState.ActiveNPEGameNumber == 0)
					{
						return _npe_game_0;
					}
					return _npe;
				}
				if (continuedMatch)
				{
					return _game;
				}
				if (privateGame)
				{
					return _privateMatch;
				}
				return _match;
			}
		}

		public void Ready()
		{
			_animator.SetTrigger(ANIM_TRIGGER_READY);
		}

		public void OnDestroy()
		{
			this.Completed = null;
		}
	}

	[HideInInspector]
	public PreGameScene _preGameScene;

	[SerializeField]
	private VSScreenAnimator _animator;

	[Space(10f)]
	[SerializeField]
	private Image _matchmakingBackgroundImage;

	[SerializeField]
	private UnityEngine.Color _defaultLetterBoxColor = UnityEngine.Color.black;

	[SerializeField]
	private Image _letterboxTop;

	[SerializeField]
	private Image _letterboxBottom;

	[Space(5f)]
	[SerializeField]
	private PlayerElements _localPlayerElements;

	[SerializeField]
	private PlayerElements _opponentElements;

	[SerializeField]
	private PlayerBrawlElements _localPlayerBrawl;

	[SerializeField]
	private PlayerBrawlElements _opponentBrawl;

	[Space(5f)]
	[SerializeField]
	private WaitingForMatchView _waitingForMatchView;

	[SerializeField]
	private CyclingTipsView _cyclingTips;

	private CharacterLibrary _npeCharacterData;

	[SerializeField]
	private UnityEngine.Color _mythicOrangeColor = UnityEngine.Color.white;

	[Space(5f)]
	[SerializeField]
	private bool _minimalLoader;

	[SerializeField]
	private bool _skipReadyScreen;

	private readonly AssetLoader.AssetTracker<Sprite> _matchmakingBackgroundSpriteTracker = new AssetLoader.AssetTracker<Sprite>("VSScreenBackgroundSpriteTracker");

	private readonly AssetLoader.AssetTracker<Sprite> _localAvatarSpriteTracker = new AssetLoader.AssetTracker<Sprite>("VSScreenLocalAvatarSpriteTracker");

	private readonly AssetLoader.AssetTracker<Sprite> _opponentAvatarSpriteTracker = new AssetLoader.AssetTracker<Sprite>("VSScreenOpponentSpriteTracker");

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private AssetLookupSystem _assetLookupSystem;

	private ICardDatabaseAdapter _cardDatabase;

	private CardMaterialBuilder _cardMaterialBuilder;

	private MatchManager _matchManager;

	private NPEState _npeState;

	private CosmeticsProvider _cosmeticsProvider;

	private CharacterLibrary NPECharacterData
	{
		get
		{
			if (_npeCharacterData == null)
			{
				_assetLookupSystem.Blackboard.Clear();
				NPECharacterLibraryPayload payload = _assetLookupSystem.TreeLoader.LoadTree<NPECharacterLibraryPayload>().GetPayload(_assetLookupSystem.Blackboard);
				_npeCharacterData = AssetLoader.AcquireAndTrackAsset<CharacterLibrary>(base.gameObject, "CharacterData", payload.CharacterLibraryRef.RelativePath);
			}
			return _npeCharacterData;
		}
	}

	public void Init(PreGameScene preGameScene, KeyboardManager keyboardManager, IActionSystem actionSystem, AssetLookupSystem assetLookupSystem, ICardDatabaseAdapter cardDatabase, CardMaterialBuilder cardMaterialBuilder, MatchManager matchManager, NPEState npeState, CosmeticsProvider cosmeticsProvider)
	{
		_preGameScene = preGameScene;
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_assetLookupSystem = assetLookupSystem;
		_cardDatabase = cardDatabase;
		_cardMaterialBuilder = cardMaterialBuilder;
		_matchManager = matchManager;
		_npeState = npeState;
		_cosmeticsProvider = cosmeticsProvider;
		SetupScreen();
	}

	private void SetupScreen()
	{
		if (_matchManager == null || _preGameScene == null)
		{
			return;
		}
		_preGameScene.GameFound += OnGameFound;
		if (_localPlayerBrawl.DeckBox != null)
		{
			MeshRenderer[] componentsInChildren = _localPlayerBrawl.DeckBox.GetComponentsInChildren<MeshRenderer>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				_localPlayerBrawl.DeckMeshLoaders = new MeshRendererReferenceLoader[componentsInChildren.Length];
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					_localPlayerBrawl.DeckMeshLoaders[i] = new MeshRendererReferenceLoader(componentsInChildren[i]);
				}
			}
		}
		if (_opponentBrawl.DeckBox != null)
		{
			MeshRenderer[] componentsInChildren2 = _opponentBrawl.DeckBox.GetComponentsInChildren<MeshRenderer>();
			if (componentsInChildren2 != null && componentsInChildren2.Length != 0)
			{
				_opponentBrawl.DeckMeshLoaders = new MeshRendererReferenceLoader[componentsInChildren2.Length];
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					_opponentBrawl.DeckMeshLoaders[j] = new MeshRendererReferenceLoader(componentsInChildren2[j]);
				}
			}
		}
		if (_npeState.ActiveNPEGame != null)
		{
			_minimalLoader = true;
			if (_npeState.ActiveNPEGameNumber == 0)
			{
				_skipReadyScreen = true;
			}
			if (_matchmakingBackgroundImage == null)
			{
				return;
			}
			int index = _npeState.ActiveNPEGameNumber + 3;
			_matchmakingBackgroundImage.sprite = NPECharacterData.Characters[index].PlaneBackground;
			Image letterboxTop = _letterboxTop;
			UnityEngine.Color color = (_letterboxBottom.color = _defaultLetterBoxColor);
			letterboxTop.color = color;
		}
		else if (_minimalLoader)
		{
			AssetLoaderUtils.CleanupImage(_matchmakingBackgroundImage, _matchmakingBackgroundSpriteTracker);
			Image letterboxTop2 = _letterboxTop;
			UnityEngine.Color color = (_letterboxBottom.color = _defaultLetterBoxColor);
			letterboxTop2.color = color;
		}
		else
		{
			_assetLookupSystem.Blackboard.Clear();
			VersusLetterboxPayload payload = _assetLookupSystem.TreeLoader.LoadTree<VersusLetterboxPayload>().GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				Image letterboxTop3 = _letterboxTop;
				UnityEngine.Color color = (_letterboxBottom.color = payload.Color);
				letterboxTop3.color = color;
			}
			else
			{
				Image letterboxTop4 = _letterboxTop;
				UnityEngine.Color color = (_letterboxBottom.color = _defaultLetterBoxColor);
				letterboxTop4.color = color;
			}
			ISetMetadataProvider setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCode = setMetadataProvider.LastPublishedMajorSet;
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<VersusBackgroundPayload> loadedTree))
			{
				VersusBackgroundPayload payload2 = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload2 != null)
				{
					if (payload2.References.Count > 0)
					{
						AltAssetReference<Sprite> altAssetReference = payload2.References.SelectRandom();
						AssetLoaderUtils.TrySetSprite(_matchmakingBackgroundImage, _matchmakingBackgroundSpriteTracker, altAssetReference.RelativePath);
					}
					else
					{
						AssetLoaderUtils.CleanupImage(_matchmakingBackgroundImage, _matchmakingBackgroundSpriteTracker);
					}
				}
			}
		}
		SetPlayerNames();
		bool flag = _matchManager.GameResults.Count > 0;
		_waitingForMatchView.Init(_preGameScene, _keyboardManager, _actionSystem);
		if (flag || _matchManager.HasReconnected || _minimalLoader)
		{
			_waitingForMatchView.SetState(WaitingForMatchView.State.Minimal);
		}
		else
		{
			_waitingForMatchView.SetState(WaitingForMatchView.State.InQueue);
		}
		if (flag)
		{
			uint num = 1u;
			num = _matchManager.WinCondition switch
			{
				MatchWinCondition.Best2Of3 => 2u, 
				MatchWinCondition.Best3Of5 => 3u, 
				_ => 1u, 
			};
			uint num2 = 0u;
			uint num3 = 0u;
			foreach (MatchManager.GameResult gameResult in _matchManager.GameResults)
			{
				if (gameResult.Result == ResultType.WinLoss)
				{
					if (gameResult.Winner == GREPlayerNum.LocalPlayer)
					{
						num2++;
					}
					else
					{
						num3++;
					}
				}
			}
			SetWinPipsForPlayer(_localPlayerElements, num, num2);
			SetWinPipsForPlayer(_opponentElements, num, num3);
		}
		string key = (flag ? "Match/PreGame/Matchmaking_MatchReady" : "Match/PreGame/PreGame_LocalPlayerDetails_You");
		string key2 = (flag ? "Match/PreGame/Sideboarding" : "Match/PreGame/OpponentDetails");
		_localPlayerElements.PlayerDetails.SetText(key);
		_opponentElements.PlayerDetails.SetText(key2);
		IQueueTipProvider queueTipProvider = Pantry.Get<IQueueTipProvider>();
		NewPlayerExperienceStrategy npeGraphStrategy = Pantry.Get<NewPlayerExperienceStrategy>();
		_cyclingTips.StartTips(queueTipProvider, npeGraphStrategy);
		_animator.Init(_npeState, flag, _matchManager.PrivateGameWaitingForMatchMade);
	}

	private void SetWinPipsForPlayer(PlayerElements playerElements, uint winsReq, uint wins)
	{
		for (int i = 0; i < _localPlayerElements.WinPips.Length; i++)
		{
			if (i < winsReq)
			{
				playerElements.WinPips[i].gameObject.SetActive(value: true);
				if (wins > i)
				{
					playerElements.WinPips[i].sprite = playerElements.WinSprite;
				}
			}
		}
	}

	private void SetPlayerNames()
	{
		MatchManager.PlayerInfo localPlayerInfo = _matchManager.LocalPlayerInfo;
		MatchManager.PlayerInfo opponentInfo = _matchManager.OpponentInfo;
		_localPlayerElements.PlayerName.text = localPlayerInfo.ScreenName;
		_localPlayerElements.PlayerName.color = (localPlayerInfo.IsWotc ? _mythicOrangeColor : UnityEngine.Color.white);
		string text = CosmeticsUtils.TitleLocKey(_cosmeticsProvider, localPlayerInfo.TitleSelection);
		_localPlayerElements.PlayerTitle.gameObject.UpdateActive(text != null);
		if (text != null)
		{
			_localPlayerElements.PlayerTitle.SetText(text);
		}
		string text2 = CosmeticsUtils.TitleLocKey(_cosmeticsProvider, opponentInfo.TitleSelection);
		_opponentElements.PlayerTitle.gameObject.UpdateActive(text2 != null);
		if (text2 != null)
		{
			_opponentElements.PlayerTitle.SetText(text2);
		}
		EventContext eventContext = _matchManager.Event;
		bool flag = eventContext != null && eventContext.PlayerEvent?.EventInfo?.FormatType == MDNEFormatType.Constructed;
		bool valueOrDefault = _matchManager.Event?.PlayerEvent?.EventInfo?.IsRanked == true;
		if (_npeState.ActiveNPEGame != null)
		{
			AvatarCharacterData avatarCharacterData = NPECharacterData.Characters[0];
			_localPlayerElements.Avatar.sprite = avatarCharacterData.FullAvatar;
			_localPlayerElements.RankDisplayObject.gameObject.SetActive(value: false);
			AvatarCharacterData avatarCharacterData2 = NPECharacterData.Characters[(int)_npeState.ActiveNPEGame.OpponentPortrait];
			_opponentElements.PlayerName.text = avatarCharacterData2.CharacterName;
			_opponentElements.PlayerName.color = (opponentInfo.IsWotc ? _mythicOrangeColor : UnityEngine.Color.white);
			_opponentElements.Avatar.sprite = avatarCharacterData2.FullAvatar;
			_opponentElements.RankDisplayObject.gameObject.SetActive(value: false);
			return;
		}
		if (_matchManager.PrivateGameWaitingForMatchMade)
		{
			_localPlayerElements.PlayerName.text = localPlayerInfo.WizardsAccountIdForPrivateGaming;
			_localPlayerElements.RankDisplayObject.gameObject.SetActive(value: false);
			AssetLoaderUtils.TrySetSprite(_localPlayerElements.Avatar, _localAvatarSpriteTracker, ProfileUtilities.GetAvatarFullImagePath(_assetLookupSystem, localPlayerInfo.AvatarSelection));
			_opponentElements.PlayerName.text = opponentInfo.WizardsAccountIdForPrivateGaming;
			_opponentElements.RankDisplayObject.gameObject.SetActive(value: false);
			AvatarCharacterData avatarCharacterData3 = NPECharacterData.Characters[0];
			_opponentElements.Avatar.sprite = avatarCharacterData3.FullAvatar;
			return;
		}
		if (localPlayerInfo.RankingClass != RankingClassType.None && !_matchManager.IsPrivateGame && valueOrDefault)
		{
			_localPlayerElements.RankDisplayObject.gameObject.SetActive(value: true);
			_localPlayerElements.RankDisplayObject.IsLimited = !flag;
			RankInfo rankInfo = new RankInfo
			{
				rankClass = localPlayerInfo.RankingClass,
				level = localPlayerInfo.RankingTier,
				steps = 0
			};
			rankInfo.mythicLeaderboardPlace = localPlayerInfo.MythicPlacement;
			rankInfo.mythicPercentile = localPlayerInfo.MythicPercentile;
			_localPlayerElements.RankDisplayObject.CalculateRankDisplay(rankInfo, _assetLookupSystem);
		}
		else
		{
			_localPlayerElements.RankDisplayObject.gameObject.SetActive(value: false);
		}
		AssetLoaderUtils.TrySetSprite(_localPlayerElements.Avatar, _localAvatarSpriteTracker, ProfileUtilities.GetAvatarFullImagePath(_assetLookupSystem, localPlayerInfo.AvatarSelection));
		_opponentElements.PlayerName.text = opponentInfo.ScreenName;
		_opponentElements.PlayerName.color = (opponentInfo.IsWotc ? _mythicOrangeColor : UnityEngine.Color.white);
		if (opponentInfo.RankingClass != RankingClassType.None && !_matchManager.IsPrivateGame && valueOrDefault)
		{
			_opponentElements.RankDisplayObject.gameObject.SetActive(value: true);
			_opponentElements.RankDisplayObject.IsLimited = !flag;
			RankInfo playerRankInfo = new RankInfo
			{
				rankClass = opponentInfo.RankingClass,
				level = opponentInfo.RankingTier,
				steps = 0,
				mythicLeaderboardPlace = opponentInfo.MythicPlacement,
				mythicPercentile = opponentInfo.MythicPercentile
			};
			_opponentElements.RankDisplayObject.CalculateRankDisplay(playerRankInfo, _assetLookupSystem);
		}
		else
		{
			_opponentElements.RankDisplayObject.gameObject.SetActive(value: false);
		}
		AssetLoaderUtils.TrySetSprite(_opponentElements.Avatar, _opponentAvatarSpriteTracker, ProfileUtilities.GetAvatarFullImagePath(_assetLookupSystem, opponentInfo.AvatarSelection));
	}

	private void OnGameFound()
	{
		_waitingForMatchView.SetState(WaitingForMatchView.State.GameFound);
		if (_skipReadyScreen)
		{
			_preGameScene.CompletePreGame();
			return;
		}
		SetPlayerNames();
		AudioManager.PlayAudio(WwiseEvents.match_making_match_found, AudioManager.Default);
		GameVariant? gameVariant = _matchManager?.Variant;
		if (!gameVariant.HasValue || gameVariant != GameVariant.Brawl)
		{
			_localPlayerElements.PlayerDetails.SetText("Match/PreGame/PreGame_LocalPlayerDetails_You");
			_opponentElements.PlayerDetails.gameObject.UpdateActive(active: true);
			_opponentElements.PlayerDetails.SetText("Match/PreGame/OpponentDetails");
		}
		else
		{
			_localPlayerElements.PlayerDetails.gameObject.UpdateActive(active: false);
			_opponentElements.PlayerDetails.gameObject.UpdateActive(active: false);
			SetupCommander(_matchManager.LocalPlayerInfo.CommanderGrpIds, _localPlayerBrawl);
			SetupCommander(_matchManager.OpponentInfo.CommanderGrpIds, _opponentBrawl);
		}
		_cyclingTips.StopTips();
		_animator.Completed += _preGameScene.CompletePreGame;
		_animator.Ready();
	}

	private void SetupCommander(IReadOnlyList<uint> commanderGrpIds, PlayerBrawlElements elements)
	{
		for (int i = 0; i < elements.CommanderFields.Length; i++)
		{
			if (commanderGrpIds.Count <= i)
			{
				elements.CommanderFields[i].transform.parent.gameObject.UpdateActive(active: false);
				continue;
			}
			elements.CommanderFields[i].transform.parent.gameObject.UpdateActive(active: true);
			uint grpId = commanderGrpIds[i];
			elements.CommanderFields[i].text = GetCommanderTitle(_cardDatabase.CardDataProvider, _cardDatabase.GreLocProvider, grpId);
		}
		if ((bool)elements.DeckBox)
		{
			elements.DeckBox.UpdateActive(active: true);
			SetDeckBoxArt(commanderGrpIds[0], elements.DeckMeshLoaders);
			SetDeckBoxColors(commanderGrpIds, elements.DeckBoxColors, elements.GoldManaSymbol);
		}
		static string GetCommanderTitle(ICardDataProvider cardProvider, IGreLocProvider greLocProvider, uint id)
		{
			if (cardProvider.TryGetCardPrintingById(id, out var card))
			{
				if (card != null && card.LinkedFaceType == LinkedFace.SpecializeParent && card != null && card.LinkedFaceGrpIds.Count > 0)
				{
					card = card.LinkedFacePrintings[0];
				}
				return CardUtilities.FormatComplexTitle((card.AltTitleId != 0) ? greLocProvider.GetLocalizedText(card.AltTitleId) : greLocProvider.GetLocalizedText(card.TitleId));
			}
			return string.Empty;
		}
	}

	private void SetDeckBoxArt(uint grpId, MeshRendererReferenceLoader[] loaders)
	{
		if (_cardMaterialBuilder != null)
		{
			DeckBoxUtil.SetDeckBoxTexture(_cardDatabase.CardDataProvider.GetCardPrintingById(grpId)?.ImageAssetPath, _cardMaterialBuilder.TextureLoader, _cardMaterialBuilder.CropDatabase, loaders);
		}
	}

	private void SetDeckBoxColors(IReadOnlyList<uint> grpIds, TextMeshProUGUI colorsTextfield, GameObject goldSymbol)
	{
		colorsTextfield.text = string.Empty;
		CardColorFlags cardColorFlags = CardColorFlags.None;
		foreach (uint grpId in grpIds)
		{
			if (_cardDatabase.CardDataProvider.TryGetCardPrintingById(grpId, out var card) && card != null)
			{
				cardColorFlags |= card.ColorIdentityFlags;
			}
		}
		IReadOnlyList<CardColorFlags> readOnlyList = cardColorFlags.ToDisplayOrder();
		if (readOnlyList != null && readOnlyList.Count < 5)
		{
			string text = ManaUtilities.ConvertToOldSchoolManaText(ManaUtilities.ConvertCardColorFlagsToManaQuantities(readOnlyList));
			text = ManaUtilities.ConvertManaSymbols(text);
			colorsTextfield.text = text;
		}
		goldSymbol.SetActive(readOnlyList.Count >= 5);
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_matchmakingBackgroundImage, _matchmakingBackgroundSpriteTracker);
		AssetLoaderUtils.CleanupImage(_localPlayerElements.Avatar, _localAvatarSpriteTracker);
		AssetLoaderUtils.CleanupImage(_opponentElements.Avatar, _opponentAvatarSpriteTracker);
		_animator?.OnDestroy();
		if ((bool)_preGameScene)
		{
			_preGameScene.GameFound -= OnGameFound;
		}
		if (_localPlayerBrawl.DeckMeshLoaders != null)
		{
			MeshRendererReferenceLoader[] deckMeshLoaders = _localPlayerBrawl.DeckMeshLoaders;
			for (int i = 0; i < deckMeshLoaders.Length; i++)
			{
				deckMeshLoaders[i]?.Cleanup();
			}
			_localPlayerBrawl.DeckMeshLoaders = null;
		}
		if (_opponentBrawl.DeckMeshLoaders != null)
		{
			MeshRendererReferenceLoader[] deckMeshLoaders = _opponentBrawl.DeckMeshLoaders;
			for (int i = 0; i < deckMeshLoaders.Length; i++)
			{
				deckMeshLoaders[i]?.Cleanup();
			}
			_opponentBrawl.DeckMeshLoaders = null;
		}
	}
}
