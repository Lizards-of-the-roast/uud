using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;
using InteractionSystem;
using Pooling;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.DuelScene.Battlefield;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class BattlefieldLayout_MP : BattlefieldCardHolder.BattlefieldLayout
{
	private BattlefieldRegion _multiplayerSlushRegion;

	public uint[] FocusPlayerIds { private get; set; } = Array.Empty<uint>();

	public BattlefieldLayout_MP(BattlefieldCardHolder_MP holder, DuelSceneLogger logger, IBattlefieldStackFactory stackFactory, IContext context, GameInteractionSystem interactionSystem, NPEDirector npeDirector)
		: base(holder, logger, stackFactory, context, interactionSystem, npeDirector)
	{
		_multiplayerSlushRegion = new BattlefieldRegion(holder.MultiplayerSlushRegion, opponent: false, holder, context.Get<IObjectPool>());
		base.Regions = new List<BattlefieldRegion>(base.Regions) { _multiplayerSlushRegion }.ToArray();
	}

	public override void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		Dictionary<BattlefieldRegion, HashSet<DuelScene_CDC>> dictionary = new Dictionary<BattlefieldRegion, HashSet<DuelScene_CDC>>();
		BattlefieldRegion[] regions = base.Regions;
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
			IEnumerable<MtgCardInstance> otherInstances = BattlefieldCardHolder.BattlefieldLayout.CardViewAndLimboInstances(allCardViews, _gameStateProvider.CurrentGameState, _browserProvider.CurrentBrowser);
			MtgCardInstance mtgCardInstance = BattlefieldCardHolder.BattlefieldLayout.FindStackParentCardInstance(allCardView.Model.Instance, otherInstances);
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
			BattlefieldCardHolder.BattlefieldStack battlefieldStack = null;
			foreach (BattlefieldCardHolder.BattlefieldStack item2 in _stacksCache)
			{
				if (item2.CanStack(item))
				{
					battlefieldStack = item2;
					break;
				}
			}
			if (battlefieldStack == null || (_npeDirector != null && !_npeDirector.AllowsStackingOnBattlefield && item.Model.CardTypes.Contains(CardType.Creature)))
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
			BattlefieldCardHolder.BattlefieldStack battlefieldStack2 = null;
			foreach (BattlefieldCardHolder.BattlefieldStack item4 in _stacksCache)
			{
				if (item4.StackParentModel.InstanceId == item3.Key)
				{
					battlefieldStack2 = item4;
					break;
				}
			}
			if (battlefieldStack2 == null)
			{
				IEnumerable<MtgCardInstance> otherInstances2 = BattlefieldCardHolder.BattlefieldLayout.CardViewAndLimboInstances(allCardViews, _gameStateProvider.CurrentGameState, _browserProvider.CurrentBrowser);
				MtgCardInstance mtgCardInstance2 = BattlefieldCardHolder.BattlefieldLayout.FindStackParentCardInstance(item3.Value[0].Model.Instance, otherInstances2);
				if (mtgCardInstance2 != null)
				{
					_stacksCache.Add(_stackFactory.Create(mtgCardInstance2, item3.Value));
				}
			}
		}
		long num = 0L;
		foreach (BattlefieldCardHolder.BattlefieldStack item5 in _stacksCache)
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
		foreach (BattlefieldCardHolder.BattlefieldStack item6 in _stacksCache)
		{
			if (item6.StackParentModel.CardTypes.Contains(CardType.Battle) || item6.StackParentModel.Controller.IsLocalPlayer || ((IReadOnlyCollection<uint>)(object)FocusPlayerIds).Contains(item6.StackParentModel.Controller.InstanceId))
			{
				if (item6.StackParentModel.CardTypes.Contains(CardType.Battle))
				{
					DesignationData designationData = (from d in _gameStateProvider.CurrentGameState.Value.Players.SelectMany((MtgPlayer p) => p.Designations)
						where d.Type == Designation.Protector
						select d).Find(item6.StackParentModel.InstanceId, (DesignationData designation, uint battleId) => designation.AffectorId == battleId);
					MtgPlayer playerById = _gameStateProvider.CurrentGameState.Value.GetPlayerById(designationData.AffectedId);
					if (playerById != null && !playerById.IsLocalPlayer && !((IReadOnlyCollection<uint>)(object)FocusPlayerIds).Contains(playerById.InstanceId))
					{
						goto IL_0591;
					}
				}
				BattlefieldRegion battlefieldRegion2 = null;
				battlefieldRegion2 = ((item6.StackParentModel.CardTypes.Contains(CardType.Battle) && BattlefieldCardHolder.BattlefieldLayout.TryGetProtector(item6.StackParentModel, _gameStateProvider.CurrentGameState.Value.Players, out var protector)) ? (protector switch
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
				continue;
			}
			goto IL_0591;
			IL_0591:
			_multiplayerSlushRegion.Stacks.Add(item6);
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
		GenerateRegionData(_multiplayerSlushRegion, allData, calcRegionVariant: false);
		foreach (CardLayoutData allDatum in allData)
		{
			if (_multiplayerSlushRegion.ContainsCardView(allDatum.Card))
			{
				allDatum.IsVisibleInLayout = false;
			}
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
			BattlefieldCardHolder.StackCounterData stackCounterData = _pendingStackCounters[num2];
			CdcStackCounterView cdcStackCounterView2 = ActiveStackCounters[num2];
			cdcStackCounterView2.transform.position = stackCounterData.positon;
			cdcStackCounterView2.SetCount(stackCounterData.count);
			cdcStackCounterView2.Init(_interactionSystem, stackCounterData.parentInstanceId);
		}
		for (int num3 = 0; num3 < _takenRects.Count; num3++)
		{
			BattlefieldCardHolder.DebugDraw.Square(_takenRects[num3], UnityEngine.Color.Lerp(UnityEngine.Color.red, UnityEngine.Color.blue, (float)num3 / (float)_takenRects.Count), 2.5f);
		}
		_logger?.UpdateMaxCreatures(_localCreatureRegion.Stacks.Sum((BattlefieldCardHolder.BattlefieldStack stack) => stack.CardCount));
		_logger?.UpdateMaxLands(_localLandRegion.Stacks.Sum((BattlefieldCardHolder.BattlefieldStack stack) => stack.CardCount));
		_logger?.UpdateMaxArtifactsAndEnchantments(_localArtifactRegion.Stacks.Sum((BattlefieldCardHolder.BattlefieldStack stack) => stack.CardCount));
	}
}
