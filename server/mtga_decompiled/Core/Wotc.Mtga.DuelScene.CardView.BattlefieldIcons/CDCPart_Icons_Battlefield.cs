using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Ability.Metadata;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.CardParts.Utils;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

public class CDCPart_Icons_Battlefield : CDCPart_Icons
{
	[SerializeField]
	private RectTransform _revealedEyeRoot;

	[Space(10f)]
	[Header("Battlefield Prefabs")]
	[SerializeField]
	protected GameObject _warningIconPrefab;

	[Header("Badge Tray")]
	[SerializeField]
	protected BadgeEntryView _readMoreBadgePrefab;

	[SerializeField]
	protected RectTransform _badgeTrayParent;

	[SerializeField]
	private RectTransform _area;

	[SerializeField]
	protected RectTransform _countBadgeTrayParent;

	[SerializeField]
	protected GameObject _plumpVFX;

	[SerializeField]
	protected float _defaultBadgeWidth = 0.75f;

	[SerializeField]
	protected bool _hideBadges;

	private CombatIcon_AttackerFrame _attackerFrameInstance;

	private CombatIcon_BlockerFrame _blockerFrameInstance;

	private GameObject _combatWarningInstance;

	private GameObject _cantAttackOrBlockInstance;

	private (WontUntapVFX, GameObject) _wontUntapInstance;

	private readonly HashSet<uint> _queuedPlumpAbilityIds = new HashSet<uint>();

	private readonly List<BadgeEntrySpawnData> _visibleBadgeEntryDatas = new List<BadgeEntrySpawnData>(3);

	private readonly List<BadgeEntrySpawnData> _hiddenBadgeEntryDatas = new List<BadgeEntrySpawnData>(3);

	private readonly List<BadgeEntryView> _badgeEntryViews = new List<BadgeEntryView>(3);

	private int _previousBadgeCount = -1;

	private bool _shouldImmediatelyUpdateLayout;

	protected override void OnInit()
	{
		base.OnInit();
		_revealedEyeAnchor = _revealedEyeRoot;
	}

	public override void SetVisible(bool visible)
	{
		base.SetVisible(visible);
		bool active = visible && !_cachedDestroyed;
		if (_badgeTrayParent.childCount != 0)
		{
			_badgeTrayParent.gameObject.UpdateActive(active);
		}
		if (_countBadgeTrayParent.childCount != 0)
		{
			_countBadgeTrayParent.gameObject.UpdateActive(active);
		}
	}

	public virtual void SetCombatIcons(CombatStateData combatState)
	{
		if (combatState.CombatAttackState != CombatAttackState.None)
		{
			if (_attackerFrameInstance == null)
			{
				_assetLookupSystem.Blackboard.Clear();
				_assetLookupSystem.Blackboard.SetCardDataExtensive(_cachedModel);
				if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AttackerIcon> loadedTree))
				{
					AttackerIcon payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
					if (payload != null && !string.IsNullOrWhiteSpace(payload?.FrameRef?.RelativePath))
					{
						_attackerFrameInstance = _unityObjectPool.PopObject(payload.FrameRef.RelativePath).GetComponent<CombatIcon_AttackerFrame>();
						_attackerFrameInstance.transform.SetParent(_iconRoot, worldPositionStays: false);
						_attackerFrameInstance.transform.localPosition = payload.OffsetData.PositionOffset;
						_attackerFrameInstance.transform.localEulerAngles = payload.OffsetData.RotationOffset;
						_attackerFrameInstance.transform.localScale = payload.OffsetData.ScaleMultiplier;
					}
				}
			}
			if (_attackerFrameInstance != null)
			{
				_attackerFrameInstance.SetupForState(combatState.CombatAttackState);
			}
		}
		else if (_attackerFrameInstance != null)
		{
			_unityObjectPool.PushObject(_attackerFrameInstance.gameObject);
			_attackerFrameInstance = null;
		}
		if (combatState.CombatBlockState != CombatBlockState.None)
		{
			if (_blockerFrameInstance == null)
			{
				_assetLookupSystem.Blackboard.Clear();
				_assetLookupSystem.Blackboard.SetCardDataExtensive(_cachedModel);
				if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<BlockerIcon> loadedTree2))
				{
					BlockerIcon payload2 = loadedTree2.GetPayload(_assetLookupSystem.Blackboard);
					if (payload2 != null && !string.IsNullOrWhiteSpace(payload2?.FrameRef?.RelativePath))
					{
						_blockerFrameInstance = _unityObjectPool.PopObject(payload2.FrameRef.RelativePath).GetComponent<CombatIcon_BlockerFrame>();
						_blockerFrameInstance.transform.SetParent(_iconRoot, worldPositionStays: false);
						_blockerFrameInstance.transform.localPosition = payload2.OffsetData.PositionOffset;
						_blockerFrameInstance.transform.localEulerAngles = payload2.OffsetData.RotationOffset;
						_blockerFrameInstance.transform.localScale = payload2.OffsetData.ScaleMultiplier;
					}
				}
			}
			if (_blockerFrameInstance != null)
			{
				_blockerFrameInstance.SetupForState(combatState.CombatBlockState);
			}
		}
		else if (_blockerFrameInstance != null)
		{
			_unityObjectPool.PushObject(_blockerFrameInstance.gameObject);
			_blockerFrameInstance = null;
		}
	}

	public void UpdateCombatWarningIcon(bool hasCombatWarning)
	{
		if (hasCombatWarning)
		{
			if (_combatWarningInstance == null && _warningIconPrefab != null)
			{
				_combatWarningInstance = _unityObjectPool.PopObject(_warningIconPrefab);
				_combatWarningInstance.transform.SetParent(_iconRoot, worldPositionStays: false);
				_combatWarningInstance.transform.localPosition = _warningIconPrefab.transform.localPosition;
				_combatWarningInstance.transform.localEulerAngles = _warningIconPrefab.transform.localEulerAngles;
				_combatWarningInstance.transform.localScale = _warningIconPrefab.transform.localScale;
			}
		}
		else if (_combatWarningInstance != null)
		{
			_unityObjectPool.PushObject(_combatWarningInstance);
			_combatWarningInstance = null;
		}
	}

	protected override void UpdateIcons()
	{
		base.UpdateIcons();
		UpdateUntapRestrictions();
	}

	protected override void HandleDestructionInternal()
	{
		base.HandleDestructionInternal();
		SetVisible(!_cachedDestroyed);
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		CleanupCombatIcons();
		if ((bool)_wontUntapInstance.Item2)
		{
			_unityObjectPool?.PushObject(_wontUntapInstance.Item2);
		}
		_wontUntapInstance = default((WontUntapVFX, GameObject));
		_queuedPlumpAbilityIds.Clear();
		foreach (BadgeEntryView badgeEntryView in _badgeEntryViews)
		{
			badgeEntryView.Cleanup();
			_unityObjectPool.PushObject(badgeEntryView.gameObject);
		}
		_badgeEntryViews.Clear();
		_hiddenBadgeEntryDatas.Clear();
		_visibleBadgeEntryDatas.Clear();
	}

	public void CleanupCombatIcons()
	{
		if (_attackerFrameInstance != null)
		{
			_unityObjectPool.PushObject(_attackerFrameInstance.gameObject);
			_attackerFrameInstance = null;
		}
		if (_blockerFrameInstance != null)
		{
			_unityObjectPool.PushObject(_blockerFrameInstance.gameObject);
			_blockerFrameInstance = null;
		}
		if (_combatWarningInstance != null)
		{
			_unityObjectPool.PushObject(_combatWarningInstance);
			_combatWarningInstance = null;
		}
		if (_cantAttackOrBlockInstance != null)
		{
			_unityObjectPool.PushObject(_cantAttackOrBlockInstance);
			_cantAttackOrBlockInstance = null;
		}
	}

	public override Rect GetRect()
	{
		return RectTransformUtils.GetRelativeRect(_badgeTrayParent, _badgeTrayParent.anchoredPosition);
	}

	protected override void UpdateBadges()
	{
		if (_hideBadges || !_shouldBeVisible)
		{
			return;
		}
		_visibleBadgeEntryDatas.Clear();
		_hiddenBadgeEntryDatas.Clear();
		foreach (BadgeEntryView badgeEntryView in _badgeEntryViews)
		{
			badgeEntryView.Cleanup();
			_unityObjectPool.PushObject(badgeEntryView.gameObject);
		}
		_badgeEntryViews.Clear();
		MtgGameState currentGameState = base.GetCurrentGameState?.Invoke();
		WorkflowBase currentInteraction = base.GetCurrentInteraction?.Invoke();
		TryAddCardBadges(_assetLookupSystem, _visibleBadgeEntryDatas, _cachedModel, _cachedCardHolderType, _cachedViewMetadata, currentGameState, currentInteraction);
		TryAddAbilityBadges(_assetLookupSystem, _visibleBadgeEntryDatas, _cachedModel, _cachedCardHolderType, _cachedViewMetadata, currentGameState, currentInteraction);
		CDCPart_Icons.SortBadges(_visibleBadgeEntryDatas);
		CDCPart_Icons.DedupeBadges(_visibleBadgeEntryDatas);
		List<BadgeEntrySpawnData> list = new List<BadgeEntrySpawnData>();
		HideMultipleNumeralBadges(_cachedModel, _visibleBadgeEntryDatas, list);
		float width = _area.rect.width;
		HideExcessVisibleBadges(_cachedModel, _visibleBadgeEntryDatas, _hiddenBadgeEntryDatas, width, _defaultBadgeWidth, out var entriesExceedWidth);
		bool flag = entriesExceedWidth;
		for (int i = 0; i < _visibleBadgeEntryDatas.Count; i++)
		{
			AddBadgeView(_visibleBadgeEntryDatas[i]);
		}
		list.ForEach(AddBadgeView);
		if (flag)
		{
			bool flag2 = false;
			foreach (BadgeEntrySpawnData hiddenBadgeEntryData in _hiddenBadgeEntryDatas)
			{
				if (hiddenBadgeEntryData.AbilityPrintingData != null && _queuedPlumpAbilityIds.Remove(hiddenBadgeEntryData.AbilityPrintingData.Id))
				{
					flag2 = true;
				}
			}
			BadgeEntryView component = _unityObjectPool.PopObject(_readMoreBadgePrefab.gameObject).GetComponent<BadgeEntryView>();
			component.transform.SetParent(_badgeTrayParent.transform);
			component.transform.ZeroOut();
			component.Init(BadgeEntryStatus.Normal);
			component.gameObject.UpdateActive(active: true);
			if (flag2)
			{
				GameObject gameObject = _unityObjectPool.PopObject(_plumpVFX);
				if ((bool)gameObject)
				{
					gameObject.transform.SetParent(component.VfxTransform, worldPositionStays: false);
				}
			}
			_badgeEntryViews.Add(component);
		}
		if (_previousBadgeCount != _visibleBadgeEntryDatas.Count)
		{
			_shouldImmediatelyUpdateLayout = true;
		}
		else
		{
			LayoutRebuilder.MarkLayoutForRebuild(_badgeTrayParent);
			LayoutRebuilder.MarkLayoutForRebuild(_countBadgeTrayParent);
		}
		_previousBadgeCount = _visibleBadgeEntryDatas.Count;
	}

	protected override void OnLateUpdate()
	{
		base.OnLateUpdate();
		if (_shouldImmediatelyUpdateLayout)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(_badgeTrayParent);
			LayoutRebuilder.ForceRebuildLayoutImmediate(_countBadgeTrayParent);
			_shouldImmediatelyUpdateLayout = false;
		}
	}

	private void AddBadgeView(BadgeEntrySpawnData badgeEntrySpawnData)
	{
		BadgeEntryView badgeEntryView = badgeEntrySpawnData.BadgeEntryViewCreator?.Create(_unityObjectPool);
		badgeEntryView.Init(badgeEntrySpawnData.BadgeEntryStatus);
		if (badgeEntryView.IsNumeric)
		{
			badgeEntryView.transform.SetParent(_countBadgeTrayParent.transform);
			_countBadgeTrayParent.gameObject.UpdateActive(active: true);
		}
		else
		{
			badgeEntryView.transform.SetParent(_badgeTrayParent.transform);
			_badgeTrayParent.gameObject.UpdateActive(active: true);
		}
		if (_countBadgeTrayParent.childCount == 0)
		{
			_countBadgeTrayParent.gameObject.UpdateActive(active: false);
		}
		if (_badgeTrayParent.childCount == 0)
		{
			_badgeTrayParent.gameObject.UpdateActive(active: false);
		}
		badgeEntryView.transform.ZeroOut();
		badgeEntryView.gameObject.UpdateActive(active: true);
		if (badgeEntrySpawnData.AbilityPrintingData != null && _queuedPlumpAbilityIds.Remove(badgeEntrySpawnData.AbilityPrintingData.Id))
		{
			GameObject gameObject = _unityObjectPool.PopObject(_plumpVFX);
			if ((bool)gameObject)
			{
				gameObject.transform.SetParent(badgeEntryView.VfxTransform, worldPositionStays: false);
			}
		}
		_badgeEntryViews.Add(badgeEntryView);
		NumericBadgeCalculatorInput? numericInput = null;
		if (badgeEntryView.IsNumeric)
		{
			numericInput = new NumericBadgeCalculatorInput
			{
				CardData = _cachedModel,
				Ability = badgeEntrySpawnData.AbilityPrintingData,
				GameState = base.GetCurrentGameState?.Invoke()
			};
		}
		BadgeActivationCalculatorInput value = new BadgeActivationCalculatorInput(_cachedModel, badgeEntrySpawnData.AbilityPrintingData, base.GetCurrentGameState?.Invoke());
		badgeEntryView.InitDataViews(badgeEntrySpawnData.BadgeEntryData, numericInput, value);
	}

	private void UpdateUntapRestrictions()
	{
		if (_shouldBeVisible && !_cachedDestroyed)
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(_cachedModel);
			_assetLookupSystem.Blackboard.SetCdcViewMetadata(_cachedViewMetadata);
			_assetLookupSystem.Blackboard.CardHolderType = _cachedCardHolderType;
			WontUntapVFX wontUntapVFX = null;
			AssetLookupTree<WontUntapVFX> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<WontUntapVFX>();
			if (assetLookupTree != null)
			{
				WontUntapVFX payload = assetLookupTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					wontUntapVFX = payload;
				}
			}
			if (wontUntapVFX == _wontUntapInstance.Item1)
			{
				return;
			}
			RemoveOldSustain();
			WontUntapVFX item = _wontUntapInstance.Item1;
			if (!string.IsNullOrWhiteSpace(item?.DespawnVfxReference?.RelativePath))
			{
				GameObject gameObject = AssetLoader.Instantiate(item.DespawnVfxReference.RelativePath, _iconRoot);
				if (gameObject != null)
				{
					gameObject.transform.ZeroOut();
					gameObject.AddOrGetComponent<SelfCleanup>();
				}
			}
			if (!string.IsNullOrWhiteSpace(wontUntapVFX?.SpawnVfxReference?.RelativePath))
			{
				GameObject gameObject2 = AssetLoader.Instantiate(wontUntapVFX.SpawnVfxReference.RelativePath, _iconRoot);
				if (gameObject2 != null)
				{
					gameObject2.transform.ZeroOut();
					gameObject2.AddOrGetComponent<SelfCleanup>();
				}
			}
			GameObject gameObject3 = null;
			if (!string.IsNullOrWhiteSpace(wontUntapVFX?.SustainVfxReference?.RelativePath))
			{
				gameObject3 = AssetLoader.Instantiate(wontUntapVFX.SustainVfxReference.RelativePath, _iconRoot);
				if (gameObject3 != null)
				{
					gameObject3.transform.ZeroOut();
				}
			}
			_wontUntapInstance = (wontUntapVFX, gameObject3);
		}
		else
		{
			RemoveOldSustain();
			_wontUntapInstance = (null, null);
		}
	}

	private void RemoveOldSustain()
	{
		GameObject item = _wontUntapInstance.Item2;
		if ((bool)item)
		{
			_unityObjectPool.PushObject(item);
		}
	}

	protected override void HandlePhaseUpdateInternal()
	{
		base.HandlePhaseUpdateInternal();
		UpdateIcons();
		UpdateBadges();
	}

	public void QueueBadgePlump(uint abilityId)
	{
		_queuedPlumpAbilityIds.Add(abilityId);
	}

	private static void TryAddCardBadges(AssetLookupSystem assetLookupSystem, IList<BadgeEntrySpawnData> visibleBadgeEntries, ICardDataAdapter cardModel, CardHolderType cardHolderType, CDCViewMetadata viewMetadata, MtgGameState currentGameState, WorkflowBase currentInteraction)
	{
		if (!assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AssetLookupTree.Payloads.Card.BadgeEntry> loadedTree) || !assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<BadgeView> loadedTree2))
		{
			return;
		}
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardModel);
		assetLookupSystem.Blackboard.SetCdcViewMetadata(viewMetadata);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		assetLookupSystem.Blackboard.GameState = currentGameState;
		assetLookupSystem.Blackboard.Interaction = currentInteraction;
		HashSet<AssetLookupTree.Payloads.Card.BadgeEntry> hashSet = new HashSet<AssetLookupTree.Payloads.Card.BadgeEntry>();
		if (!loadedTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet))
		{
			return;
		}
		foreach (AssetLookupTree.Payloads.Card.BadgeEntry item in hashSet)
		{
			if (item?.Data != null && item.Data.ValidOnBattlefield)
			{
				assetLookupSystem.Blackboard.BadgeData = item.Data;
				BadgeView payload = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
				if (payload != null)
				{
					visibleBadgeEntries.Add(new BadgeEntrySpawnData(item.Data, BadgeEntryStatus.Special, null, new BadgeEntryViewCreator(payload.BadgeEntryView.RelativePath)));
				}
			}
		}
	}

	public static void TryAddAbilityBadges(AssetLookupSystem assetLookupSystem, IList<BadgeEntrySpawnData> visibleBadgeEntries, ICardDataAdapter cardModel, CardHolderType cardHolderType, CDCViewMetadata viewMetadata, MtgGameState currentGameState, WorkflowBase currentInteraction)
	{
		if (cardModel.AllAbilities.Count == 0 || !assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AssetLookupTree.Payloads.Ability.BadgeEntry> loadedTree) || !assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<BadgeView> loadedTree2))
		{
			return;
		}
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardModel);
		assetLookupSystem.Blackboard.SetCdcViewMetadata(viewMetadata);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		assetLookupSystem.Blackboard.GameState = currentGameState;
		assetLookupSystem.Blackboard.Interaction = currentInteraction;
		foreach (KeyValuePair<AbilityPrintingData, AbilityState> allAbility in cardModel.AllAbilities)
		{
			AbilityPrintingData key = allAbility.Key;
			AbilityState value = allAbility.Value;
			assetLookupSystem.Blackboard.Ability = key;
			if (value == AbilityState.Removed)
			{
				continue;
			}
			BadgeEntryStatus badgeEntryStatus;
			if (cardModel.Instance.HasPerpetualAddedAbility(key))
			{
				badgeEntryStatus = BadgeEntryStatus.Perpetual;
			}
			else
			{
				switch (value)
				{
				case AbilityState.Normal:
				case AbilityState.Normal | AbilityState.Exhausted:
					badgeEntryStatus = BadgeEntryStatus.Normal;
					break;
				case AbilityState.Added:
					badgeEntryStatus = BadgeEntryStatus.Added;
					break;
				default:
					badgeEntryStatus = BadgeEntryStatus.Other;
					break;
				}
			}
			HashSet<AssetLookupTree.Payloads.Ability.BadgeEntry> hashSet = new HashSet<AssetLookupTree.Payloads.Ability.BadgeEntry>();
			if (!loadedTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet))
			{
				continue;
			}
			foreach (AssetLookupTree.Payloads.Ability.BadgeEntry item in hashSet)
			{
				BadgeEntryStatus badgeEntryStatus2 = badgeEntryStatus;
				IBadgeEntryData data = item.Data;
				if (data != null && data.ValidOnBattlefield)
				{
					if (data.NumberCalculator.HasNumber(new NumericBadgeCalculatorInput
					{
						Ability = key,
						CardData = cardModel
					}))
					{
						badgeEntryStatus2 = BadgeEntryStatus.Special;
					}
					if (data.UseSpecialEntryStatusOnBattlefield)
					{
						badgeEntryStatus2 = BadgeEntryStatus.Special;
					}
					assetLookupSystem.Blackboard.BadgeData = data;
					BadgeView payload = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
					if (payload != null)
					{
						BadgeEntryViewCreator badgeEntryView = new BadgeEntryViewCreator(payload.BadgeEntryView.RelativePath);
						visibleBadgeEntries.Add(new BadgeEntrySpawnData(data, badgeEntryStatus2, key, badgeEntryView));
					}
					else
					{
						Debug.LogException(new NullReferenceException($"No badge view found for ability #{key.Id}"));
					}
				}
			}
		}
	}

	public static void HideMultipleNumeralBadges(ICardDataAdapter cardModel, IList<BadgeEntrySpawnData> visibleBadgeEntries, IList<BadgeEntrySpawnData> hiddenBadgeEntries)
	{
		for (int i = 0; i < visibleBadgeEntries.Count; i++)
		{
			if (visibleBadgeEntries[i].BadgeEntryData.NumberCalculator.HasNumber(new NumericBadgeCalculatorInput
			{
				Ability = visibleBadgeEntries[i].AbilityPrintingData,
				CardData = cardModel
			}))
			{
				hiddenBadgeEntries.Add(visibleBadgeEntries[i]);
				visibleBadgeEntries.RemoveAt(i);
			}
		}
	}

	public static void HideExcessVisibleBadges(ICardDataAdapter cardModel, IList<BadgeEntrySpawnData> visibleBadgeEntries, IList<BadgeEntrySpawnData> hiddenBadgeEntries, float availableWidth, float defaultEntryWidth, out bool entriesExceedWidth)
	{
		entriesExceedWidth = false;
		for (int i = 0; i < visibleBadgeEntries.Count; i++)
		{
			BadgeEntrySpawnData item = visibleBadgeEntries[i];
			float num = (item.BadgeEntryData.NumberCalculator.HasNumber(new NumericBadgeCalculatorInput
			{
				Ability = item.AbilityPrintingData,
				CardData = cardModel
			}) ? (2f * defaultEntryWidth) : defaultEntryWidth);
			if (num > availableWidth)
			{
				visibleBadgeEntries.RemoveAt(i);
				hiddenBadgeEntries.Add(item);
				i--;
				if (!entriesExceedWidth)
				{
					entriesExceedWidth = true;
					if (i >= 0)
					{
						BadgeEntrySpawnData item2 = visibleBadgeEntries[i];
						visibleBadgeEntries.RemoveAt(i);
						hiddenBadgeEntries.Add(item2);
						i--;
					}
				}
			}
			else
			{
				availableWidth -= num;
			}
		}
	}
}
