using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Universal;

public class UniversalBattlefieldLayout : ICardLayout
{
	private class CompareByApnapOrder : IComparer<GREPlayerNum>
	{
		public Func<MtgGameState> GetCurrentGameState { private get; set; }

		public int Compare(GREPlayerNum x, GREPlayerNum y)
		{
			GREPlayerNum valueOrDefault = (GetCurrentGameState?.Invoke()?.ActivePlayer?.ClientPlayerEnum).GetValueOrDefault();
			bool value = x == valueOrDefault;
			return (y == valueOrDefault).CompareTo(value);
		}
	}

	private IReadOnlyList<UniversalBattlefieldRegion> _regions;

	private readonly GameManager _gameManager;

	private readonly GameObject _stackUiPrefab;

	private Vector3 _stackUiOffset;

	private readonly List<UniversalBattlefieldStackUi> _stackUis = new List<UniversalBattlefieldStackUi>();

	private readonly Dictionary<DuelScene_CDC, (IUniversalBattlefieldGroup, IUniversalBattlefieldGroup)> _intraGroupChanges = new Dictionary<DuelScene_CDC, (IUniversalBattlefieldGroup, IUniversalBattlefieldGroup)>();

	private readonly CompareByApnapOrder _regionOrderComparer = new CompareByApnapOrder();

	private readonly IUnityObjectPool _unityPool;

	public IReadOnlyDictionary<DuelScene_CDC, (IUniversalBattlefieldGroup From, IUniversalBattlefieldGroup To)> IntraGroupChanges => _intraGroupChanges;

	public IReadOnlyCollection<uint> FocusPlayerIds { private get; set; } = (IReadOnlyCollection<uint>)(object)Array.Empty<uint>();

	public UniversalBattlefieldLayout(IReadOnlyList<UniversalBattlefieldRegion> regions, GameManager gameManager, GameObject stackUiPrefab, Vector3 stackUiOffset)
	{
		_regions = regions;
		_gameManager = gameManager;
		_stackUiPrefab = stackUiPrefab;
		_stackUiOffset = stackUiOffset;
		_regionOrderComparer.GetCurrentGameState = _gameManager.GetCurrentGameState;
		_unityPool = PlatformContext.CreateUnityPool("BattlefieldLayout", keepAlive: false, null, gameManager.SplineMovementSystem);
	}

	public void Clear()
	{
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			region.Clear(_gameManager.GenericPool);
		}
		foreach (UniversalBattlefieldStackUi stackUi in _stackUis)
		{
			if ((bool)stackUi)
			{
				stackUi.Clear(_unityPool);
				_unityPool.PushObject(stackUi.gameObject, worldPositionStays: false);
			}
		}
		_stackUis.Clear();
		_intraGroupChanges.Clear();
	}

	public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		Dictionary<DuelScene_CDC, IUniversalBattlefieldGroup> dictionary = _gameManager.GenericPool.PopObject<Dictionary<DuelScene_CDC, IUniversalBattlefieldGroup>>();
		foreach (IUniversalBattlefieldGroup item in _regions.SelectMany((UniversalBattlefieldRegion region) => region.AllGroups))
		{
			foreach (DuelScene_CDC item2 in item.AllStacks.SelectMany((UniversalBattlefieldStack stack) => stack.AllCards))
			{
				dictionary.Add(item2, item);
			}
		}
		Clear();
		Dictionary<DuelScene_CDC, (DuelScene_CDC, ICardDataAdapter)> dictionary2 = _gameManager.GenericPool.PopObject<Dictionary<DuelScene_CDC, (DuelScene_CDC, ICardDataAdapter)>>();
		GenerateAttachmentMap(allCardViews, dictionary2, _gameManager.CurrentGameState, _gameManager.BrowserManager.CurrentBrowser, _gameManager.CardDatabase);
		HashSet<uint> hashSet = _gameManager.GenericPool.PopObject<HashSet<uint>>();
		foreach (DuelScene_CDC allCardView in allCardViews)
		{
			bool isFocusPlayer = allCardView.Model.Controller.IsLocalPlayer || FocusPlayerIds.Contains(allCardView.Model.Controller.InstanceId);
			PlaceInRegion(allCardView, dictionary2, hashSet, _regions, _gameManager, isFocusPlayer);
		}
		foreach (UniversalBattlefieldRegion item3 in _regions.OrderBy((UniversalBattlefieldRegion region) => region.Controller, _regionOrderComparer))
		{
			allData.AddRange(item3.GenerateLayoutDatas(_gameManager.CurrentGameState, _gameManager.CurrentInteraction, allData));
		}
		GenerateStackUis(_regions, _unityPool, _stackUiPrefab, _stackUiOffset, _stackUis);
		foreach (IUniversalBattlefieldGroup item4 in _regions.SelectMany((UniversalBattlefieldRegion region) => region.AllGroups))
		{
			foreach (DuelScene_CDC item5 in item4.AllStacks.SelectMany((UniversalBattlefieldStack stack) => stack.AllCards))
			{
				if (dictionary.TryGetValue(item5, out var value))
				{
					if (value != item4)
					{
						_intraGroupChanges.Add(item5, (value, item4));
					}
					dictionary.Remove(item5);
				}
			}
		}
		foreach (KeyValuePair<DuelScene_CDC, IUniversalBattlefieldGroup> item6 in dictionary)
		{
			_intraGroupChanges.Add(item6.Key, (item6.Value, null));
		}
		_gameManager.GenericPool.PushObject(dictionary2);
		_gameManager.GenericPool.PushObject(hashSet);
		_gameManager.GenericPool.PushObject(dictionary);
	}

	private static void GenerateAttachmentMap(List<DuelScene_CDC> allCardViews, Dictionary<DuelScene_CDC, (DuelScene_CDC, ICardDataAdapter)> attachmentMap, MtgGameState gameState, BrowserBase browser, ICardDatabaseAdapter cardDb)
	{
		foreach (DuelScene_CDC cardView in allCardViews)
		{
			if (cardView.Model.Instance.AttachedToId == 0)
			{
				continue;
			}
			DuelScene_CDC duelScene_CDC = allCardViews.FirstOrDefault((DuelScene_CDC x) => x.InstanceId == cardView.Model.Instance.AttachedToId);
			if ((object)duelScene_CDC != null)
			{
				attachmentMap[cardView] = (duelScene_CDC, duelScene_CDC.Model);
				continue;
			}
			MtgCardInstance mtgCardInstance = gameState?.Limbo?.VisibleCards.FirstOrDefault((MtgCardInstance x) => x.InstanceId == cardView.Model.Instance.AttachedToId);
			if (mtgCardInstance != null)
			{
				attachmentMap[cardView] = (null, CardDataExtensions.CreateWithDatabase(mtgCardInstance, cardDb));
			}
			else if (browser is CardBrowserBase cardBrowserBase)
			{
				DuelScene_CDC duelScene_CDC2 = cardBrowserBase.GetCardViews().FirstOrDefault((DuelScene_CDC x) => x.InstanceId == cardView.Model.Instance.AttachedToId);
				if ((object)duelScene_CDC2 != null)
				{
					attachmentMap[cardView] = (null, CardDataExtensions.CreateWithDatabase(duelScene_CDC2.Model.Instance, cardDb));
				}
			}
		}
	}

	private static void PlaceInRegion(DuelScene_CDC cardView, Dictionary<DuelScene_CDC, (DuelScene_CDC view, ICardDataAdapter model)> attachmentMap, HashSet<uint> placedCardIds, IEnumerable<UniversalBattlefieldRegion> regions, GameManager gameManager, bool isFocusPlayer)
	{
		if (attachmentMap.TryGetValue(cardView, out (DuelScene_CDC, ICardDataAdapter) value))
		{
			if ((bool)value.Item1)
			{
				PlaceInRegion(value.Item1, attachmentMap, placedCardIds, regions, gameManager, isFocusPlayer);
			}
			else
			{
				PlaceInRegion(value.Item2, placedCardIds, regions, gameManager, isFocusPlayer);
			}
		}
		if (placedCardIds.Contains(cardView.InstanceId))
		{
			return;
		}
		bool flag = false;
		foreach (UniversalBattlefieldRegion region in regions)
		{
			if (region.TryAddCard(cardView, gameManager, isFocusPlayer))
			{
				placedCardIds.Add(cardView.InstanceId);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SimpleLog.LogPreProdError("could not find region for card " + cardView.name);
		}
	}

	private static void PlaceInRegion(ICardDataAdapter cardModel, HashSet<uint> placedCardIds, IEnumerable<UniversalBattlefieldRegion> regions, GameManager gameManager, bool isFocusPlayer)
	{
		if (placedCardIds.Contains(cardModel.InstanceId))
		{
			return;
		}
		bool flag = false;
		foreach (UniversalBattlefieldRegion region in regions)
		{
			if (region.TryAddCard(cardModel, gameManager, isFocusPlayer))
			{
				placedCardIds.Add(cardModel.InstanceId);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SimpleLog.LogPreProdError($"could not find region for card #{cardModel.InstanceId}");
		}
	}

	private static void GenerateStackUis(IEnumerable<UniversalBattlefieldRegion> regions, IUnityObjectPool pool, GameObject stackUiPrefab, Vector3 posOffset, List<UniversalBattlefieldStackUi> stackUis)
	{
		foreach (UniversalBattlefieldStack item in regions.SelectMany((UniversalBattlefieldRegion region) => region.AllGroups).SelectMany((IUniversalBattlefieldGroup group) => group.VisibleStacks))
		{
			DuelScene_CDC stackParent = item.StackParent;
			if ((object)stackParent == null)
			{
				continue;
			}
			Transform partsRoot = stackParent.PartsRoot;
			if ((object)partsRoot == null || item.StackParentModel == null)
			{
				continue;
			}
			UniversalBattlefieldStackUi universalBattlefieldStackUi = null;
			int attachmentCount = item.AttachmentCount;
			int exileCount = item.ExileCount;
			int num = attachmentCount + exileCount;
			if (num > 1 || item.StackParentModel.Instance.AttachedWithIds.Count > 0)
			{
				universalBattlefieldStackUi = getOrCreateStackUi(posOffset, universalBattlefieldStackUi, stackUis, stackUiPrefab, pool, partsRoot);
				universalBattlefieldStackUi.SetExpandEnabled(pool);
			}
			if (num > 0)
			{
				if (num >= 4 && attachmentCount > 0 && (bool)partsRoot)
				{
					universalBattlefieldStackUi = getOrCreateStackUi(posOffset, universalBattlefieldStackUi, stackUis, stackUiPrefab, pool, partsRoot);
					universalBattlefieldStackUi.AddCount(attachmentCount, pool);
				}
				if (num >= 4 && exileCount > 0 && (bool)partsRoot)
				{
					universalBattlefieldStackUi = getOrCreateStackUi(posOffset, universalBattlefieldStackUi, stackUis, stackUiPrefab, pool, partsRoot);
					universalBattlefieldStackUi.AddCount(exileCount, pool);
				}
			}
			else if (item.StackCount.HasValue && (bool)partsRoot)
			{
				universalBattlefieldStackUi = getOrCreateStackUi(posOffset, universalBattlefieldStackUi, stackUis, stackUiPrefab, pool, partsRoot);
				universalBattlefieldStackUi.AddCount(item.StackCount.Value, pool);
			}
		}
		static UniversalBattlefieldStackUi getOrCreateStackUi(Vector3 localPosition, UniversalBattlefieldStackUi stackUi, List<UniversalBattlefieldStackUi> list, GameObject prefab, IUnityObjectPool unityObjectPool, Transform parent)
		{
			if (stackUi == null)
			{
				stackUi = unityObjectPool.PopObject(prefab, parent).GetComponent<UniversalBattlefieldStackUi>();
				stackUi.transform.localPosition = localPosition;
				list.Add(stackUi);
			}
			return stackUi;
		}
	}

	public bool HandleCardClick(DuelScene_CDC cardView, WorkflowBase currentWorkflow, out bool layoutStale)
	{
		layoutStale = false;
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			bool layoutStale2;
			bool num = region.HandleCardClick(cardView, currentWorkflow, out layoutStale2);
			layoutStale |= layoutStale2;
			if (num)
			{
				return true;
			}
		}
		return false;
	}

	public bool HandleCardDragBegin(DuelScene_CDC cardView, Vector2 pointerPos)
	{
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			if (region.HandleCardDragBegin(cardView, pointerPos))
			{
				return true;
			}
		}
		return false;
	}

	public bool HandleCardDragSustain(DuelScene_CDC cardView, Vector2 pointerPos, float dragScale, out IEnumerable<CardLayoutData> cardLayoutDatas)
	{
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			if (region.HandleCardDragSustain(cardView, pointerPos, dragScale, out cardLayoutDatas))
			{
				return true;
			}
		}
		cardLayoutDatas = Enumerable.Empty<CardLayoutData>();
		return false;
	}

	public bool HandleCardDragEnd(DuelScene_CDC cardView)
	{
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			if (region.HandleCardDragEnd(cardView))
			{
				return true;
			}
		}
		return false;
	}

	public bool HandlePhaseChange()
	{
		bool flag = false;
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			flag |= region.HandlePhaseChange();
		}
		return flag;
	}
}
