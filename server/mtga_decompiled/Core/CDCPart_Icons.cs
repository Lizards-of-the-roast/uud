using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Ability.Metadata;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;
using Wotc.Mtga.DuelScene.CardView;
using Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtgo.Gre.External.Messaging;

public class CDCPart_Icons : CDCPart
{
	[Header("Prefabs")]
	[SerializeField]
	protected GameObject _tapIconPrefab;

	[SerializeField]
	protected GameObject _revealedIconPrefab;

	[Header("Anchors")]
	[SerializeField]
	protected Transform _iconRoot;

	[SerializeField]
	private Transform _badgeGrid;

	protected Transform _revealedEyeAnchor;

	protected bool _shouldBeVisible = true;

	private GameObject _tapIconInstance;

	private GameObject _revealIconInstance;

	protected readonly List<BadgeEntryView> _activeAbilityBadges = new List<BadgeEntryView>();

	private List<BadgeEntrySpawnData> _allBadgeDatasTemp = new List<BadgeEntrySpawnData>();

	protected override void OnInit()
	{
		base.OnInit();
		_revealedEyeAnchor = _badgeGrid;
	}

	public virtual void SetVisible(bool visible)
	{
		_shouldBeVisible = visible;
		bool flag = _shouldBeVisible && !_cachedDestroyed;
		if (_iconRoot.gameObject.activeSelf != flag)
		{
			_iconRoot.gameObject.SetActive(visible);
			if (flag)
			{
				HandleUpdateInternal();
			}
		}
	}

	protected override void HandleDestructionInternal()
	{
		bool flag = _shouldBeVisible && !_cachedDestroyed;
		if (_iconRoot.gameObject.activeSelf != flag)
		{
			_iconRoot.gameObject.SetActive(flag);
		}
		base.HandleDestructionInternal();
	}

	protected override void HandleUpdateInternal()
	{
		UpdateIcons();
		UpdateBadges();
	}

	protected virtual void UpdateIcons()
	{
		if (_cachedModel != null)
		{
			SetTapped(_cachedModel.IsTapped);
			SetRevealed(_cachedModel.RevealedToOpponent);
		}
	}

	protected virtual void UpdateBadges()
	{
		foreach (BadgeEntryView activeAbilityBadge in _activeAbilityBadges)
		{
			if (!(activeAbilityBadge == null))
			{
				activeAbilityBadge.Cleanup();
				if (Application.isPlaying)
				{
					_unityObjectPool.PushObject(activeAbilityBadge.gameObject);
				}
				else
				{
					Object.DestroyImmediate(activeAbilityBadge.gameObject, allowDestroyingAssets: false);
				}
			}
		}
		_activeAbilityBadges.Clear();
		_allBadgeDatasTemp.Clear();
		if (_cachedViewMetadata.IsHoverCopy || _cachedCardHolderType != CardHolderType.Examine)
		{
			MtgGameState mtgGameState = base.GetCurrentGameState?.Invoke();
			WorkflowBase activeInteraction = base.GetCurrentInteraction?.Invoke();
			TryAddCardBadges(_assetLookupSystem, _allBadgeDatasTemp, _cachedModel, _cachedCardHolderType, _cachedViewMetadata, mtgGameState, activeInteraction);
			if (_cachedModel.ObjectType == GameObjectType.Ability)
			{
				AbilityPrintingData abilityPrintingData = null;
				if (_cachedModel.Instance.Abilities.Count == 1)
				{
					abilityPrintingData = _cachedModel.Instance.Abilities[0];
				}
				if (abilityPrintingData == null && _cachedModel.Instance.GrpId > 11)
				{
					abilityPrintingData = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(_cachedModel.Instance.GrpId);
				}
				if (abilityPrintingData == null && _cachedModel.Instance.ObjectSourceGrpId > 11)
				{
					abilityPrintingData = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(_cachedModel.Instance.ObjectSourceGrpId);
				}
				if (abilityPrintingData != null)
				{
					TryAddAbilityBadge(_assetLookupSystem, _allBadgeDatasTemp, abilityPrintingData, _cachedModel, _cachedCardHolderType, _cachedViewMetadata, base.GetCurrentGameState?.Invoke(), base.GetCurrentInteraction?.Invoke());
				}
			}
			else
			{
				if ((_cachedCardHolderType == CardHolderType.Hand || _cachedCardHolderType == CardHolderType.Stack) && _cachedModel.IsAdventureCard(ignoreInstances: true))
				{
					AbilityBadgeData badgeDataForCondition = AbilityBadgeUtil.GetBadgeDataForCondition(_assetLookupSystem, ConditionType.Adventure, _cachedModel);
					if (badgeDataForCondition != null && !string.IsNullOrEmpty(badgeDataForCondition.BadgePrefabPath) && !string.IsNullOrEmpty(badgeDataForCondition.IconSpritePath))
					{
						_allBadgeDatasTemp.Add(new BadgeEntrySpawnData(badgeDataForCondition));
					}
				}
				using IEnumerator<(AbilityPrintingData, AbilityState)> enumerator2 = HangerUtilities.GetAllAbilities(_cachedModel, _cardDatabase.CardDataProvider, includeLinkedFaces: false).GetEnumerator();
				while (enumerator2.MoveNext())
				{
					TryAddAbilityBadge(abilityPrintingData: enumerator2.Current.Item1, assetLookupSystem: _assetLookupSystem, badgeSpawnDatas: _allBadgeDatasTemp, cardModel: _cachedModel, cardHolderType: _cachedCardHolderType, viewMetadata: _cachedViewMetadata, mtgGameState: mtgGameState, activeInteraction: activeInteraction);
				}
			}
		}
		SortBadges(_allBadgeDatasTemp);
		DedupeBadges(_allBadgeDatasTemp);
		int num = (_cachedModel.RevealedToOpponent ? 2 : 3);
		for (int i = 0; i < num && i < _allBadgeDatasTemp.Count; i++)
		{
			BadgeEntrySpawnData badgeEntrySpawnData = _allBadgeDatasTemp[i];
			BadgeEntryView badgeEntryView = badgeEntrySpawnData.BadgeEntryViewCreator.Create(_unityObjectPool);
			if ((bool)badgeEntryView)
			{
				BadgeActivationCalculatorInput badgeActivationCalculatorInput = new BadgeActivationCalculatorInput(_cachedModel, badgeEntrySpawnData.AbilityPrintingData, base.GetCurrentGameState?.Invoke());
				bool active = badgeEntrySpawnData.BadgeEntryData.ActivationCalculator.GetActive(badgeActivationCalculatorInput);
				badgeEntryView.Init(badgeEntrySpawnData.BadgeEntryStatus, active);
				badgeEntryView.transform.SetParent(_badgeGrid);
				badgeEntryView.transform.ZeroOut();
				badgeEntryView.gameObject.UpdateActive(active: true);
				_activeAbilityBadges.Add(badgeEntryView);
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
				badgeEntryView.InitDataViews(badgeEntrySpawnData.BadgeEntryData, numericInput, badgeActivationCalculatorInput);
				if (string.IsNullOrEmpty(badgeEntrySpawnData.BadgeEntryData.SpriteRef.RelativePath) && !badgeEntryView.IsNumeric)
				{
					badgeEntryView.gameObject.UpdateActive(active: false);
				}
			}
		}
		if ((bool)_revealIconInstance && _revealIconInstance.transform.GetSiblingIndex() != 0)
		{
			_revealIconInstance.transform.SetAsFirstSibling();
		}
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		SetRevealed(revealed: false);
		SetTapped(tapped: false);
		SetVisible(visible: true);
		foreach (BadgeEntryView activeAbilityBadge in _activeAbilityBadges)
		{
			activeAbilityBadge.Cleanup();
			_unityObjectPool?.PushObject(activeAbilityBadge.gameObject);
		}
		_activeAbilityBadges.Clear();
		_allBadgeDatasTemp.Clear();
	}

	private void SetRevealed(bool revealed)
	{
		if (revealed)
		{
			if (_revealIconInstance == null)
			{
				_revealIconInstance = _unityObjectPool.PopObject(_revealedIconPrefab);
				Transform obj = _revealIconInstance.transform;
				obj.SetParent(_revealedEyeAnchor);
				obj.SetAsFirstSibling();
				obj.ZeroOut();
			}
		}
		else if (_revealIconInstance != null)
		{
			_unityObjectPool.PushObject(_revealIconInstance);
			_revealIconInstance = null;
		}
	}

	private void SetTapped(bool tapped)
	{
		if (tapped)
		{
			if (_tapIconInstance == null)
			{
				_tapIconInstance = _unityObjectPool.PopObject(_tapIconPrefab);
				_tapIconInstance.transform.parent = _iconRoot.transform;
				_tapIconInstance.transform.localPosition = _tapIconPrefab.transform.localPosition;
				_tapIconInstance.transform.localRotation = _tapIconPrefab.transform.localRotation;
				_tapIconInstance.transform.localScale = _tapIconPrefab.transform.localScale;
			}
		}
		else if (_tapIconInstance != null)
		{
			_unityObjectPool.PushObject(_tapIconInstance);
			_tapIconInstance = null;
		}
	}

	private static void TryAddCardBadges(AssetLookupSystem assetLookupSystem, IList<BadgeEntrySpawnData> badgeSpawnDatas, ICardDataAdapter cardModel, CardHolderType cardHolderType, CDCViewMetadata viewMetadata, MtgGameState mtgGameState, WorkflowBase activeInteraction)
	{
		if (!assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AssetLookupTree.Payloads.Card.BadgeEntry> loadedTree) || !assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<BadgeViewPrinted> loadedTree2))
		{
			return;
		}
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardModel);
		assetLookupSystem.Blackboard.SetCdcViewMetadata(viewMetadata);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		assetLookupSystem.Blackboard.GameState = mtgGameState;
		assetLookupSystem.Blackboard.Interaction = activeInteraction;
		HashSet<AssetLookupTree.Payloads.Card.BadgeEntry> hashSet = new HashSet<AssetLookupTree.Payloads.Card.BadgeEntry>();
		if (!loadedTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet))
		{
			return;
		}
		foreach (AssetLookupTree.Payloads.Card.BadgeEntry item in hashSet)
		{
			IBadgeEntryData data = item.Data;
			if (data != null && data.ValidOnTTP)
			{
				assetLookupSystem.Blackboard.BadgeData = data;
				BadgeViewPrinted payload = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
				if (payload != null && payload.BadgeEntryView?.RelativePath != null)
				{
					BadgeEntryStatus badgeEntryStatus = BadgeEntryStatus.Normal;
					BadgeEntryViewCreator badgeEntryView = new BadgeEntryViewCreator(payload.BadgeEntryView.RelativePath);
					badgeSpawnDatas.Add(new BadgeEntrySpawnData(data, badgeEntryStatus, null, badgeEntryView));
				}
			}
		}
	}

	public static void TryAddAbilityBadge(AssetLookupSystem assetLookupSystem, IList<BadgeEntrySpawnData> badgeSpawnDatas, AbilityPrintingData abilityPrintingData, ICardDataAdapter cardModel, CardHolderType cardHolderType, CDCViewMetadata viewMetadata, MtgGameState mtgGameState, WorkflowBase activeInteraction)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardModel);
		assetLookupSystem.Blackboard.SetCdcViewMetadata(viewMetadata);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		assetLookupSystem.Blackboard.GameState = mtgGameState;
		assetLookupSystem.Blackboard.Interaction = activeInteraction;
		assetLookupSystem.Blackboard.Ability = abilityPrintingData;
		HashSet<AssetLookupTree.Payloads.Ability.BadgeEntry> hashSet = new HashSet<AssetLookupTree.Payloads.Ability.BadgeEntry>();
		AssetLookupTree<AssetLookupTree.Payloads.Ability.BadgeEntry> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<AssetLookupTree.Payloads.Ability.BadgeEntry>();
		if (assetLookupTree == null || !assetLookupTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet))
		{
			return;
		}
		foreach (AssetLookupTree.Payloads.Ability.BadgeEntry item in hashSet)
		{
			if (item.Data == null || !item.Data.ValidOnTTP)
			{
				continue;
			}
			assetLookupSystem.Blackboard.BadgeData = item.Data;
			AssetLookupTree<BadgeViewPrinted> assetLookupTree2 = assetLookupSystem.TreeLoader.LoadTree<BadgeViewPrinted>();
			if (assetLookupTree2 != null)
			{
				BadgeViewPrinted payload = assetLookupTree2.GetPayload(assetLookupSystem.Blackboard);
				if (payload != null && payload.BadgeEntryView?.RelativePath != null)
				{
					BadgeEntryStatus badgeEntryStatus = BadgeEntryStatus.Normal;
					BadgeEntryViewCreator badgeEntryView = new BadgeEntryViewCreator(payload.BadgeEntryView.RelativePath);
					badgeSpawnDatas.Add(new BadgeEntrySpawnData(item.Data, badgeEntryStatus, abilityPrintingData, badgeEntryView));
				}
			}
		}
	}

	public static void SortBadges(List<BadgeEntrySpawnData> badgeSpawnDatas)
	{
		badgeSpawnDatas.Sort(delegate(BadgeEntrySpawnData x, BadgeEntrySpawnData y)
		{
			int num = x.BadgeEntryData.CompareTo(y.BadgeEntryData);
			return (num != 0) ? num : x.BadgeEntryStatus.CompareTo(y.BadgeEntryStatus);
		});
	}

	public static void DedupeBadges(List<BadgeEntrySpawnData> badgeSpawnDatas)
	{
		for (int i = 0; i < badgeSpawnDatas.Count; i++)
		{
			IBadgeEntryData badgeEntryData = badgeSpawnDatas[i].BadgeEntryData;
			for (int j = i + 1; j < badgeSpawnDatas.Count; j++)
			{
				IBadgeEntryData badgeEntryData2 = badgeSpawnDatas[j].BadgeEntryData;
				if (badgeEntryData.Equals(badgeEntryData2))
				{
					badgeSpawnDatas.RemoveAt(j);
					j--;
				}
			}
		}
	}
}
