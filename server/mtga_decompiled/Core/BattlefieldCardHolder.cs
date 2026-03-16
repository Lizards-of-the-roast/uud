using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Card;
using AssetLookupTree.Payloads.RegionTransfer;
using GreClient.CardData;
using GreClient.Rules;
using InteractionSystem;
using MovementSystem;
using Pooling;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Duel;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Battlefield;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class BattlefieldCardHolder : ZoneCardHolderBase, IBattlefieldCardHolder, ICardHolder
{
	public struct StackCounterData
	{
		public Vector3 positon;

		public int count;

		public uint parentInstanceId;
	}

	public class BattlefieldLayout : ICardLayout
	{
		private class StackAgeComparer : IComparer<BattlefieldStack>
		{
			private Dictionary<uint, uint> _titleIdToStackAge;

			public StackAgeComparer(Dictionary<uint, uint> titleIdToStackAge)
			{
				Reinitialize(titleIdToStackAge);
			}

			public void Reinitialize(Dictionary<uint, uint> titleIdToStackAge)
			{
				_titleIdToStackAge = titleIdToStackAge;
			}

			public int Compare(BattlefieldStack x, BattlefieldStack y)
			{
				int num = 0;
				ICardDataAdapter stackParentModel = x.StackParentModel;
				ICardDataAdapter stackParentModel2 = y.StackParentModel;
				num = CompareControllerIds(stackParentModel.Controller, stackParentModel2.Controller);
				if (num != 0)
				{
					return num;
				}
				if (stackParentModel.TitleId != stackParentModel2.TitleId)
				{
					num = -_titleIdToStackAge[stackParentModel.TitleId].CompareTo(_titleIdToStackAge[stackParentModel2.TitleId]);
				}
				if (num == 0)
				{
					num = -x.Age.CompareTo(y.Age);
				}
				return num;
			}

			private static int CompareControllerIds(MtgPlayer x, MtgPlayer y)
			{
				uint value = x?.InstanceId ?? 0;
				return (y?.InstanceId ?? 0).CompareTo(value);
			}
		}

		private class StackBlockerAndAgeComparer : IComparer<BattlefieldStack>
		{
			private readonly Dictionary<uint, float> _attackerPositions;

			private readonly StackAgeComparer _stackAgeComparer;

			private List<CardLayoutData> _layoutDatas;

			private List<BattlefieldStack> _attackerStacks;

			private Dictionary<uint, uint> _blockerTitleIdToStackAge;

			public StackBlockerAndAgeComparer(List<CardLayoutData> layoutDatas, List<BattlefieldStack> attackerStacks, Dictionary<uint, uint> blockerTitleIdToStackAge)
			{
				_attackerPositions = new Dictionary<uint, float>();
				_stackAgeComparer = new StackAgeComparer(blockerTitleIdToStackAge);
				Reinitialize(layoutDatas, attackerStacks, blockerTitleIdToStackAge);
			}

			public void Reinitialize(List<CardLayoutData> layoutDatas, List<BattlefieldStack> attackerStacks, Dictionary<uint, uint> blockerTitleIdToStackAge)
			{
				_layoutDatas = layoutDatas;
				_attackerStacks = attackerStacks;
				_blockerTitleIdToStackAge = blockerTitleIdToStackAge;
				_attackerPositions.Clear();
				_stackAgeComparer.Reinitialize(_blockerTitleIdToStackAge);
				foreach (BattlefieldStack attackerStack in _attackerStacks)
				{
					foreach (DuelScene_CDC allCard in attackerStack.AllCards)
					{
						if (!allCard)
						{
							continue;
						}
						foreach (CardLayoutData layoutData in _layoutDatas)
						{
							if (!(layoutData.Card != allCard))
							{
								_attackerPositions[layoutData.Card.InstanceId] = layoutData.Position.x;
								break;
							}
						}
					}
				}
			}

			public int Compare(BattlefieldStack x, BattlefieldStack y)
			{
				bool isBlockStack = x.IsBlockStack;
				bool isBlockStack2 = y.IsBlockStack;
				if (!isBlockStack && !isBlockStack2)
				{
					return _stackAgeComparer.Compare(x, y);
				}
				if (isBlockStack != isBlockStack2)
				{
					return -isBlockStack2.CompareTo(isBlockStack);
				}
				ICardDataAdapter stackParentModel = x.StackParentModel;
				ICardDataAdapter stackParentModel2 = y.StackParentModel;
				uint key = stackParentModel.Instance.BlockingIds.FirstOrDefault();
				uint key2 = stackParentModel2.Instance.BlockingIds.FirstOrDefault();
				_attackerPositions.TryGetValue(key, out var value);
				_attackerPositions.TryGetValue(key2, out var value2);
				int num = value.CompareTo(value2);
				if (num != 0)
				{
					return num;
				}
				return _stackAgeComparer.Compare(x, y);
			}
		}

		public const float OFFSCREEN_Y = -5f;

		protected BattlefieldCardHolder _battlefield;

		protected BattlefieldRegion _localCreatureRegion;

		protected BattlefieldRegion _localLandRegion;

		protected BattlefieldRegion _localArtifactRegion;

		protected BattlefieldRegion _localPlaneswalkerRegion;

		protected BattlefieldRegion _opponentCreatureRegion;

		protected BattlefieldRegion _opponentLandRegion;

		protected BattlefieldRegion _opponentArtifactRegion;

		protected BattlefieldRegion _opponentPlaneswalkerRegion;

		public readonly List<CdcStackCounterView> ActiveStackCounters = new List<CdcStackCounterView>();

		private readonly Dictionary<CardType, Dictionary<GREPlayerNum, BattlefieldRegion>> _regionsByCardTypeAndOwner;

		protected readonly List<Rect> _takenRects = new List<Rect>();

		protected readonly List<StackCounterData> _pendingStackCounters = new List<StackCounterData>();

		public readonly Dictionary<DuelScene_CDC, KeyValuePair<BattlefieldRegion, BattlefieldRegion>> CardRegionChanges = new Dictionary<DuelScene_CDC, KeyValuePair<BattlefieldRegion, BattlefieldRegion>>();

		protected readonly IBattlefieldStackFactory _stackFactory;

		protected readonly DuelSceneLogger _logger;

		protected readonly IUnityObjectPool _unityPool;

		protected readonly UXEventQueue _uxEventQueue;

		protected readonly IGameStateProvider _gameStateProvider;

		protected readonly IBrowserProvider _browserProvider;

		protected readonly ICardDatabaseAdapter _cardDatabase;

		protected readonly GameInteractionSystem _interactionSystem;

		protected readonly NPEDirector _npeDirector;

		protected List<DuelScene_CDC> _unattachedCardsCache = new List<DuelScene_CDC>(4);

		protected Dictionary<uint, List<DuelScene_CDC>> _attachedCardsByParentCache = new Dictionary<uint, List<DuelScene_CDC>>(4);

		protected List<BattlefieldStack> _stacksCache = new List<BattlefieldStack>(4);

		protected Dictionary<uint, uint> _titleIdToOldestStackAgeLocalLands = new Dictionary<uint, uint>();

		protected Dictionary<uint, uint> _titleIdToOldestStackAgeLocalArtifacts = new Dictionary<uint, uint>();

		protected Dictionary<uint, uint> _titleIdToOldestStackAgeLocalPlaneswalkers = new Dictionary<uint, uint>();

		protected Dictionary<uint, uint> _titleIdToOldestStackAgeLocalCreatures = new Dictionary<uint, uint>();

		protected Dictionary<uint, uint> _titleIdToOldestStackAgeOpponentLands = new Dictionary<uint, uint>();

		protected Dictionary<uint, uint> _titleIdToOldestStackAgeOpponentArtifacts = new Dictionary<uint, uint>();

		protected Dictionary<uint, uint> _titleIdToOldestStackAgeOpponentPlaneswalkers = new Dictionary<uint, uint>();

		protected Dictionary<uint, uint> _titleIdToOldestStackAgeOpponentCreatures = new Dictionary<uint, uint>();

		private static HashSet<MtgCardInstance> _attachmentChain = new HashSet<MtgCardInstance>();

		private HashSet<uint> _titleIdsToRemoveCache = new HashSet<uint>();

		private StackAgeComparer _stackAgeComparer;

		private StackBlockerAndAgeComparer _stackBlockerAndAgeComparer;

		public BattlefieldRegion[] Regions { get; protected set; }

		public Dictionary<CardType, Dictionary<GREPlayerNum, BattlefieldRegion>> RegionsByCardTypeAndOwner => _regionsByCardTypeAndOwner;

		public BattlefieldLayout(BattlefieldCardHolder holder, DuelSceneLogger logger, IBattlefieldStackFactory stackFactory, IContext context, GameInteractionSystem interactionSystem, NPEDirector npeDirector)
		{
			_battlefield = holder;
			_stackFactory = stackFactory;
			_logger = logger;
			IObjectPool objectPool = context.Get<IObjectPool>() ?? NullObjectPool.Default;
			_unityPool = context.Get<IUnityObjectPool>() ?? NullUnityObjectPool.Default;
			_uxEventQueue = context.Get<UXEventQueue>();
			_gameStateProvider = context.Get<IGameStateProvider>() ?? NullGameStateProvider.Default;
			_browserProvider = context.Get<IBrowserProvider>() ?? NullBrowserProvider.Default;
			_cardDatabase = context.Get<ICardDatabaseAdapter>() ?? NullCardDatabaseAdapter.Default;
			_interactionSystem = interactionSystem;
			_npeDirector = npeDirector;
			_localCreatureRegion = new BattlefieldRegion(holder.LocalCreatureRegion, opponent: false, holder, objectPool);
			_localLandRegion = new BattlefieldRegion(holder.LocalLandRegion, opponent: false, holder, objectPool);
			_localArtifactRegion = new BattlefieldRegion(holder.LocalArtifactRegion, opponent: false, holder, objectPool);
			_localPlaneswalkerRegion = new BattlefieldRegion(holder.LocalPlaneswalkerRegion, opponent: false, holder, objectPool);
			_opponentCreatureRegion = new BattlefieldRegion(holder.OpponentCreatureRegion, opponent: true, holder, objectPool);
			_opponentLandRegion = new BattlefieldRegion(holder.OpponentLandRegion, opponent: true, holder, objectPool);
			_opponentArtifactRegion = new BattlefieldRegion(holder.OpponentArtifactRegion, opponent: true, holder, objectPool);
			_opponentPlaneswalkerRegion = new BattlefieldRegion(holder.OpponentPlaneswalkerRegion, opponent: true, holder, objectPool);
			Regions = new BattlefieldRegion[8] { _localCreatureRegion, _localLandRegion, _localArtifactRegion, _localPlaneswalkerRegion, _opponentCreatureRegion, _opponentLandRegion, _opponentArtifactRegion, _opponentPlaneswalkerRegion };
			Dictionary<GREPlayerNum, BattlefieldRegion> value = new Dictionary<GREPlayerNum, BattlefieldRegion>
			{
				[GREPlayerNum.LocalPlayer] = _localCreatureRegion,
				[GREPlayerNum.Opponent] = _opponentCreatureRegion
			};
			Dictionary<GREPlayerNum, BattlefieldRegion> value2 = new Dictionary<GREPlayerNum, BattlefieldRegion>
			{
				[GREPlayerNum.LocalPlayer] = _localLandRegion,
				[GREPlayerNum.Opponent] = _opponentLandRegion
			};
			Dictionary<GREPlayerNum, BattlefieldRegion> value3 = new Dictionary<GREPlayerNum, BattlefieldRegion>
			{
				[GREPlayerNum.LocalPlayer] = _localArtifactRegion,
				[GREPlayerNum.Opponent] = _opponentArtifactRegion
			};
			Dictionary<GREPlayerNum, BattlefieldRegion> value4 = new Dictionary<GREPlayerNum, BattlefieldRegion>
			{
				[GREPlayerNum.LocalPlayer] = _localPlaneswalkerRegion,
				[GREPlayerNum.Opponent] = _opponentPlaneswalkerRegion
			};
			_regionsByCardTypeAndOwner = new Dictionary<CardType, Dictionary<GREPlayerNum, BattlefieldRegion>>
			{
				{
					CardType.Creature,
					value
				},
				{
					CardType.Artifact,
					value3
				},
				{
					CardType.Enchantment,
					value3
				},
				{
					CardType.Land,
					value2
				},
				{
					CardType.Planeswalker,
					value4
				}
			};
			_uxEventQueue.EventExecutionCommenced += OnUxEventCommenced;
		}

		public void Cleanup()
		{
			foreach (CdcStackCounterView activeStackCounter in ActiveStackCounters)
			{
				activeStackCounter.Cleanup();
				_unityPool.PushObject(activeStackCounter.gameObject);
			}
			ActiveStackCounters.Clear();
			_uxEventQueue.EventExecutionCommenced -= OnUxEventCommenced;
		}

		public virtual void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
		{
			Dictionary<BattlefieldRegion, HashSet<DuelScene_CDC>> dictionary = new Dictionary<BattlefieldRegion, HashSet<DuelScene_CDC>>();
			BattlefieldRegion[] regions = Regions;
			foreach (BattlefieldRegion battlefieldRegion in regions)
			{
				dictionary[battlefieldRegion] = new HashSet<DuelScene_CDC>(battlefieldRegion.AllCards);
				battlefieldRegion.Stacks.Clear();
			}
			_takenRects.Clear();
			_pendingStackCounters.Clear();
			CardRegionChanges.Clear();
			if (allCardViews.Count == 0)
			{
				CleanupArrowsAndCounters();
				return;
			}
			_unattachedCardsCache.Clear();
			_unattachedCardsCache.AddRange(allCardViews);
			_attachedCardsByParentCache.Clear();
			foreach (DuelScene_CDC allCardView in allCardViews)
			{
				IEnumerable<MtgCardInstance> otherInstances = CardViewAndLimboInstances(allCardViews, _gameStateProvider.CurrentGameState, _browserProvider.CurrentBrowser);
				MtgCardInstance mtgCardInstance = FindStackParentCardInstance(allCardView.Model.Instance, otherInstances);
				if (mtgCardInstance != null)
				{
					_unattachedCardsCache.Remove(allCardView);
					if (_attachedCardsByParentCache.TryGetValue(mtgCardInstance.InstanceId, out var value))
					{
						value.Add(allCardView);
						continue;
					}
					_attachedCardsByParentCache[mtgCardInstance.InstanceId] = new List<DuelScene_CDC> { allCardView };
				}
			}
			_stacksCache.Clear();
			foreach (DuelScene_CDC item in _unattachedCardsCache)
			{
				uint instanceId = item.Model.InstanceId;
				if (_attachedCardsByParentCache.TryGetValue(instanceId, out var value2))
				{
					value2.Sort((DuelScene_CDC lhs, DuelScene_CDC rhs) => rhs.Model.Instance.AttachedToId.CompareTo(lhs.Model.Instance.AttachedToId));
					_stacksCache.Add(_stackFactory.Create(item, value2));
					continue;
				}
				if (item.Model.Instance.AttachedWithIds.Count != 0)
				{
					_stacksCache.Add(_stackFactory.Create(item, null));
					continue;
				}
				BattlefieldStack battlefieldStack = null;
				foreach (BattlefieldStack item2 in _stacksCache)
				{
					if (item2.CanStack(item))
					{
						battlefieldStack = item2;
						break;
					}
				}
				if (battlefieldStack == null || (_npeDirector != null && _npeDirector.AllowsStackingOnBattlefield && item.Model.CardTypes.Contains(CardType.Creature)))
				{
					_stacksCache.Add(_stackFactory.Create(item, null));
				}
				else
				{
					battlefieldStack.PushCardOnStack(item);
				}
			}
			foreach (KeyValuePair<uint, List<DuelScene_CDC>> item3 in _attachedCardsByParentCache)
			{
				BattlefieldStack battlefieldStack2 = null;
				foreach (BattlefieldStack item4 in _stacksCache)
				{
					if (item4.StackParentModel.InstanceId == item3.Key)
					{
						battlefieldStack2 = item4;
						break;
					}
				}
				if (battlefieldStack2 == null)
				{
					IEnumerable<MtgCardInstance> otherInstances2 = CardViewAndLimboInstances(allCardViews, _gameStateProvider.CurrentGameState, _browserProvider.CurrentBrowser);
					MtgCardInstance mtgCardInstance2 = FindStackParentCardInstance(item3.Value[0].Model.Instance, otherInstances2);
					if (mtgCardInstance2 != null)
					{
						_stacksCache.Add(_stackFactory.Create(CardDataExtensions.CreateWithDatabase(mtgCardInstance2, _cardDatabase), item3.Value));
					}
				}
			}
			long num = 0L;
			foreach (BattlefieldStack item5 in _stacksCache)
			{
				if (item5.StackParent != null)
				{
					num++;
				}
				if (item5.StackedCards.Count > 0)
				{
					num += item5.StackedCards.Count;
				}
			}
			if (num < allCardViews.Count)
			{
				throw new UnityException($"[BattlefieldLayout] The total number of stacked cards ({num}) does not match the number of cardViews provided ({allCardViews.Count}).");
			}
			foreach (BattlefieldStack item6 in _stacksCache)
			{
				BattlefieldRegion battlefieldRegion2 = null;
				battlefieldRegion2 = ((item6.StackParentModel.CardTypes.Contains(CardType.Battle) && TryGetProtector(item6.StackParentModel, _gameStateProvider.CurrentGameState.Value.Players, out var protector)) ? (protector switch
				{
					GREPlayerNum.Opponent => _opponentPlaneswalkerRegion, 
					GREPlayerNum.LocalPlayer => _localPlaneswalkerRegion, 
					_ => null, 
				}) : (item6.StackParentModel.CardTypes.Contains(CardType.Creature) ? ((item6.StackParentModel.ControllerNum == GREPlayerNum.LocalPlayer) ? _localCreatureRegion : ((item6.StackParentModel.ControllerNum == GREPlayerNum.Opponent) ? _opponentCreatureRegion : null)) : ((item6.StackParentModel.CardTypes.Contains(CardType.Planeswalker) || item6.StackParentModel.Subtypes.Contains(SubType.Saga) || item6.StackParentModel.Subtypes.Contains(SubType.Class) || item6.StackParentModel.Subtypes.Contains(SubType.Case) || item6.StackParentModel.Subtypes.Contains(SubType.Room)) ? ((item6.StackParentModel.ControllerNum == GREPlayerNum.LocalPlayer) ? _localPlaneswalkerRegion : ((item6.StackParentModel.ControllerNum == GREPlayerNum.Opponent) ? _opponentPlaneswalkerRegion : null)) : ((!item6.StackParentModel.CardTypes.Contains(CardType.Land)) ? ((item6.StackParentModel.ControllerNum == GREPlayerNum.LocalPlayer) ? _localArtifactRegion : ((item6.StackParentModel.ControllerNum == GREPlayerNum.Opponent) ? _opponentArtifactRegion : null)) : ((item6.StackParentModel.ControllerNum == GREPlayerNum.LocalPlayer) ? _localLandRegion : ((item6.StackParentModel.ControllerNum == GREPlayerNum.Opponent) ? _opponentLandRegion : null))))));
				if (battlefieldRegion2 != null)
				{
					battlefieldRegion2.Stacks.Add(item6);
					HashSet<DuelScene_CDC> hashSet = dictionary[battlefieldRegion2];
					foreach (DuelScene_CDC allCard in item6.AllCards)
					{
						if (hashSet.Contains(allCard))
						{
							continue;
						}
						BattlefieldRegion battlefieldRegion3 = null;
						foreach (KeyValuePair<BattlefieldRegion, HashSet<DuelScene_CDC>> item7 in dictionary)
						{
							if (item7.Value.Contains(allCard))
							{
								battlefieldRegion3 = item7.Key;
								break;
							}
						}
						if (battlefieldRegion3 != null)
						{
							CardRegionChanges[allCard] = new KeyValuePair<BattlefieldRegion, BattlefieldRegion>(battlefieldRegion3, battlefieldRegion2);
						}
					}
				}
				else
				{
					_ = item6.StackParentModel.InstanceId;
				}
			}
			BattlefieldRegion currentlyHoveredRegion = _battlefield.GetCurrentlyHoveredRegion();
			SortStacksByAge(_localPlaneswalkerRegion.Stacks, _titleIdToOldestStackAgeLocalPlaneswalkers);
			SortStacksByAge(_localArtifactRegion.Stacks, _titleIdToOldestStackAgeLocalArtifacts);
			SortStacksByAge(_localLandRegion.Stacks, _titleIdToOldestStackAgeLocalLands);
			GenerateRegionData(_localPlaneswalkerRegion, allData, _localPlaneswalkerRegion != currentlyHoveredRegion);
			GenerateRegionData(_localArtifactRegion, allData, _localArtifactRegion != currentlyHoveredRegion);
			GenerateRegionData(_localLandRegion, allData, _localLandRegion != currentlyHoveredRegion);
			SortStacksByAge(_opponentPlaneswalkerRegion.Stacks, _titleIdToOldestStackAgeOpponentPlaneswalkers);
			SortStacksByAge(_opponentArtifactRegion.Stacks, _titleIdToOldestStackAgeOpponentArtifacts);
			SortStacksByAge(_opponentLandRegion.Stacks, _titleIdToOldestStackAgeOpponentLands);
			GenerateRegionData(_opponentPlaneswalkerRegion, allData, _opponentPlaneswalkerRegion != currentlyHoveredRegion);
			GenerateRegionData(_opponentArtifactRegion, allData, _opponentArtifactRegion != currentlyHoveredRegion);
			GenerateRegionData(_opponentLandRegion, allData, _opponentLandRegion != currentlyHoveredRegion);
			if (_gameStateProvider.CurrentGameState.Value.CurrentPhase == Phase.Combat)
			{
				BattlefieldRegion battlefieldRegion4 = (_gameStateProvider.CurrentGameState.Value.ActivePlayer.IsLocalPlayer ? _localCreatureRegion : _opponentCreatureRegion);
				BattlefieldRegion battlefieldRegion5 = (_gameStateProvider.CurrentGameState.Value.ActivePlayer.IsLocalPlayer ? _opponentCreatureRegion : _localCreatureRegion);
				Dictionary<uint, uint> titleIdToStackAge = (_gameStateProvider.CurrentGameState.Value.ActivePlayer.IsLocalPlayer ? _titleIdToOldestStackAgeLocalCreatures : _titleIdToOldestStackAgeOpponentCreatures);
				Dictionary<uint, uint> blockerTitleIdToStackAge = (_gameStateProvider.CurrentGameState.Value.ActivePlayer.IsLocalPlayer ? _titleIdToOldestStackAgeOpponentCreatures : _titleIdToOldestStackAgeLocalCreatures);
				SortStacksByAge(battlefieldRegion4.Stacks, titleIdToStackAge);
				GenerateRegionData(battlefieldRegion4, allData, battlefieldRegion4 != currentlyHoveredRegion);
				SortStacksByBlocking(allData, battlefieldRegion4.Stacks, battlefieldRegion5.Stacks, blockerTitleIdToStackAge);
				GenerateRegionData(battlefieldRegion5, allData, battlefieldRegion5 != currentlyHoveredRegion);
			}
			else
			{
				SortStacksByAge(_localCreatureRegion.Stacks, _titleIdToOldestStackAgeLocalCreatures);
				SortStacksByAge(_opponentCreatureRegion.Stacks, _titleIdToOldestStackAgeOpponentCreatures);
				GenerateRegionData(_localCreatureRegion, allData, _localCreatureRegion != currentlyHoveredRegion);
				GenerateRegionData(_opponentCreatureRegion, allData, _opponentCreatureRegion != currentlyHoveredRegion);
			}
			while (_pendingStackCounters.Count < ActiveStackCounters.Count)
			{
				CdcStackCounterView cdcStackCounterView = ActiveStackCounters[0];
				ActiveStackCounters.RemoveAt(0);
				cdcStackCounterView.Cleanup();
				_unityPool.PushObject(cdcStackCounterView.gameObject);
			}
			while (ActiveStackCounters.Count < _pendingStackCounters.Count)
			{
				CdcStackCounterView component = _unityPool.PopObject(_battlefield.CounterPrefab.gameObject).GetComponent<CdcStackCounterView>();
				Transform transform = component.transform;
				transform.parent = _battlefield.transform;
				transform.localRotation = Quaternion.identity;
				ActiveStackCounters.Add(component);
			}
			for (int num2 = 0; num2 < _pendingStackCounters.Count; num2++)
			{
				StackCounterData stackCounterData = _pendingStackCounters[num2];
				CdcStackCounterView cdcStackCounterView2 = ActiveStackCounters[num2];
				cdcStackCounterView2.transform.position = stackCounterData.positon;
				cdcStackCounterView2.SetCount(stackCounterData.count);
				cdcStackCounterView2.Init(_interactionSystem, stackCounterData.parentInstanceId);
			}
			for (int num3 = 0; num3 < _takenRects.Count; num3++)
			{
				DebugDraw.Square(_takenRects[num3], UnityEngine.Color.Lerp(UnityEngine.Color.red, UnityEngine.Color.blue, (float)num3 / (float)_takenRects.Count), 2.5f);
			}
			_logger?.UpdateMaxCreatures(_localCreatureRegion.Stacks.Sum((BattlefieldStack stack) => stack.CardCount));
			_logger?.UpdateMaxLands(_localLandRegion.Stacks.Sum((BattlefieldStack stack) => stack.CardCount));
			_logger?.UpdateMaxArtifactsAndEnchantments(_localArtifactRegion.Stacks.Sum((BattlefieldStack stack) => stack.CardCount));
		}

		protected static bool TryGetProtector(ICardDataAdapter card, IReadOnlyCollection<MtgPlayer> players, out GREPlayerNum protector)
		{
			foreach (MtgPlayer player in players)
			{
				foreach (DesignationData designation in player.Designations)
				{
					if (designation.Type == Designation.Protector && designation.AffectorId == card.InstanceId)
					{
						protector = player.ClientPlayerEnum;
						return true;
					}
				}
			}
			protector = GREPlayerNum.Invalid;
			return false;
		}

		protected static IEnumerable<MtgCardInstance> CardViewAndLimboInstances(IEnumerable<DuelScene_CDC> cardViews, MtgGameState gameState, BrowserBase currentBrowser)
		{
			if (cardViews != null)
			{
				foreach (DuelScene_CDC cardView in cardViews)
				{
					if (cardView == null)
					{
						continue;
					}
					ICardDataAdapter model = cardView.Model;
					if (model != null)
					{
						MtgCardInstance instance = model.Instance;
						if (instance != null)
						{
							yield return instance;
						}
					}
				}
			}
			if (gameState != null)
			{
				MtgZone limbo = gameState.Limbo;
				if (limbo != null)
				{
					foreach (MtgCardInstance visibleCard in limbo.VisibleCards)
					{
						yield return visibleCard;
					}
				}
			}
			if (!(currentBrowser is CardBrowserBase cardBrowserBase))
			{
				yield break;
			}
			foreach (DuelScene_CDC cardView2 in cardBrowserBase.GetCardViews())
			{
				if ((bool)cardView2)
				{
					ICardDataAdapter model2 = cardView2.Model;
					if (model2 != null && model2.ZoneType == ZoneType.Battlefield)
					{
						yield return cardView2.Model.Instance;
					}
				}
			}
		}

		public static MtgCardInstance FindStackParentCardInstance(MtgCardInstance cardInstance, IEnumerable<MtgCardInstance> otherInstances)
		{
			if (cardInstance == null || cardInstance.AttachedToId == 0)
			{
				return null;
			}
			MtgCardInstance mtgCardInstance = cardInstance;
			_attachmentChain.Clear();
			_attachmentChain.Add(cardInstance);
			MtgCardInstance attachedInstance;
			while (TryGetAttachedInstance(mtgCardInstance, otherInstances, out attachedInstance) && _attachmentChain.Add(attachedInstance))
			{
				mtgCardInstance = attachedInstance;
			}
			_attachmentChain.Clear();
			if (mtgCardInstance == cardInstance)
			{
				return null;
			}
			return mtgCardInstance;
		}

		private static bool TryGetAttachedInstance(MtgCardInstance cardInstance, IEnumerable<MtgCardInstance> allInstances, out MtgCardInstance attachedInstance)
		{
			foreach (MtgCardInstance item in allInstances ?? Array.Empty<MtgCardInstance>())
			{
				if (cardInstance.AttachedToId == item.InstanceId)
				{
					attachedInstance = item;
					return true;
				}
			}
			attachedInstance = null;
			return false;
		}

		private void OnUxEventCommenced(UXEvent uxEvent)
		{
			if (!(uxEvent is ZoneTransferGroup zoneTransferGroup))
			{
				if (!(uxEvent is ZoneTransferUXEvent zoneTransferUxEvent))
				{
					if (uxEvent is UpdateCardModelUXEvent { Property: PropertyType.TitleId } updateCardModelUXEvent && updateCardModelUXEvent.NewInstance.Zone.Type == ZoneType.Battlefield)
					{
						tryUpdateTitleId(getRelevantTitleIdToStackAge(updateCardModelUXEvent.NewInstance), updateCardModelUXEvent.OldInstance.TitleId, updateCardModelUXEvent.NewInstance.TitleId);
					}
				}
				else
				{
					tryUpdateStackAgeIfZoneTransferToBattlefield(zoneTransferUxEvent);
				}
				return;
			}
			foreach (ZoneTransferUXEvent zoneTransfer in zoneTransferGroup._zoneTransfers)
			{
				tryUpdateStackAgeIfZoneTransferToBattlefield(zoneTransfer);
			}
			Dictionary<uint, uint> getRelevantTitleIdToStackAge(MtgCardInstance card)
			{
				if (card.CardTypes.Contains(CardType.Creature))
				{
					if (!card.Controller.IsLocalPlayer)
					{
						return _titleIdToOldestStackAgeOpponentCreatures;
					}
					return _titleIdToOldestStackAgeLocalCreatures;
				}
				if (card.CardTypes.Contains(CardType.Planeswalker) || card.Subtypes.Contains(SubType.Saga) || card.Subtypes.Contains(SubType.Class) || card.Subtypes.Contains(SubType.Case) || card.Subtypes.Contains(SubType.Room))
				{
					if (!card.Controller.IsLocalPlayer)
					{
						return _titleIdToOldestStackAgeOpponentPlaneswalkers;
					}
					return _titleIdToOldestStackAgeLocalPlaneswalkers;
				}
				if (card.CardTypes.Contains(CardType.Land))
				{
					if (!card.Controller.IsLocalPlayer)
					{
						return _titleIdToOldestStackAgeOpponentLands;
					}
					return _titleIdToOldestStackAgeLocalLands;
				}
				if (!card.Controller.IsLocalPlayer)
				{
					return _titleIdToOldestStackAgeOpponentArtifacts;
				}
				return _titleIdToOldestStackAgeLocalArtifacts;
			}
			static void tryUpdateStackAge(Dictionary<uint, uint> titleIdToStackAge, uint oldId, uint newId)
			{
				foreach (KeyValuePair<uint, uint> item in titleIdToStackAge)
				{
					if (item.Value == oldId)
					{
						titleIdToStackAge[item.Key] = newId;
						break;
					}
				}
			}
			void tryUpdateStackAgeIfZoneTransferToBattlefield(ZoneTransferUXEvent zoneTransferUXEvent)
			{
				if (zoneTransferUXEvent.ToZoneType == ZoneType.Battlefield)
				{
					uint oldId = zoneTransferUXEvent.OldId;
					uint newId = zoneTransferUXEvent.NewId;
					tryUpdateStackAge(getRelevantTitleIdToStackAge(zoneTransferUXEvent.NewInstance), oldId, newId);
				}
			}
			static void tryUpdateTitleId(Dictionary<uint, uint> titleIdToStackAge, uint oldTitleId, uint newTitleId)
			{
				if (titleIdToStackAge.ContainsKey(oldTitleId))
				{
					if (!titleIdToStackAge.ContainsKey(newTitleId))
					{
						titleIdToStackAge.Add(newTitleId, titleIdToStackAge[oldTitleId]);
					}
					else
					{
						titleIdToStackAge[newTitleId] = titleIdToStackAge[oldTitleId];
					}
				}
			}
		}

		private void ComputeAgesForStacks(List<BattlefieldStack> stacks, Dictionary<uint, uint> titleIdToStackAge)
		{
			_titleIdsToRemoveCache.Clear();
			foreach (uint key in titleIdToStackAge.Keys)
			{
				bool flag = false;
				foreach (BattlefieldStack stack in stacks)
				{
					if (stack.StackParentModel.TitleId == key)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_titleIdsToRemoveCache.Add(key);
				}
			}
			foreach (uint item in _titleIdsToRemoveCache)
			{
				titleIdToStackAge.Remove(item);
			}
			foreach (BattlefieldStack stack2 in stacks)
			{
				uint titleId = stack2.StackParentModel.TitleId;
				if (!titleIdToStackAge.ContainsKey(titleId))
				{
					titleIdToStackAge.Add(titleId, uint.MaxValue);
				}
				titleIdToStackAge[titleId] = Math.Min(titleIdToStackAge[titleId], stack2.Age);
			}
		}

		protected void SortStacksByAge(List<BattlefieldStack> stacks, Dictionary<uint, uint> titleIdToStackAge)
		{
			ComputeAgesForStacks(stacks, titleIdToStackAge);
			if (_stackAgeComparer == null)
			{
				_stackAgeComparer = new StackAgeComparer(titleIdToStackAge);
			}
			else
			{
				_stackAgeComparer.Reinitialize(titleIdToStackAge);
			}
			stacks.Sort(_stackAgeComparer);
		}

		protected void SortStacksByBlocking(List<CardLayoutData> layoutDatas, List<BattlefieldStack> attackerStacks, List<BattlefieldStack> blockerStacks, Dictionary<uint, uint> blockerTitleIdToStackAge)
		{
			ComputeAgesForStacks(blockerStacks, blockerTitleIdToStackAge);
			if (_stackBlockerAndAgeComparer == null)
			{
				_stackBlockerAndAgeComparer = new StackBlockerAndAgeComparer(layoutDatas, attackerStacks, blockerTitleIdToStackAge);
			}
			else
			{
				_stackBlockerAndAgeComparer.Reinitialize(layoutDatas, attackerStacks, blockerTitleIdToStackAge);
			}
			blockerStacks.Sort(_stackBlockerAndAgeComparer);
		}

		protected void GenerateRegionData(BattlefieldRegion region, List<CardLayoutData> layoutData, bool calcRegionVariant)
		{
			float y = _battlefield.transform.position.y;
			region.GenerateData(layoutData, (region.Opponent ? 1f : (-1f)) * _battlefield.DeclaredAttackOffset, _battlefield.TapRotation, _pendingStackCounters, y, _takenRects, calcRegionVariant);
			if (region.LayoutVariant != null && region.LayoutVariant.UsesPaging && region.Stacks.Count > 0 && (region.PagedOutLeftCount > 0 || region.PagedOutRightCount > 0))
			{
				if (region.LeftPagingButton == null)
				{
					region.LeftPagingButton = _unityPool.PopObject(_battlefield.LeftPagingButton.gameObject).GetComponent<RegionPagingButton>();
					region.LeftPagingButton.transform.SetParent(_battlefield.transform);
				}
				region.LeftPagingButton.SetCount(region.PagedOutLeftCount);
				region.LeftPagingButton.SetPosition(region.LeftArrowButtonRect, y);
				region.LeftPagingButton.SetCallback(delegate
				{
					region.PageLeft();
					_battlefield.LayoutNow();
				});
				if (region.RightPagingButton == null)
				{
					region.RightPagingButton = _unityPool.PopObject(_battlefield.RightPagingButton.gameObject).GetComponent<RegionPagingButton>();
					region.RightPagingButton.transform.SetParent(_battlefield.transform);
				}
				region.RightPagingButton.SetCount(region.PagedOutRightCount);
				region.RightPagingButton.SetPosition(region.RightArrowButtonRect, y);
				region.RightPagingButton.SetCallback(delegate
				{
					region.PageRight();
					_battlefield.LayoutNow();
				});
			}
			else
			{
				if (region.LeftPagingButton != null)
				{
					_unityPool.PushObject(region.LeftPagingButton.gameObject);
					region.LeftPagingButton = null;
				}
				if (region.RightPagingButton != null)
				{
					_unityPool.PushObject(region.RightPagingButton.gameObject);
					region.RightPagingButton = null;
				}
			}
		}

		protected void CleanupArrowsAndCounters()
		{
			BattlefieldRegion[] regions = Regions;
			foreach (BattlefieldRegion battlefieldRegion in regions)
			{
				if (battlefieldRegion.LeftPagingButton != null)
				{
					_unityPool.PushObject(battlefieldRegion.LeftPagingButton.gameObject);
					battlefieldRegion.LeftPagingButton = null;
				}
				if (battlefieldRegion.RightPagingButton != null)
				{
					_unityPool.PushObject(battlefieldRegion.RightPagingButton.gameObject);
					battlefieldRegion.RightPagingButton = null;
				}
			}
			while (ActiveStackCounters.Count > 0)
			{
				CdcStackCounterView cdcStackCounterView = ActiveStackCounters[0];
				ActiveStackCounters.RemoveAt(0);
				cdcStackCounterView.Cleanup();
				_unityPool.PushObject(cdcStackCounterView.gameObject);
			}
		}
	}

	public class BattlefieldStack : IBattlefieldStack
	{
		private Vector3 _cardSize = new Vector3(4.17f, 3.5f, 0.26f);

		private readonly float Radius = 8f;

		private readonly float FitAngle = 30f;

		private readonly float MaxDeltaAngle = 7f;

		private readonly float YOffset = 1f;

		private readonly float ZOffset = 3f;

		private readonly float DistanceBetweenCardsModifier = 0.4f;

		private readonly float OverlapRotation = 2.5f;

		private readonly float RotationMultiplier = 1f;

		private readonly int FocusProximityWeight = 10;

		private readonly int VISUALCARDS = 4;

		private readonly ICardDatabaseAdapter _cardDatabase;

		private readonly IGameStateProvider _gameStateProvider;

		private readonly IWorkflowProvider _workflowProvider;

		private readonly ICardViewProvider _cardViewProvider;

		private readonly IEqualityComparer<DuelScene_CDC> _canStackComparer;

		private readonly List<DuelScene_CDC> _tmpCardList = new List<DuelScene_CDC>(10);

		private ActionsAvailableWorkflow _tmpActionsAvail;

		public List<DuelScene_CDC> AllCards { get; private set; } = new List<DuelScene_CDC>();

		public DuelScene_CDC StackParent { get; private set; }

		public List<DuelScene_CDC> StackedCards { get; private set; } = new List<DuelScene_CDC>();

		public ICardDataAdapter StackParentModel { get; private set; }

		public int CardCount => AllCards.Count;

		public int AttachmentCount { get; private set; }

		public int ExileCount { get; private set; }

		public bool HasAttachmentOrExile
		{
			get
			{
				if (AttachmentCount <= 0)
				{
					return ExileCount > 0;
				}
				return true;
			}
		}

		public bool IsAttackStack
		{
			get
			{
				if (StackParentModel.Instance.AttackState != AttackState.Declared)
				{
					return StackParentModel.Instance.AttackState == AttackState.Attacking;
				}
				return true;
			}
		}

		public bool IsBlockStack
		{
			get
			{
				if (StackParentModel.Instance.BlockState != BlockState.Declared)
				{
					return StackParentModel.Instance.BlockState == BlockState.Blocking;
				}
				return true;
			}
		}

		private Vector3 _center => StackParent.transform.TransformDirection(Vector3.back) * 4f;

		public float GetArcBaseY => Mathf.Sin(GetLeftmostAngle * (MathF.PI / 180f)) * Radius;

		public float GetLeftmostAngle => 90f + FitAngle * 0.5f;

		public float GetRightmostAngle => 90f - FitAngle * 0.5f;

		public uint Age
		{
			get
			{
				uint result = uint.MaxValue;
				if (0 < AttachmentCount || 0 < ExileCount)
				{
					result = StackParentModel.InstanceId;
				}
				else
				{
					DuelScene_CDC oldestCard = OldestCard;
					if ((object)oldestCard != null)
					{
						result = oldestCard.InstanceId;
					}
				}
				return result;
			}
		}

		public DuelScene_CDC OldestCard
		{
			get
			{
				DuelScene_CDC duelScene_CDC = null;
				foreach (DuelScene_CDC allCard in AllCards)
				{
					if (allCard.Model.ZoneType == ZoneType.Battlefield && (duelScene_CDC == null || duelScene_CDC.InstanceId > allCard.InstanceId))
					{
						duelScene_CDC = allCard;
					}
				}
				return duelScene_CDC;
			}
		}

		public DuelScene_CDC YoungestCard
		{
			get
			{
				DuelScene_CDC duelScene_CDC = null;
				foreach (DuelScene_CDC allCard in AllCards)
				{
					if (allCard.Model.ZoneType == ZoneType.Battlefield && (duelScene_CDC == null || duelScene_CDC.InstanceId < allCard.InstanceId))
					{
						duelScene_CDC = allCard;
					}
				}
				return duelScene_CDC;
			}
		}

		private void CalcPosRotFromAngle(float angle, out Vector3 pos, out Vector3 rot)
		{
			pos = new Vector3
			{
				x = 0f - Mathf.Cos(angle) * Radius,
				y = YOffset,
				z = 0f - (Mathf.Sin(angle) * Radius - GetArcBaseY) + ZOffset
			};
			rot = new Vector3
			{
				x = 0f,
				y = OverlapRotation,
				z = Vector3.SignedAngle(Vector3.up, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)), Vector3.forward) * RotationMultiplier
			};
		}

		public BattlefieldStack(DuelScene_CDC parent, List<DuelScene_CDC> attachmentsAndExiles, ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider, ICardViewProvider cardViewProvider, IEqualityComparer<DuelScene_CDC> canStackComparer)
		{
			_cardDatabase = cardDatabase;
			_gameStateProvider = gameStateProvider;
			_workflowProvider = workflowProvider;
			_cardViewProvider = cardViewProvider;
			_canStackComparer = canStackComparer;
			StackParent = parent;
			StackParentModel = parent.Model;
			if (attachmentsAndExiles != null)
			{
				StackedCards.AddRange(attachmentsAndExiles);
			}
			if (StackParent != null)
			{
				AllCards.Add(StackParent);
			}
			AllCards.AddRange(StackedCards);
			if (StackParent.IsDirty)
			{
				StackParent.ImmediateUpdate();
			}
			_cardSize = StackParent.Collider.size;
			InitializeAttachmentAndExileCounts();
		}

		public BattlefieldStack(ICardDataAdapter parentModel, List<DuelScene_CDC> attachmentsAndExiles, ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider, ICardViewProvider cardViewProvider, IEqualityComparer<DuelScene_CDC> canStackComparer)
		{
			_cardDatabase = cardDatabase;
			_gameStateProvider = gameStateProvider;
			_workflowProvider = workflowProvider;
			_cardViewProvider = cardViewProvider;
			_canStackComparer = canStackComparer;
			StackParent = null;
			StackParentModel = parentModel;
			if (attachmentsAndExiles != null)
			{
				StackedCards.AddRange(attachmentsAndExiles);
				_cardSize = attachmentsAndExiles[0].Collider.size;
			}
			if (StackParent != null)
			{
				AllCards.Add(StackParent);
			}
			AllCards.AddRange(StackedCards);
			InitializeAttachmentAndExileCounts();
		}

		public void PushCardOnStack(DuelScene_CDC card)
		{
			if (!(card == null))
			{
				if (StackParent != null)
				{
					StackedCards.Insert(0, StackParent);
				}
				StackParent = card;
				StackParentModel = card.Model;
				AllCards.Insert(0, card);
			}
		}

		public bool CanStack(DuelScene_CDC card)
		{
			if (AttachmentCount > 0 || ExileCount > 0)
			{
				return false;
			}
			return _canStackComparer.Equals(StackParent, card);
		}

		private void Sort()
		{
			ActionsAvailableWorkflow result;
			if (AttachmentCount > 0 || ExileCount > 0)
			{
				_tmpCardList.Clear();
				DuelScene_CDC stackParent = StackParent;
				ICardDataAdapter stackParentModel = StackParentModel;
				MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
				addCards(AttachmentAndExileStackGroupData.GenerateGroups(mtgGameState.GetCardById(stackParentModel.InstanceId), mtgGameState, _cardViewProvider, setParent: false), _tmpCardList);
				if ((bool)stackParent && !_tmpCardList.Contains(stackParent))
				{
					_tmpCardList.Insert(0, stackParent);
				}
				if (_tmpCardList.Count > 0)
				{
					AllCards.Sort((DuelScene_CDC x, DuelScene_CDC y) => _tmpCardList.IndexOf(x).CompareTo(_tmpCardList.IndexOf(y)));
					_tmpCardList.Clear();
				}
			}
			else if (IsBlockStack)
			{
				AllCards.Sort(CompareDistinguishedObjects);
			}
			else if (TryGetActionsAvailableWorkflow(_workflowProvider.GetCurrentWorkflow(), out result))
			{
				_tmpActionsAvail = result;
				AllCards.Sort(CompareAvailableActions);
			}
			else
			{
				AllCards.Sort(CompareDistinguishedObjects);
			}
			if (AttachmentCount <= 0 && ExileCount <= 0)
			{
				StackParent = AllCards[0];
				StackParentModel = StackParent.Model;
			}
			StackedCards.Clear();
			StackedCards.AddRange(AllCards);
			StackedCards.Remove(StackParent);
			static void addCards(List<AttachmentAndExileStackGroupData> groupList, List<DuelScene_CDC> cards)
			{
				foreach (AttachmentAndExileStackGroupData group in groupList)
				{
					cards.AddRange(group.Cards);
					if (group.Children != null)
					{
						addCards(group.Children, cards);
					}
				}
			}
		}

		public float CalcWidth(float cardScale, float maxWidth, float minWidth)
		{
			float num = _cardSize.x * cardScale;
			if (CardCount != 1)
			{
				return num * maxWidth;
			}
			return num * minWidth;
		}

		public float CalcHeight(float cardScale)
		{
			return _cardSize.y * cardScale;
		}

		public Vector3 GetCopyCounterPosition(float cardScale, float maxWidth, float minWidth, int maxStacked)
		{
			return GetCounterPosition(cardScale, maxWidth, minWidth, maxStacked);
		}

		public Vector3 GetAttachmentCounterPosition(float cardScale, float maxWidth, float minWidth, int maxStacked)
		{
			return GetCounterPosition(cardScale, maxWidth, minWidth, maxStacked);
		}

		public Vector3 GetExileCounterPosition(float cardScale, float maxWidth, float minWidth, int maxStacked)
		{
			return GetCounterPosition(cardScale, maxWidth, minWidth, maxStacked, isExileCounter: true);
		}

		private Vector3 GetCounterPosition(float cardScale, float maxWidth, float minWidth, int maxStacked, bool isExileCounter = false)
		{
			float num = CalcWidth(cardScale, maxWidth, minWidth);
			float num2 = CalcHeight(cardScale);
			float num3 = _cardSize.z * cardScale;
			Vector3 result = new Vector3(num * 0.5f - 0.5f, ((float)Mathf.Min(CardCount, maxStacked) + 4f) * num3, (0f - num2) * 0.5f);
			switch (StackParentModel.OwnerNum)
			{
			case GREPlayerNum.LocalPlayer:
				result.z += 0.5f;
				break;
			case GREPlayerNum.Opponent:
				result.z += 1f;
				break;
			}
			if (isExileCounter)
			{
				result.z += 1f;
			}
			return result;
		}

		private List<CardLayoutData> GenerateStackFanningData(float cardScale)
		{
			List<CardLayoutData> list = new List<CardLayoutData>();
			int num = AllCards.IndexOf(CardHoverController.HoveredCard);
			int num2 = Math.Min(CardCount, VISUALCARDS);
			float num3 = Mathf.Min(MaxDeltaAngle, FitAngle / (float)(CardCount - 1));
			Dictionary<int, DuelScene_CDC> dictionary = new Dictionary<int, DuelScene_CDC>(CardCount);
			int num4 = 0;
			for (int i = 0; i < CardCount; i++)
			{
				dictionary[i] = AllCards[num4];
				num4++;
			}
			int num5 = 0;
			int num6 = 0;
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>(CardCount);
			for (int j = 0; j < CardCount; j++)
			{
				if (j != num)
				{
					int num7 = Mathf.Max(1, FocusProximityWeight - Mathf.Abs(num - j));
					if (j < num)
					{
						num5 += num7;
					}
					else
					{
						num6 += num7;
					}
					dictionary2[j] = num7;
				}
			}
			float num8 = GetLeftmostAngle - num3 * (float)num;
			float num9 = GetLeftmostAngle - num8;
			float num10 = num8 - GetRightmostAngle;
			float num11 = Mathf.Max(0.1f, num9 / (float)num5);
			float num12 = Mathf.Max(0.1f, num10 / (float)num6);
			Vector3 vector = _cardSize * cardScale;
			Vector3 scale = Vector3.one * cardScale;
			float z = vector.z;
			int num13 = CardCount - num2 / 2;
			float num14 = num8;
			Vector3 pos;
			Vector3 rot;
			for (int k = num + 1; k < CardCount; k++)
			{
				if (CardCount - k <= num13)
				{
					num14 -= num12 * (float)dictionary2[k];
				}
				if (dictionary[k] != null)
				{
					CalcPosRotFromAngle(num14 * (MathF.PI / 180f), out pos, out rot);
					pos += _center;
					pos.y -= (0f - z) * (float)(CardCount - k) * DistanceBetweenCardsModifier;
					list.Add(new CardLayoutData(dictionary[k], pos, Quaternion.Euler(rot), scale, isVisibleInLayout: true));
				}
			}
			num14 = num8;
			if (dictionary[num] != null)
			{
				CalcPosRotFromAngle(num14 * (MathF.PI / 180f), out pos, out rot);
				pos += _center;
				pos.y -= (0f - z) * (float)(CardCount - num) * DistanceBetweenCardsModifier;
				list.Add(new CardLayoutData(dictionary[num], pos + new Vector3(0f, 0f, -1f), Quaternion.identity, scale, isVisibleInLayout: true));
			}
			num14 = num8;
			int num15 = num - 1;
			int num16 = 0;
			while (num15 >= 0)
			{
				if (num16 <= num13)
				{
					num14 += num11 * (float)dictionary2[num15];
				}
				if (dictionary[num15] != null)
				{
					CalcPosRotFromAngle(num14 * (MathF.PI / 180f), out pos, out rot);
					pos += _center;
					pos.y -= (0f - z) * (float)(CardCount - num15) * DistanceBetweenCardsModifier;
					list.Add(new CardLayoutData(dictionary[num15], pos, Quaternion.Euler(rot), scale, isVisibleInLayout: true));
				}
				num15--;
				num16++;
			}
			return list;
		}

		public List<CardLayoutData> GenerateData(float cardScale, float maxWidth, float minWidth, int maxStacked, bool allowStackFans = false)
		{
			WorkflowBase currentWorkflow = _workflowProvider.GetCurrentWorkflow();
			List<CardLayoutData> list = new List<CardLayoutData>();
			Sort();
			if (allowStackFans && AttachmentCount == 0 && ExileCount == 0 && IsBlockStack && AllCards.Count > 1 && AllCards.Contains(CardHoverController.HoveredCard))
			{
				if (currentWorkflow is SelectTargetsWorkflow selectTargetsWorkflow)
				{
					if (selectTargetsWorkflow.IsSelectableId(StackParentModel.InstanceId))
					{
						return GenerateStackFanningData(cardScale);
					}
				}
				else if (currentWorkflow is ActionsAvailableWorkflow actionsAvailableWorkflow && actionsAvailableWorkflow.GetActionsForId(StackParentModel.InstanceId).Count > 0)
				{
					return GenerateStackFanningData(cardScale);
				}
			}
			float num = CalcWidth(cardScale, maxWidth, minWidth);
			Vector3 vector = Vector3.one * cardScale;
			Vector3 vector2 = _cardSize * cardScale;
			int num2 = Mathf.Min(CardCount, maxStacked);
			float num3 = ((num2 > 1) ? ((num - vector2.x) / (float)(num2 - 1)) : 0f);
			Vector3 vector3 = new Vector3((IsBlockStack && AttachmentCount == 0 && ExileCount == 0) ? (num * 0.5f - vector2.x * 0.5f) : (0f - num * 0.5f + vector2.x * 0.5f), vector2.z * (float)(num2 - 1), 0f);
			Vector3 vector4 = new Vector3((IsBlockStack && AttachmentCount == 0 && ExileCount == 0) ? (0f - num3) : num3, 0f - vector2.z, 0f);
			if (StackParent != null)
			{
				list.Add(new CardLayoutData(StackParent, vector3, Quaternion.identity, vector));
				if (num2 > 1)
				{
					float num4 = (StackParent.Collider.size.x - StackedCards[0].Collider.size.x) * cardScale / 2f;
					vector3.x += ((IsBlockStack && AttachmentCount == 0 && ExileCount == 0) ? (0f - num4) : num4);
				}
			}
			else
			{
				num2++;
			}
			int i;
			for (i = 0; i < num2 - 1; i++)
			{
				vector3 += vector4;
				Vector3 pos = vector3;
				Vector3 vector5 = vector;
				if (StackedCards[i].Model.Zone.Type == ZoneType.Exile)
				{
					Vector3 a = new Vector3(0f, 0f, 1.25f);
					float num5 = 0.6f;
					vector5 *= num5;
					float num6 = (vector2.x - vector2.x * num5) * 0.5f;
					pos.x += num6;
					pos += Vector3.Scale(a, vector5);
				}
				list.Add(new CardLayoutData(StackedCards[i], pos, Quaternion.identity, vector5));
			}
			vector3.y = -5f;
			for (; i < StackedCards.Count; i++)
			{
				list.Add(new CardLayoutData(StackedCards[i], vector3, Quaternion.identity, vector, isVisibleInLayout: false));
			}
			return list;
		}

		public void RefreshAbilitiesBasedOnStackPosition()
		{
			float value = 0f;
			if ((bool)StackParent && StackParent.Model != null)
			{
				value = StackParent.UpdateAbilityVisuals<PersistVFX, PersistSfx>(display: true, animate: true, null);
				StackParent.UpdateTopCardRelevantVisuals(display: true);
				StackParent.UpdateCounterVisibility(display: true);
			}
			foreach (DuelScene_CDC stackedCard in StackedCards)
			{
				if ((bool)stackedCard && stackedCard.Model != null)
				{
					stackedCard.UpdateAbilityVisuals<PersistVFX, PersistSfx>(display: false, !HasAttachmentOrExile, value);
					stackedCard.UpdateTopCardRelevantVisuals(display: false);
					stackedCard.UpdateCounterVisibility(AttachmentCount > 0 || ExileCount > 0);
				}
			}
		}

		public override string ToString()
		{
			return string.Format("Stack: {0} {1}\n   Parent: {2}\n      Card: {3}", _cardDatabase.GreLocProvider.GetLocalizedText(StackParentModel.TitleId), (CardCount > 1) ? $"({CardCount} cards)" : string.Empty, (StackParent != null) ? StackParent.name : "NULL", string.Join("\n      Card: ", StackedCards.ConvertAll((DuelScene_CDC x) => x.name).ToArray()));
		}

		private void InitializeAttachmentAndExileCounts()
		{
			int num = 0;
			int num2 = 0;
			foreach (DuelScene_CDC stackedCard in StackedCards)
			{
				if ((object)stackedCard == null || stackedCard.Model?.Instance?.AttachedToId != 0)
				{
					if (stackedCard.Model.Instance.Zone.Type == ZoneType.Exile)
					{
						num2++;
					}
					else
					{
						num++;
					}
				}
			}
			AttachmentCount = num;
			ExileCount = num2;
		}

		private bool TryGetActionsAvailableWorkflow(WorkflowBase workflow, out ActionsAvailableWorkflow result)
		{
			if (workflow is ActionsAvailableWorkflow actionsAvailableWorkflow)
			{
				result = actionsAvailableWorkflow;
				return true;
			}
			if (workflow is IParentWorkflow parentWorkflow)
			{
				foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
				{
					if (TryGetActionsAvailableWorkflow(childWorkflow, out result))
					{
						return true;
					}
				}
			}
			result = null;
			return false;
		}

		private int CompareAge(DuelScene_CDC cardViewA, DuelScene_CDC cardViewB)
		{
			return cardViewA.InstanceId.CompareTo(cardViewB.InstanceId);
		}

		private int CompareAvailableActions(DuelScene_CDC cardViewA, DuelScene_CDC cardViewB)
		{
			int num = 0;
			if (_tmpActionsAvail != null)
			{
				bool flag = _tmpActionsAvail.GetActionsForId(cardViewA.InstanceId).Exists((GreInteraction x) => x.IsActive);
				bool value = _tmpActionsAvail.GetActionsForId(cardViewB.InstanceId).Exists((GreInteraction x) => x.IsActive);
				num = -flag.CompareTo(value);
				_tmpActionsAvail = null;
			}
			if (num == 0)
			{
				return CompareDistinguishedObjects(cardViewA, cardViewB);
			}
			return num;
		}

		private int CompareDistinguishedObjects(DuelScene_CDC cardViewA, DuelScene_CDC cardViewB)
		{
			int num = 0;
			bool value = (cardViewA?.Model?.Instance?.DistinguishedByIds.Count).GetValueOrDefault() > 0;
			num = ((cardViewB?.Model?.Instance?.DistinguishedByIds.Count).GetValueOrDefault() > 0).CompareTo(value);
			if (num == 0)
			{
				num = CompareAge(cardViewA, cardViewB);
			}
			return num;
		}
	}

	public static class DebugDraw
	{
		public static void Square(Rect rect, UnityEngine.Color color, float duration = 0.1f)
		{
			Debug.DrawLine(new Vector3(rect.xMin, 0.1f, rect.yMin), new Vector3(rect.xMin, 0.1f, rect.yMax), color, duration);
			Debug.DrawLine(new Vector3(rect.xMin, 0.1f, rect.yMax), new Vector3(rect.xMax, 0.1f, rect.yMax), color, duration);
			Debug.DrawLine(new Vector3(rect.xMax, 0.1f, rect.yMax), new Vector3(rect.xMax, 0.1f, rect.yMin), color, duration);
			Debug.DrawLine(new Vector3(rect.xMax, 0.1f, rect.yMin), new Vector3(rect.xMin, 0.1f, rect.yMin), color, duration);
		}
	}

	public static class GizmosDraw
	{
		public static void Square(Rect rect, UnityEngine.Color color)
		{
			Gizmos.color = color;
			Gizmos.DrawLine(new Vector3(rect.xMin, 0.1f, rect.yMin), new Vector3(rect.xMin, 0.1f, rect.yMax));
			Gizmos.DrawLine(new Vector3(rect.xMin, 0.1f, rect.yMax), new Vector3(rect.xMax, 0.1f, rect.yMax));
			Gizmos.DrawLine(new Vector3(rect.xMax, 0.1f, rect.yMax), new Vector3(rect.xMax, 0.1f, rect.yMin));
			Gizmos.DrawLine(new Vector3(rect.xMax, 0.1f, rect.yMin), new Vector3(rect.xMin, 0.1f, rect.yMin));
		}
	}

	[Space(10f)]
	[SerializeField]
	private CdcStackCounterView StackCounterPrefab;

	[SerializeField]
	private RegionPagingButton _leftPagingButtonPrefab;

	[SerializeField]
	private RegionPagingButton _rightPagingButtonPrefab;

	public BattlefieldRegionDefinition LocalCreatureRegion = new BattlefieldRegionDefinition();

	public BattlefieldRegionDefinition LocalLandRegion = new BattlefieldRegionDefinition();

	public BattlefieldRegionDefinition LocalArtifactRegion = new BattlefieldRegionDefinition();

	public BattlefieldRegionDefinition LocalPlaneswalkerRegion = new BattlefieldRegionDefinition();

	public BattlefieldRegionDefinition OpponentCreatureRegion = new BattlefieldRegionDefinition();

	public BattlefieldRegionDefinition OpponentLandRegion = new BattlefieldRegionDefinition();

	public BattlefieldRegionDefinition OpponentArtifactRegion = new BattlefieldRegionDefinition();

	public BattlefieldRegionDefinition OpponentPlaneswalkerRegion = new BattlefieldRegionDefinition();

	public Vector3 DeclaredAttackOffset = new Vector3(0f, 0.1f, 0.1f);

	public Vector3 TapRotation = new Vector3(0f, 0f, -8f);

	private Phase _lastActivePhase;

	protected BattlefieldLayout _battlefieldLayout;

	private readonly HashSet<IBattlefieldStack> _handledEtbStacks = new HashSet<IBattlefieldStack>();

	private readonly HashSet<IBattlefieldStack> _stacksToRefreshInLateUpdate = new HashSet<IBattlefieldStack>();

	public CdcStackCounterView CounterPrefab => StackCounterPrefab;

	public RegionPagingButton LeftPagingButton => _leftPagingButtonPrefab;

	public RegionPagingButton RightPagingButton => _rightPagingButtonPrefab;

	public bool LayoutLocked { get; set; }

	public Transform Transform => base.transform;

	public List<CardLayoutData> PreviousLayoutData => _previousLayoutData;

	protected override void LateUpdate()
	{
		if (_stacksToRefreshInLateUpdate.Count > 0)
		{
			foreach (IBattlefieldStack item in _stacksToRefreshInLateUpdate)
			{
				item?.RefreshAbilitiesBasedOnStackPosition();
			}
			_stacksToRefreshInLateUpdate.Clear();
		}
		if (LayoutLocked)
		{
			return;
		}
		base.LateUpdate();
		if (Time.frameCount % 60 != 0)
		{
			return;
		}
		BattlefieldRegion[] regions = _battlefieldLayout.Regions;
		for (int i = 0; i < regions.Length; i++)
		{
			if (regions[i].HasPendingVariantCalculation)
			{
				LayoutNow();
				break;
			}
		}
	}

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		IContext context = gameManager.Context;
		IBattlefieldStackFactory stackFactory = new BattlefieldStackFactory(context, new CanStackComparer(context, gameManager.ReferenceMapAggregate, gameManager.UIManager));
		base.Layout = (_battlefieldLayout = new BattlefieldLayout(this, _gameManager.Logger, stackFactory, context, gameManager.InteractionSystem, gameManager.NpeDirector));
		if (_cardViewBuilder != null)
		{
			_cardViewBuilder.onCardUpdated += OnCardUpdated;
		}
	}

	protected override void OnDestroy()
	{
		_battlefieldLayout.Cleanup();
		if (_cardViewBuilder != null)
		{
			_cardViewBuilder.onCardUpdated -= OnCardUpdated;
		}
		base.OnDestroy();
	}

	protected override void HandleAddedCard(DuelScene_CDC cardView)
	{
		foreach (CDCPart value in cardView.ActiveParts.Values)
		{
			value.OnPhaseUpdate(_lastActivePhase);
		}
		base.HandleAddedCard(cardView);
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		if (_cardViews.Contains(cardView))
		{
			BattlefieldRegion[] regions = _battlefieldLayout.Regions;
			for (int i = 0; i < regions.Length; i++)
			{
				regions[i].RemoveCard(cardView);
			}
			base.RemoveCard(cardView);
			cardView.UpdateAbilityVisuals<PersistVFX, PersistSfx>(display: false, animate: false, null);
			cardView.UpdateTopCardRelevantVisuals(display: true);
			cardView.UpdateCounterVisibility(display: true);
		}
	}

	public void UpdateForPhase(Phase phase, Step step)
	{
		if (_lastActivePhase == phase)
		{
			return;
		}
		_lastActivePhase = phase;
		if (_battlefieldLayout == null || _battlefieldLayout.Regions == null)
		{
			return;
		}
		BattlefieldRegion[] regions = _battlefieldLayout.Regions;
		foreach (BattlefieldRegion battlefieldRegion in regions)
		{
			if (battlefieldRegion.Stacks == null)
			{
				continue;
			}
			foreach (BattlefieldStack stack in battlefieldRegion.Stacks)
			{
				stack.RefreshAbilitiesBasedOnStackPosition();
				if (stack.AllCards == null)
				{
					continue;
				}
				foreach (DuelScene_CDC allCard in stack.AllCards)
				{
					if (allCard == null || allCard.ActiveParts == null)
					{
						continue;
					}
					foreach (CDCPart value in allCard.ActiveParts.Values)
					{
						value.OnPhaseUpdate(phase);
					}
				}
			}
		}
	}

	public IBattlefieldStack GetStackForInstanceId(uint id)
	{
		BattlefieldRegion[] regions = _battlefieldLayout.Regions;
		for (int i = 0; i < regions.Length; i++)
		{
			foreach (BattlefieldStack stack in regions[i].Stacks)
			{
				foreach (DuelScene_CDC allCard in stack.AllCards)
				{
					if (allCard.InstanceId == id)
					{
						return stack;
					}
				}
			}
		}
		return null;
	}

	public IBattlefieldStack GetStackForCard(DuelScene_CDC card)
	{
		if (card == null)
		{
			return null;
		}
		BattlefieldRegion[] regions = _battlefieldLayout.Regions;
		for (int i = 0; i < regions.Length; i++)
		{
			foreach (BattlefieldStack stack in regions[i].Stacks)
			{
				if (stack.AllCards.Contains(card))
				{
					return stack;
				}
			}
		}
		return null;
	}

	public BattlefieldRegion GetRegionForCard(DuelScene_CDC card)
	{
		if (card == null)
		{
			return null;
		}
		BattlefieldRegion[] regions = _battlefieldLayout.Regions;
		foreach (BattlefieldRegion battlefieldRegion in regions)
		{
			foreach (BattlefieldStack stack in battlefieldRegion.Stacks)
			{
				if (stack.AllCards.Contains(card))
				{
					return battlefieldRegion;
				}
			}
		}
		return null;
	}

	public Transform GetRegionTransformForCardType(CardType cardType, GREPlayerNum grePlayerNum)
	{
		return GetRegionForCardType(cardType, grePlayerNum)?.RegionLocator;
	}

	private BattlefieldRegion GetRegionForCardType(CardType cardType, GREPlayerNum grePlayerNum)
	{
		if (!_battlefieldLayout.RegionsByCardTypeAndOwner.ContainsKey(cardType))
		{
			return null;
		}
		if (!_battlefieldLayout.RegionsByCardTypeAndOwner[cardType].ContainsKey(grePlayerNum))
		{
			return null;
		}
		return _battlefieldLayout.RegionsByCardTypeAndOwner[cardType][grePlayerNum];
	}

	public BattlefieldRegion GetCurrentlyHoveredRegion()
	{
		Vector2 vector = Input.mousePosition;
		if (Physics.Raycast(CurrentCamera.Value.ScreenPointToRay(vector), out var hitInfo))
		{
			Vector3 point = hitInfo.point;
			if (point.y < 2f)
			{
				Vector2 point2 = new Vector2(point.x, point.z);
				BattlefieldRegion[] regions = _battlefieldLayout.Regions;
				foreach (BattlefieldRegion battlefieldRegion in regions)
				{
					if (battlefieldRegion.LayoutVariant == null || !battlefieldRegion.LayoutVariant.Bounds.Contains(point2))
					{
						continue;
					}
					Collider collider = hitInfo.collider;
					foreach (BattlefieldStack stack in battlefieldRegion.Stacks)
					{
						_ = stack;
						foreach (DuelScene_CDC allCard in battlefieldRegion.AllCards)
						{
							if (collider == allCard.Collider)
							{
								return battlefieldRegion;
							}
						}
					}
				}
			}
		}
		return null;
	}

	public bool CardIsStackParent(DuelScene_CDC card)
	{
		if (card == null)
		{
			return false;
		}
		IBattlefieldStack stackForCard = GetStackForCard(card);
		if (stackForCard == null)
		{
			return false;
		}
		return stackForCard.StackParent == card;
	}

	public void RefreshAbilitiesForCardsStack(DuelScene_CDC card)
	{
		GetStackForCard(card)?.RefreshAbilitiesBasedOnStackPosition();
	}

	protected override void LayoutNowInternal(List<DuelScene_CDC> cardsToLayout, bool layoutInstantly = false)
	{
		_handledEtbStacks.Clear();
		DuelScene_CDC lastAddedCard = _newlyAddedCards.FirstOrDefault();
		base.LayoutNowInternal(cardsToLayout, layoutInstantly);
		if (lastAddedCard != null)
		{
			BattlefieldRegion battlefieldRegion = Array.Find(_battlefieldLayout.Regions, (BattlefieldRegion x) => x.ContainsCardView(lastAddedCard));
			if (battlefieldRegion != null)
			{
				while (!battlefieldRegion.IsInVisibleStack(lastAddedCard))
				{
					if (battlefieldRegion.IsPagedLeft(lastAddedCard))
					{
						battlefieldRegion.PageLeft();
					}
					else if (battlefieldRegion.IsPagedRight(lastAddedCard))
					{
						battlefieldRegion.PageRight();
					}
					base.LayoutNowInternal(cardsToLayout, layoutInstantly);
				}
			}
		}
		BattlefieldRegion[] regions = _battlefieldLayout.Regions;
		for (int num = 0; num < regions.Length; num++)
		{
			foreach (BattlefieldStack stack in regions[num].Stacks)
			{
				stack.RefreshAbilitiesBasedOnStackPosition();
			}
		}
	}

	public override IdealPoint GetLayoutEndpoint(CardLayoutData data)
	{
		return new IdealPoint(data.Position, base.transform.rotation * data.Rotation, data.Scale);
	}

	protected override string GetInternalLayoutSplinePath(CardLayoutData data)
	{
		string text = base.GetInternalLayoutSplinePath(data);
		if (string.IsNullOrEmpty(text) && _battlefieldLayout.CardRegionChanges.ContainsKey(data.Card))
		{
			KeyValuePair<BattlefieldRegion, BattlefieldRegion> keyValuePair = _battlefieldLayout.CardRegionChanges[data.Card];
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(data.Card.Model);
			_assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Battlefield;
			_assetLookupSystem.Blackboard.RegionPair = new RegionPair(keyValuePair.Key.RegionDef.Type, keyValuePair.Key.RegionDef.Owner, keyValuePair.Value.RegionDef.Type, keyValuePair.Value.RegionDef.Owner);
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<InternalMovementPayload_Spline> loadedTree))
			{
				InternalMovementPayload_Spline payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					text = payload.SplineDataRef.RelativePath;
				}
			}
			_assetLookupSystem.Blackboard.Clear();
		}
		return text;
	}

	protected override SplineEventData GetInternalLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData splineEvents = base.GetInternalLayoutSplineEvents(data);
		CardHolderBase.SetupSplineEventsALT<InternalMovementPayload_VFX, InternalMovementPayload_SFX>(data, added: false, ref splineEvents, GetCardPosition(data.Card), CardViewUtilities.GetFromToZoneForCard(data.Card, added: false), _gameManager);
		if (_battlefieldLayout.CardRegionChanges.ContainsKey(data.Card))
		{
			KeyValuePair<BattlefieldRegion, BattlefieldRegion> keyValuePair = _battlefieldLayout.CardRegionChanges[data.Card];
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(data.Card.Model);
			_assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Battlefield;
			_assetLookupSystem.Blackboard.RegionPair = new RegionPair(keyValuePair.Key.RegionDef.Type, keyValuePair.Key.RegionDef.Owner, keyValuePair.Value.RegionDef.Type, keyValuePair.Value.RegionDef.Owner);
			AssetLookupTree<RegionSFX> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<RegionSFX>();
			AssetLookupTree<RegionVFX> assetLookupTree2 = _assetLookupSystem.TreeLoader.LoadTree<RegionVFX>();
			RegionSFX payload = assetLookupTree.GetPayload(_assetLookupSystem.Blackboard);
			RegionVFX payload2 = assetLookupTree2.GetPayload(_assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				foreach (VfxData vfxData in payload2.VfxDatas)
				{
					float time = Mathf.Clamp01(vfxData.PrefabData.StartTime);
					if (payload != null)
					{
						splineEvents.Events.Add(new SplineEventAudio(time, payload.SfxData.AudioEvents, data.CardGameObject));
					}
					if (vfxData.PrefabData == null)
					{
						continue;
					}
					DuelScene_CDC cacheCard = data.Card;
					VfxData cachedVfxData = vfxData;
					splineEvents.Events.Add(new SplineEventCallback(time, delegate
					{
						if (!(cacheCard == null) && !(cacheCard.EffectsRoot == null))
						{
							_vfxProvider.PlayVFX(cachedVfxData, cacheCard.Model);
						}
					}));
				}
				if (base.CardViews != null && data.Card.Model.Instance != null && data.Card.Model.Instance.AttachedToId != 0)
				{
					splineEvents.Events.Add(new SplineEventCallback(1f, delegate
					{
						base.CardViews.FirstOrDefault((DuelScene_CDC x) => x.InstanceId == data.Card.Model.Instance.AttachedToId)?.PlayReactionAnimation(CardReactionEnum.Attachment);
					}));
				}
			}
		}
		return splineEvents;
	}

	private void PlayEtbTriggerVFX(SplineEventData events, IBattlefieldStack battlefieldStack)
	{
		foreach (DuelScene_CDC cardView in base.CardViews)
		{
			_vfxProvider.GenerateEtbTriggerEvents(cardView, events.Events, battlefieldStack, cardView.EffectsRoot);
		}
	}

	protected override SplineEventData GetLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData layoutSplineEvents = base.GetLayoutSplineEvents(data);
		if (data == null || data.Card == null || data.CardGameObject == null)
		{
			Debug.LogErrorFormat("Null card passed into BattlefieldCardHolder.GetLayoutSplineEvents()!");
			return layoutSplineEvents;
		}
		IBattlefieldStack stackForCard = GetStackForCard(data.Card);
		bool num = data.Card.Model.Zone.Type == ZoneType.Battlefield && data.Card.PreviousCardHolder.CardHolderType != CardHolderType.Hand;
		bool flag = _gameManager.ViewManager.GetCardPreviousId(data.Card.InstanceId) != data.Card.InstanceId && data.Card.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield && data.Card.PreviousCardHolder.CardHolderType == CardHolderType.Hand;
		bool flag2 = data.Card.Model.Zone.Type == ZoneType.Exile && data.Card.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield;
		if (num || flag)
		{
			PlayEtbTriggerVFX(layoutSplineEvents, stackForCard);
			layoutSplineEvents.Events.AddRange(_vfxProvider.GenerateEtbSplineEvents(data.Card, stackForCard, !_handledEtbStacks.Contains(stackForCard), data.Card.EffectsRoot));
			layoutSplineEvents.Events.Add(new SplineEventCallbackWithParams<IReadOnlyCollection<DuelScene_CDC>>(1f, _cardViews, delegate(float _, IReadOnlyCollection<DuelScene_CDC> cdcList)
			{
				foreach (DuelScene_CDC cdc in cdcList)
				{
					cdc.PlayReactionAnimation(CardReactionEnum.ETB);
				}
			}));
		}
		else if (flag2)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_exile, data.Card.gameObject);
		}
		_handledEtbStacks.Add(stackForCard);
		return layoutSplineEvents;
	}

	private void OnCardUpdated(BASE_CDC cardView)
	{
		if (cardView.Model != null && cardView.Model.Zone != null && cardView.Model.Zone.Type == ZoneType.Battlefield && cardView is DuelScene_CDC card && GetStackForCard(card) is BattlefieldStack item)
		{
			_stacksToRefreshInLateUpdate.Add(item);
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		for (int i = 0; i < LocalLandRegion.LayoutVariants.Length; i++)
		{
			BattlefieldRegionDefinition.LayoutVariant layoutVariant = LocalLandRegion.LayoutVariants[i];
			GizmosDraw.Square(layoutVariant.Bounds, UnityEngine.Color.Lerp(UnityEngine.Color.black, UnityEngine.Color.white, (float)i / (float)LocalLandRegion.LayoutVariants.Length));
			if (layoutVariant.UsesPaging)
			{
				GizmosDraw.Square(layoutVariant.LeftPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.black, UnityEngine.Color.cyan, (float)i / (float)LocalLandRegion.LayoutVariants.Length));
				GizmosDraw.Square(layoutVariant.RightPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.black, UnityEngine.Color.magenta, (float)i / (float)LocalLandRegion.LayoutVariants.Length));
			}
		}
		for (int j = 0; j < LocalArtifactRegion.LayoutVariants.Length; j++)
		{
			BattlefieldRegionDefinition.LayoutVariant layoutVariant2 = LocalArtifactRegion.LayoutVariants[j];
			GizmosDraw.Square(layoutVariant2.Bounds, UnityEngine.Color.Lerp(UnityEngine.Color.blue, UnityEngine.Color.white, (float)j / (float)LocalArtifactRegion.LayoutVariants.Length));
			if (layoutVariant2.UsesPaging)
			{
				GizmosDraw.Square(layoutVariant2.LeftPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.blue, UnityEngine.Color.cyan, (float)j / (float)LocalArtifactRegion.LayoutVariants.Length));
				GizmosDraw.Square(layoutVariant2.RightPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.blue, UnityEngine.Color.magenta, (float)j / (float)LocalArtifactRegion.LayoutVariants.Length));
			}
		}
		for (int k = 0; k < LocalCreatureRegion.LayoutVariants.Length; k++)
		{
			BattlefieldRegionDefinition.LayoutVariant layoutVariant3 = LocalCreatureRegion.LayoutVariants[k];
			GizmosDraw.Square(layoutVariant3.Bounds, UnityEngine.Color.Lerp(UnityEngine.Color.cyan, UnityEngine.Color.white, (float)k / (float)LocalCreatureRegion.LayoutVariants.Length));
			if (layoutVariant3.UsesPaging)
			{
				GizmosDraw.Square(layoutVariant3.LeftPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.cyan, UnityEngine.Color.cyan, (float)k / (float)LocalCreatureRegion.LayoutVariants.Length));
				GizmosDraw.Square(layoutVariant3.RightPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.cyan, UnityEngine.Color.magenta, (float)k / (float)LocalCreatureRegion.LayoutVariants.Length));
			}
		}
		for (int l = 0; l < LocalPlaneswalkerRegion.LayoutVariants.Length; l++)
		{
			BattlefieldRegionDefinition.LayoutVariant layoutVariant4 = LocalPlaneswalkerRegion.LayoutVariants[l];
			GizmosDraw.Square(layoutVariant4.Bounds, UnityEngine.Color.Lerp(UnityEngine.Color.gray, UnityEngine.Color.white, (float)l / (float)LocalPlaneswalkerRegion.LayoutVariants.Length));
			if (layoutVariant4.UsesPaging)
			{
				GizmosDraw.Square(layoutVariant4.LeftPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.gray, UnityEngine.Color.cyan, (float)l / (float)LocalPlaneswalkerRegion.LayoutVariants.Length));
				GizmosDraw.Square(layoutVariant4.RightPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.gray, UnityEngine.Color.magenta, (float)l / (float)LocalPlaneswalkerRegion.LayoutVariants.Length));
			}
		}
		for (int m = 0; m < OpponentLandRegion.LayoutVariants.Length; m++)
		{
			BattlefieldRegionDefinition.LayoutVariant layoutVariant5 = OpponentLandRegion.LayoutVariants[m];
			GizmosDraw.Square(layoutVariant5.Bounds, UnityEngine.Color.Lerp(UnityEngine.Color.green, UnityEngine.Color.white, (float)m / (float)OpponentLandRegion.LayoutVariants.Length));
			if (layoutVariant5.UsesPaging)
			{
				GizmosDraw.Square(layoutVariant5.LeftPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.green, UnityEngine.Color.cyan, (float)m / (float)OpponentLandRegion.LayoutVariants.Length));
				GizmosDraw.Square(layoutVariant5.RightPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.green, UnityEngine.Color.magenta, (float)m / (float)OpponentLandRegion.LayoutVariants.Length));
			}
		}
		for (int n = 0; n < OpponentArtifactRegion.LayoutVariants.Length; n++)
		{
			BattlefieldRegionDefinition.LayoutVariant layoutVariant6 = OpponentArtifactRegion.LayoutVariants[n];
			GizmosDraw.Square(layoutVariant6.Bounds, UnityEngine.Color.Lerp(UnityEngine.Color.magenta, UnityEngine.Color.white, (float)n / (float)OpponentArtifactRegion.LayoutVariants.Length));
			if (layoutVariant6.UsesPaging)
			{
				GizmosDraw.Square(layoutVariant6.LeftPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.magenta, UnityEngine.Color.cyan, (float)n / (float)OpponentArtifactRegion.LayoutVariants.Length));
				GizmosDraw.Square(layoutVariant6.RightPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.magenta, UnityEngine.Color.magenta, (float)n / (float)OpponentArtifactRegion.LayoutVariants.Length));
			}
		}
		for (int num = 0; num < OpponentCreatureRegion.LayoutVariants.Length; num++)
		{
			BattlefieldRegionDefinition.LayoutVariant layoutVariant7 = OpponentCreatureRegion.LayoutVariants[num];
			GizmosDraw.Square(layoutVariant7.Bounds, UnityEngine.Color.Lerp(UnityEngine.Color.red, UnityEngine.Color.white, (float)num / (float)OpponentCreatureRegion.LayoutVariants.Length));
			if (layoutVariant7.UsesPaging)
			{
				GizmosDraw.Square(layoutVariant7.LeftPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.red, UnityEngine.Color.cyan, (float)num / (float)OpponentCreatureRegion.LayoutVariants.Length));
				GizmosDraw.Square(layoutVariant7.RightPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.red, UnityEngine.Color.magenta, (float)num / (float)OpponentCreatureRegion.LayoutVariants.Length));
			}
		}
		for (int num2 = 0; num2 < OpponentPlaneswalkerRegion.LayoutVariants.Length; num2++)
		{
			BattlefieldRegionDefinition.LayoutVariant layoutVariant8 = OpponentPlaneswalkerRegion.LayoutVariants[num2];
			GizmosDraw.Square(layoutVariant8.Bounds, UnityEngine.Color.Lerp(UnityEngine.Color.yellow, UnityEngine.Color.white, (float)num2 / (float)OpponentPlaneswalkerRegion.LayoutVariants.Length));
			if (layoutVariant8.UsesPaging)
			{
				GizmosDraw.Square(layoutVariant8.LeftPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.yellow, UnityEngine.Color.cyan, (float)num2 / (float)OpponentPlaneswalkerRegion.LayoutVariants.Length));
				GizmosDraw.Square(layoutVariant8.RightPagingArrow, UnityEngine.Color.Lerp(UnityEngine.Color.yellow, UnityEngine.Color.magenta, (float)num2 / (float)OpponentPlaneswalkerRegion.LayoutVariants.Length));
			}
		}
	}
}
