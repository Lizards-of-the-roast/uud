using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Avatar;
using Assets.Core.DuelScene;
using GreClient.CardData;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.AvatarView;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class DuelScene_AvatarView : MonoBehaviour, IAvatarView, IEntityView
{
	[Serializable]
	public class HighlightSystem
	{
		[SerializeField]
		private SpriteRenderer _renderer;

		[SerializeField]
		private Sprite _normalSprite;

		[SerializeField]
		private Sprite _hotSprite;

		[SerializeField]
		private Sprite _coldSprite;

		[SerializeField]
		private Sprite _selectedSprite;

		private Dictionary<HighlightType, Sprite> _highlights;

		private HighlightType _currentHighlightType;

		public void Init()
		{
			_highlights = new Dictionary<HighlightType, Sprite>
			{
				{
					HighlightType.None,
					_normalSprite
				},
				{
					HighlightType.Hot,
					_hotSprite
				},
				{
					HighlightType.Tepid,
					_hotSprite
				},
				{
					HighlightType.Cold,
					_coldSprite
				},
				{
					HighlightType.Selected,
					_selectedSprite
				}
			};
			Update(HighlightType.None);
		}

		public void Update(HighlightType highlightType)
		{
			if (_currentHighlightType == highlightType)
			{
				return;
			}
			_currentHighlightType = highlightType;
			if (_highlights.TryGetValue(highlightType, out var value))
			{
				if (_renderer.sprite != value)
				{
					_renderer.sprite = value;
				}
			}
			else
			{
				_renderer.sprite = _highlights[HighlightType.None];
				Debug.LogError("Invalid Highlight Type for Player " + highlightType);
			}
		}
	}

	[Serializable]
	public class DecisionIndicator
	{
		[SerializeField]
		private GameObject _effectRoot;

		[SerializeField]
		private GameObject _frameEffectRoot;

		public void UpdatePriority(bool hasPriority)
		{
			_effectRoot.SetActive(hasPriority);
			_frameEffectRoot.SetActive(hasPriority);
		}
	}

	[Serializable]
	public class LifeDisplay
	{
		[SerializeField]
		private TMP_Text _lifeTotalLabel;

		[SerializeField]
		private Vector3 _flyingTextOffset;

		private GameManager _gameManager;

		private int _lifeTotal;

		public Transform TextTransform => _lifeTotalLabel.transform;

		public void Init(GameManager gameManager)
		{
			_gameManager = gameManager;
			UpdateLifeLabel(_lifeTotal);
		}

		public void IncrementLifeTotal(int amount)
		{
			if (amount != 0)
			{
				FlyingText.SpawnAvatarText(_lifeTotalLabel.transform.position + _flyingTextOffset, amount, _gameManager.AssetLookupSystem, _gameManager.UnityPool);
			}
			UpdateLifeLabel(_lifeTotal + amount);
		}

		public void UpdateLifeLabel(int displayedTotal)
		{
			_lifeTotal = displayedTotal;
			_lifeTotalLabel.text = displayedTotal.ToString();
		}

		public void NPEResetLifeDisplay()
		{
			UpdateLifeLabel(_lifeTotal);
		}
	}

	[Serializable]
	public class AvatarEffects
	{
		[SerializeField]
		private Transform _effectRoot;

		private GameObject _avatarObj;

		private Func<MtgPlayer> _getPlayer;

		private GameManager _gameManager;

		private IVfxProvider _vfxProvider;

		private Dictionary<string, GameObject> _persistEffectPrefabPathToInstanceMap = new Dictionary<string, GameObject>();

		private HashSet<string> _persistentEffectAudioEvents = new HashSet<string>();

		private Dictionary<HighlightType, HashSet<GameObject>> _currentHighlightVFX = new Dictionary<HighlightType, HashSet<GameObject>>();

		private AvatarCounterEffects _counterEffects;

		public void Init(GameManager gameManager, GameObject avatarObj, Func<MtgPlayer> getPlayer)
		{
			_gameManager = gameManager;
			_vfxProvider = gameManager.VfxProvider;
			_avatarObj = avatarObj;
			_getPlayer = getPlayer;
			_counterEffects = new AvatarCounterEffects(gameManager, getPlayer);
		}

		public void PlayDefeatEffect(MtgGameLossData gameLossData)
		{
			if (_getPlayer == null)
			{
				return;
			}
			MtgPlayer mtgPlayer = _getPlayer();
			if (mtgPlayer == null)
			{
				return;
			}
			AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.GREPlayerNum = mtgPlayer.ClientPlayerEnum;
			assetLookupSystem.Blackboard.GameState.GameLossData = gameLossData;
			AssetLookupTree<DeathVFX> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<DeathVFX>();
			AssetLookupTree<DeathSFX> assetLookupTree2 = assetLookupSystem.TreeLoader.LoadTree<DeathSFX>();
			DeathVFX payload = assetLookupTree.GetPayload(assetLookupSystem.Blackboard);
			DeathSFX payload2 = assetLookupTree2.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_vfxProvider.PlayVFX(payload.VfxData, null, mtgPlayer, _avatarObj.transform);
				if (payload.HideAvatar)
				{
					SetActiveAfterDelay(payload.HideAvatarAfterSeconds, _avatarObj, active: false);
				}
			}
			if (payload2 != null)
			{
				AudioManager.PlayAudio(payload2.SfxData.AudioEvents, _avatarObj);
			}
		}

		private async void SetActiveAfterDelay(float seconds, GameObject obj, bool active)
		{
			if (seconds > 0f)
			{
				await Task.Delay((int)(seconds * 1000f));
			}
			if ((bool)obj)
			{
				obj.SetActive(active);
			}
		}

		public void HandleAbilityAdded(uint abilityId, uint affectorId)
		{
			if (_getPlayer == null)
			{
				return;
			}
			MtgPlayer mtgPlayer = _getPlayer();
			if (mtgPlayer == null)
			{
				return;
			}
			AbilityPrintingData abilityPrintingById = _gameManager.CardDatabase.AbilityDataProvider.GetAbilityPrintingById(abilityId);
			if (abilityPrintingById == null)
			{
				return;
			}
			ICardDataAdapter effectContext = null;
			MtgCardInstance cardById = _gameManager.CurrentGameState.GetCardById(affectorId);
			if (cardById != null)
			{
				effectContext = CardDataExtensions.CreateWithDatabase(cardById, _gameManager.CardDatabase);
			}
			AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.Player = mtgPlayer;
			assetLookupSystem.Blackboard.GREPlayerNum = mtgPlayer.ClientPlayerEnum;
			assetLookupSystem.Blackboard.Ability = abilityPrintingById;
			AssetLookupTree<GainVfx> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<GainVfx>();
			AssetLookupTree<GainSfx> assetLookupTree2 = assetLookupSystem.TreeLoader.LoadTree<GainSfx>();
			GainVfx payload = assetLookupTree.GetPayload(assetLookupSystem.Blackboard);
			GainSfx payload2 = assetLookupTree2.GetPayload(assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				AudioManager.PlayAudio(payload2.SfxData.AudioEvents, _avatarObj);
			}
			if (payload != null)
			{
				foreach (VfxData vfxData in payload.VfxDatas)
				{
					_vfxProvider.PlayVFX(vfxData, effectContext, mtgPlayer, _avatarObj.transform);
				}
			}
			UpdatePerstentAbilityVisuals(mtgPlayer);
		}

		public void OnReplacementAdded()
		{
			MtgPlayer mtgPlayer = _getPlayer();
			if (mtgPlayer != null)
			{
				UpdatePerstentReplacementVisuals(mtgPlayer);
			}
		}

		private void UpdatePerstentReplacementVisuals(MtgPlayer player)
		{
			AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
			AssetLookupTree<PersistVFX> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<PersistVFX>();
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.Player = player;
			assetLookupSystem.Blackboard.GREPlayerNum = player.ClientPlayerEnum;
			HashSet<string> hashSet = new HashSet<string>();
			HashSet<PersistVFX> hashSet2 = new HashSet<PersistVFX>();
			if (assetLookupTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet2))
			{
				foreach (PersistVFX item in hashSet2)
				{
					foreach (VfxData vfxData in item.VfxDatas)
					{
						foreach (AltAssetReference<GameObject> allPrefab in vfxData.PrefabData.AllPrefabs)
						{
							string relativePath = allPrefab.RelativePath;
							if (!hashSet.Contains(relativePath))
							{
								hashSet.Add(relativePath);
								GameObject value = null;
								if (!_persistEffectPrefabPathToInstanceMap.TryGetValue(relativePath, out value) || value == null)
								{
									value = (_persistEffectPrefabPathToInstanceMap[relativePath] = _gameManager.UnityPool.PopObject(relativePath));
								}
								Transform transform = value.transform;
								value.UpdateActive(active: true);
								transform.SetParent(_effectRoot);
								transform.ZeroOut();
								OffsetData offset = vfxData.Offset;
								transform.localPosition += offset.PositionOffset;
								transform.localEulerAngles += offset.RotationOffset;
								transform.localScale = offset.ScaleMultiplier;
							}
						}
					}
				}
			}
			if (_persistEffectPrefabPathToInstanceMap.Count > 0)
			{
				List<string> list = _gameManager.GenericPool.PopObject<List<string>>();
				list.Clear();
				list.AddRange(_persistEffectPrefabPathToInstanceMap.Keys);
				foreach (string item2 in list)
				{
					if (!hashSet.Contains(item2))
					{
						GameObject gameObject2 = _persistEffectPrefabPathToInstanceMap[item2];
						if ((bool)gameObject2)
						{
							_gameManager.UnityPool.PushObject(gameObject2);
						}
						_persistEffectPrefabPathToInstanceMap.Remove(item2);
					}
				}
				list.Clear();
				_gameManager.GenericPool.PushObject(list);
			}
			UpdatePersistentSFX(hashSet, playNewAudio: true);
		}

		private void UpdatePerstentAbilityVisuals(MtgPlayer player)
		{
			AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
			AssetLookupTree<PersistVFX> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<PersistVFX>();
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.Player = player;
			HashSet<PersistVFX> hashSet = _gameManager.GenericPool.PopObject<HashSet<PersistVFX>>();
			if (assetLookupTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet))
			{
				List<string> list = _gameManager.GenericPool.PopObject<List<string>>();
				foreach (PersistVFX item in hashSet)
				{
					foreach (VfxData vfxData in item.VfxDatas)
					{
						foreach (AltAssetReference<GameObject> allPrefab in vfxData.PrefabData.AllPrefabs)
						{
							if (!string.IsNullOrEmpty(allPrefab.RelativePath))
							{
								list.Add(allPrefab.RelativePath);
							}
						}
					}
				}
				foreach (string item2 in list)
				{
					if (!_persistEffectPrefabPathToInstanceMap.ContainsKey(item2))
					{
						GameObject gameObject = _gameManager.UnityPool.PopObject(item2);
						if (gameObject != null)
						{
							_persistEffectPrefabPathToInstanceMap.Add(item2, gameObject);
						}
					}
				}
				List<string> list2 = _gameManager.GenericPool.PopObject<List<string>>();
				list2.Clear();
				list2.AddRange(_persistEffectPrefabPathToInstanceMap.Keys);
				foreach (string item3 in list2)
				{
					if (!list.Contains(item3))
					{
						GameObject gameObject2 = _persistEffectPrefabPathToInstanceMap[item3];
						if ((bool)gameObject2)
						{
							_gameManager.UnityPool.PushObject(gameObject2);
						}
						_persistEffectPrefabPathToInstanceMap.Remove(item3);
					}
				}
				list2.Clear();
				_gameManager.GenericPool.PushObject(list2);
				foreach (PersistVFX item4 in hashSet)
				{
					foreach (VfxData vfxData2 in item4.VfxDatas)
					{
						foreach (AltAssetReference<GameObject> allPrefab2 in vfxData2.PrefabData.AllPrefabs)
						{
							string relativePath = allPrefab2.RelativePath;
							if (!string.IsNullOrEmpty(relativePath))
							{
								GameObject gameObject3 = _persistEffectPrefabPathToInstanceMap[relativePath];
								if ((bool)gameObject3)
								{
									gameObject3.SetActive(value: true);
									gameObject3.transform.SetParent(_effectRoot);
									gameObject3.transform.ZeroOut();
									OffsetData offset = vfxData2.Offset;
									gameObject3.transform.localPosition += offset.PositionOffset;
									gameObject3.transform.localEulerAngles += offset.RotationOffset;
									gameObject3.transform.localScale = offset.ScaleMultiplier;
								}
							}
						}
					}
				}
				list.Clear();
				_gameManager.GenericPool.PushObject(list);
			}
			hashSet.Clear();
			_gameManager.GenericPool.PushObject(hashSet);
			HashSet<string> hashSet2 = _gameManager.GenericPool.PopObject<HashSet<string>>();
			hashSet2.Clear();
			hashSet2.UnionWith(_persistEffectPrefabPathToInstanceMap.Keys);
			UpdatePersistentSFX(hashSet2, playNewAudio: true);
			hashSet2.Clear();
			_gameManager.GenericPool.PushObject(hashSet2, tryClear: false);
		}

		private void UpdatePersistentSFX(ICollection<string> audioEvents, bool playNewAudio)
		{
			MtgPlayer mtgPlayer = _getPlayer();
			audioEvents.Clear();
			AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.Player = mtgPlayer;
			assetLookupSystem.Blackboard.GREPlayerNum = mtgPlayer.ClientPlayerEnum;
			AssetLookupTree<PersistSfx> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<PersistSfx>();
			HashSet<PersistSfx> hashSet = _gameManager.GenericPool.PopObject<HashSet<PersistSfx>>();
			if (assetLookupTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet))
			{
				foreach (PersistSfx item in hashSet)
				{
					foreach (AudioEvent audioEvent in item.SfxData.AudioEvents)
					{
						if (!audioEvents.Contains(audioEvent.WwiseEventName))
						{
							audioEvents.Add(audioEvent.WwiseEventName);
							if (_persistentEffectAudioEvents.Add(audioEvent.WwiseEventName) && playNewAudio)
							{
								AudioManager.PlayAudio(audioEvent, _effectRoot.gameObject);
							}
						}
					}
				}
			}
			hashSet.Clear();
			_gameManager.GenericPool.PushObject(hashSet, tryClear: false);
			_persistentEffectAudioEvents.IntersectWith(audioEvents);
		}

		public void OnReplacementRemoved()
		{
			MtgPlayer mtgPlayer = _getPlayer();
			if (mtgPlayer != null)
			{
				UpdatePerstentReplacementVisuals(mtgPlayer);
			}
		}

		public void HandleAbilityRemoved(uint abilityId)
		{
			if (_getPlayer == null)
			{
				return;
			}
			MtgPlayer mtgPlayer = _getPlayer();
			if (mtgPlayer == null)
			{
				return;
			}
			UpdatePerstentAbilityVisuals(mtgPlayer);
			if (mtgPlayer.Abilities.Exists((AbilityPrintingData x) => x.Id == abilityId))
			{
				return;
			}
			AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
			AssetLookupTree<PersistVFX> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<PersistVFX>();
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.Player = mtgPlayer;
			List<string> list = _gameManager.GenericPool.PopObject<List<string>>();
			HashSet<PersistVFX> hashSet = _gameManager.GenericPool.PopObject<HashSet<PersistVFX>>();
			if (assetLookupTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet))
			{
				foreach (PersistVFX item in hashSet)
				{
					foreach (VfxData vfxData in item.VfxDatas)
					{
						foreach (AltAssetReference<GameObject> allPrefab in vfxData.PrefabData.AllPrefabs)
						{
							string relativePath = allPrefab.RelativePath;
							list.Add(relativePath);
						}
					}
				}
			}
			hashSet.Clear();
			_gameManager.GenericPool.PushObject(hashSet);
			if (_persistEffectPrefabPathToInstanceMap.Count > 0)
			{
				List<string> list2 = _gameManager.GenericPool.PopObject<List<string>>();
				list2.Clear();
				list2.AddRange(_persistEffectPrefabPathToInstanceMap.Keys);
				foreach (string item2 in list2)
				{
					if (!list.Contains(item2))
					{
						GameObject gameObject = _persistEffectPrefabPathToInstanceMap[item2];
						if ((bool)gameObject)
						{
							_gameManager.UnityPool.PushObject(gameObject);
						}
						_persistEffectPrefabPathToInstanceMap.Remove(item2);
					}
				}
				list2.Clear();
				_gameManager.GenericPool.PushObject(list2);
			}
			UpdatePersistentSFX(list, playNewAudio: false);
			list.Clear();
			_gameManager.GenericPool.PushObject(list);
		}

		public void UpdatePersistentVFX()
		{
			MtgPlayer player = _getPlayer();
			UpdatePerstentAbilityVisuals(player);
		}

		public void UpdateHighlightVFX(HighlightType highlightType)
		{
			foreach (KeyValuePair<HighlightType, HashSet<GameObject>> item in _currentHighlightVFX)
			{
				if (item.Key == highlightType)
				{
					continue;
				}
				foreach (GameObject item2 in item.Value)
				{
					if ((bool)item2)
					{
						_gameManager.UnityPool.PushObject(item2);
					}
				}
				item.Value.Clear();
			}
			MtgPlayer mtgPlayer = _getPlayer();
			AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.Player = mtgPlayer;
			assetLookupSystem.Blackboard.GREPlayerNum = mtgPlayer.ClientPlayerEnum;
			assetLookupSystem.Blackboard.HighlightType = highlightType;
			HighlightVFX payload = assetLookupSystem.TreeLoader.LoadTree<HighlightVFX>().GetPayload(assetLookupSystem.Blackboard);
			if (payload == null)
			{
				return;
			}
			GameObject gameObject = _vfxProvider.PlayVFX(payload.VfxData, null, mtgPlayer, _avatarObj.transform);
			if ((bool)gameObject)
			{
				if (!_currentHighlightVFX.TryGetValue(highlightType, out var value))
				{
					value = _gameManager.GenericPool.PopObject<HashSet<GameObject>>();
					_currentHighlightVFX.Add(highlightType, value);
				}
				value.Add(gameObject);
			}
		}

		public void UpdateCounterFX(Dictionary<CounterType, int> oldCounters, Dictionary<CounterType, int> newCounters)
		{
			_counterEffects.UpdateCounterFX(oldCounters, newCounters);
		}

		public void CleanUp()
		{
			foreach (KeyValuePair<HighlightType, HashSet<GameObject>> item in _currentHighlightVFX)
			{
				foreach (GameObject item2 in item.Value)
				{
					if ((bool)item2)
					{
						_gameManager.UnityPool.PushObject(item2);
					}
				}
				item.Value.Clear();
				_gameManager.GenericPool.PushObject(item.Value);
			}
			_currentHighlightVFX.Clear();
		}
	}

	[SerializeField]
	private GREPlayerNum _playerType;

	[Space(10f)]
	[Header("Avatar Components")]
	[SerializeField]
	private LifeDisplay _lifeDisplay;

	[SerializeField]
	private ManaPool _manaPool;

	[SerializeField]
	private DuelScene_CounterPool _counterPool;

	[SerializeField]
	private HighlightSystem _highlightSystem;

	[SerializeField]
	private DecisionIndicator _decisionIndicator;

	[SerializeField]
	private SpriteRenderer _portrait;

	[SerializeField]
	private Transform _framePosition;

	[SerializeField]
	private Transform _lifeDisplayPosition;

	[Space(10f)]
	[Header("Avatar Interaction System")]
	[SerializeField]
	private ClickAndHoldButton LifePillButton;

	[SerializeField]
	private ClickAndHoldButton PortraitButton;

	[SerializeField]
	private Transform _targetEffectsRoot;

	[Space(10f)]
	[Header("Related Avatar Systems")]
	[SerializeField]
	private AvatarEffects _effects;

	[SerializeField]
	private Animator _playerTurnFrame;

	[SerializeField]
	private AvatarPhaseIcon[] _phaseIcons;

	[SerializeField]
	public Transform NPEChatBubble_TargetPosition;

	[SerializeField]
	private bool _useThumbnailAvatar;

	[SerializeField]
	private GameObject _concessionIndicator;

	private AssetLoader.AssetTracker<Sprite> _spriteTracker = new AssetLoader.AssetTracker<Sprite>("PortraitSpriteTracker");

	public AvatarInput PortraitInput;

	public AvatarInput LifeInput;

	private Phase _currentPhase;

	private AvatarPhaseIcon _currentIcon;

	private UXEventQueue _uxEventQueue;

	private readonly List<AvatarFramePart> _frameParts = new List<AvatarFramePart>();

	private readonly HashSet<AvatarFrameColorOverride> _colorOverridesCache = new HashSet<AvatarFrameColorOverride>();

	private GameManager _gameManager;

	private AssetLookupSystem _assetLookupSystem;

	public Transform FramePosition => _framePosition;

	public Transform LifeDisplayPosition => _lifeDisplayPosition;

	public Transform TargetTransform => _targetEffectsRoot;

	public Transform EffectsRoot => _targetEffectsRoot;

	public AvatarPhaseIcon[] PhaseIcons => _phaseIcons;

	public Transform TurnFramePosition => _playerTurnFrame.gameObject.transform;

	public Transform LifeTextTransform => _lifeDisplay.TextTransform;

	public RectTransform ManaPoolRoot => _manaPool.Rect;

	public RectTransform CounterPoolRoot => _counterPool.Rect;

	public MtgPlayer Model { get; private set; }

	public uint InstanceId => Model?.InstanceId ?? 0;

	public bool IsLocalPlayer => _playerType == GREPlayerNum.LocalPlayer;

	public bool ShowingPlayerName { get; private set; }

	public Transform ArrowRoot
	{
		get
		{
			if (!PlatformUtils.IsHandheld() || PlatformUtils.IsAspectRatio4x3())
			{
				return base.transform;
			}
			return _lifeDisplayPosition;
		}
	}

	public event Action<uint> ManaSelected
	{
		add
		{
			_manaPool.ManaSelected += value;
		}
		remove
		{
			_manaPool.ManaSelected -= value;
		}
	}

	private void Awake()
	{
		PortraitInput = new AvatarInput(this, PortraitButton);
		LifeInput = new AvatarInput(this, LifePillButton);
		_frameParts.AddRange(GetComponentsInChildren<AvatarFramePart>(includeInactive: true));
		AvatarPhaseIcon[] phaseIcons = _phaseIcons;
		foreach (AvatarPhaseIcon avatarPhaseIcon in phaseIcons)
		{
			_frameParts.AddRange(avatarPhaseIcon.GetComponentsInChildren<AvatarPhaseIconPart>(includeInactive: true));
		}
		foreach (AvatarFramePart framePart in _frameParts)
		{
			framePart.GetDefaults();
		}
	}

	public void Init(GameManager gameManager, MtgPlayer player, string portraitId)
	{
		_gameManager = gameManager;
		_assetLookupSystem = _gameManager.AssetLookupSystem;
		Model = player;
		SetPortraitAvatarId(portraitId, gameManager.AssetLookupSystem);
		_lifeDisplay.Init(gameManager);
		_manaPool.Init(gameManager.UIManager.TooltipSystem, gameManager.PromptEngine, gameManager.UnityPool);
		_counterPool.Init(gameManager.UIManager.TooltipSystem, gameManager.PromptEngine, gameManager.UnityPool);
		_highlightSystem.Init();
		_effects.Init(gameManager, _portrait.gameObject, () => Model);
		ButtonPhaseLadder phaseLadder = gameManager.UIManager.PhaseLadder;
		AvatarPhaseIcon[] phaseIcons = _phaseIcons;
		foreach (AvatarPhaseIcon avatarPhaseIcon in phaseIcons)
		{
			phaseLadder.PhaseIcons.Add(avatarPhaseIcon);
			avatarPhaseIcon.Init(phaseLadder);
			avatarPhaseIcon.InitFullControlToggle(gameManager.UIManager.FullControl);
		}
		_uxEventQueue = gameManager.UXEventQueue;
		if (_uxEventQueue != null)
		{
			_uxEventQueue.EventExecutionCommenced += OnUxEventCommenced;
		}
		SetLifeTotal(Model.LifeTotal);
		UpdateCounters(Model.Counters);
	}

	private void OnUxEventCommenced(UXEvent uxEvent)
	{
		if (uxEvent is GameStatePlaybackCommencedUXEvent gameStatePlaybackCommencedUXEvent)
		{
			OnVisibleGameStateChangeCommenced(gameStatePlaybackCommencedUXEvent.GameState);
		}
	}

	private void UpdateFrameColors()
	{
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AvatarFrameColorOverride> loadedTree))
		{
			return;
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.GREPlayerNum = _playerType;
		_assetLookupSystem.Blackboard.Player = Model;
		foreach (AvatarFramePart framePart in _frameParts)
		{
			_assetLookupSystem.Blackboard.AvatarFramePart = framePart.Type;
			_assetLookupSystem.Blackboard.PhaseIconType = ((framePart is AvatarPhaseIconPart avatarPhaseIconPart) ? avatarPhaseIconPart.IconType : PhaseIconType.None);
			_colorOverridesCache.Clear();
			loadedTree.GetPayloadLayered(_assetLookupSystem.Blackboard, _colorOverridesCache);
			framePart.Recolor(_colorOverridesCache);
		}
	}

	private void OnVisibleGameStateChangeCommenced(MtgGameState newState)
	{
		uint instanceId = Model.InstanceId;
		Model = newState.GetPlayerById(instanceId);
		if (newState.ActivePlayer == null)
		{
			return;
		}
		if (newState.ActivePlayer.InstanceId != instanceId)
		{
			ToggleCurrentIcon(active: false);
			_playerTurnFrame.SetTrigger("None");
		}
		else if (_currentPhase != newState.CurrentPhase)
		{
			_currentPhase = newState.CurrentPhase;
			ToggleCurrentIcon(active: false);
			_currentIcon = _phaseIcons.Find(newState, (AvatarPhaseIcon icon, MtgGameState data) => icon.Phase == data.CurrentPhase && icon.isActiveAndEnabled && (icon.Step == data.CurrentStep || icon.Step == Step.None) && (icon.StopOnlyThisPlayer == data.ActivePlayer.ClientPlayerEnum || icon.StopOnlyThisPlayer == GREPlayerNum.Invalid));
			ToggleCurrentIcon(active: true);
			switch (_currentPhase)
			{
			case Phase.Beginning:
				_playerTurnFrame.SetTrigger("Begin");
				break;
			case Phase.Main1:
				_playerTurnFrame.SetTrigger("FirstMain");
				break;
			case Phase.Combat:
				_playerTurnFrame.SetTrigger("Combat");
				break;
			case Phase.Main2:
				_playerTurnFrame.SetTrigger("SecondMain");
				break;
			case Phase.Ending:
				_playerTurnFrame.SetTrigger("End");
				break;
			}
		}
		if (_concessionIndicator != null)
		{
			_concessionIndicator.UpdateActive(Model.ControllerType == ControllerType.AiMultiplayerConcession);
		}
	}

	private void ToggleCurrentIcon(bool active)
	{
		if (_currentIcon != null)
		{
			_currentIcon.Lit = active;
		}
	}

	private void SetPortraitAvatarId(string avatarId, AssetLookupSystem assetLookupSystem)
	{
		string text = (_useThumbnailAvatar ? new Func<AssetLookupSystem, string, string>(ProfileUtilities.GetAvatarThumbImagePath) : new Func<AssetLookupSystem, string, string>(ProfileUtilities.GetAvatarBustImagePath))(assetLookupSystem, avatarId);
		if (!string.IsNullOrEmpty(text))
		{
			AssetLoaderUtils.TrySetRendererSprite(_portrait, _spriteTracker, text);
		}
	}

	public void ShowPlayerNames(bool enabled)
	{
		ShowingPlayerName = enabled;
		if (IsLocalPlayer)
		{
			_gameManager.UIManager.PlayerNames?.ActivateInfoItemsOnPlayerName(GREPlayerNum.LocalPlayer, enabled);
			_gameManager.TimerManager.LocalPlayerTimeoutDisplay?.gameObject.SetActive(enabled);
		}
		else
		{
			_gameManager.UIManager.PlayerNames?.ActivateInfoItemsOnPlayerName(GREPlayerNum.Opponent, enabled);
			_gameManager.TimerManager.OpponentPlayerTimeoutDisplay?.gameObject.SetActive(enabled);
		}
	}

	public void SetLifeTotal(int amount)
	{
		_lifeDisplay.UpdateLifeLabel(amount);
	}

	public void IncrementLifeTotal(int amount)
	{
		_lifeDisplay.IncrementLifeTotal(amount);
	}

	public void UpdateDecidingPlayer(bool hasPriority)
	{
		_decisionIndicator.UpdatePriority(hasPriority);
		UpdateFrameColors();
	}

	public void PlayerControllerChanged()
	{
		UpdateFrameColors();
	}

	public void PlayDefeatEffect(MtgGameLossData gameLossData)
	{
		_effects.PlayDefeatEffect(gameLossData);
	}

	public void UpdateManaPool(List<MtgMana> manaPool)
	{
		_manaPool.Mana = manaPool;
	}

	public void HighlightMana(Dictionary<uint, HighlightType> highlights)
	{
		_manaPool.Highlights = highlights;
	}

	public Transform GetTransformForManaButton(MtgMana mana)
	{
		return _manaPool.GetResourceTransform(mana);
	}

	public void UpdateCounters(Dictionary<CounterType, int> counters)
	{
		_effects.UpdateCounterFX(_counterPool.Counters, counters);
		_counterPool.Counters = counters;
	}

	public void SetCounterHighlights(params CounterType[] counters)
	{
		_counterPool.Highlights = counters;
	}

	public void HandleAbilityAdded(uint abilityId, uint affectorId)
	{
		if (_effects != null)
		{
			_effects.HandleAbilityAdded(abilityId, affectorId);
		}
	}

	public void OnReplacementAdded()
	{
		if (_effects != null)
		{
			_effects.OnReplacementAdded();
		}
	}

	public void OnReplacementRemoved()
	{
		if (_effects != null)
		{
			_effects.OnReplacementRemoved();
		}
	}

	public void HandleAbilityRemoved(uint abilityId)
	{
		if (_effects != null)
		{
			_effects.HandleAbilityRemoved(abilityId);
		}
	}

	public void HandleQualificationAdded(QualificationData data)
	{
		if (_effects != null)
		{
			_effects.UpdatePersistentVFX();
		}
	}

	public void HandleQualificationRemoved(QualificationData data)
	{
		if (_effects != null)
		{
			_effects.UpdatePersistentVFX();
		}
	}

	public void UpdateHighlight(HighlightType highlightType)
	{
		_highlightSystem.Update(highlightType);
		_effects.UpdateHighlightVFX(highlightType);
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupSpriteRenderer(_portrait, _spriteTracker, null);
		PortraitInput?.Dispose();
		LifeInput?.Dispose();
		_effects.CleanUp();
		if (_gameManager != null)
		{
			ButtonPhaseLadder phaseLadder = _gameManager.UIManager.PhaseLadder;
			AvatarPhaseIcon[] phaseIcons = _phaseIcons;
			foreach (AvatarPhaseIcon item in phaseIcons)
			{
				phaseLadder.PhaseIcons.Remove(item);
			}
			_gameManager = null;
		}
		if (_uxEventQueue != null)
		{
			_uxEventQueue.EventExecutionCommenced -= OnUxEventCommenced;
			_uxEventQueue = null;
		}
		foreach (AvatarFramePart framePart in _frameParts)
		{
			framePart.Cleanup();
		}
		_frameParts.Clear();
	}
}
