using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Universal;

[Serializable]
public class UniversalBattlefieldRegion : IDisposable
{
	[Serializable]
	public class BoundsDefinition
	{
		[Tooltip("Space between groups in this region")]
		public float SpacingX;

		[Tooltip("lower-left corner of the region in viewport space [0,1]")]
		public Vector2 AnchorMin;

		[Tooltip("upper-right corner of the region in viewport space [0,1]")]
		public Vector2 AnchorMax;

		private Bounds? _bounds;

		public Bounds Bounds
		{
			get
			{
				if (!_bounds.HasValue)
				{
					throw new InvalidOperationException("Bounds are null. Call CalculateBounds first.");
				}
				return _bounds.Value;
			}
		}

		public void CalculateBounds(Camera camera)
		{
			float num = (Screen.safeArea.xMin + AnchorMin.x * Screen.safeArea.width) / (float)Screen.width;
			float num2 = (Screen.safeArea.xMin + AnchorMax.x * Screen.safeArea.width) / (float)Screen.width;
			Plane plane = new Plane(Vector3.up, Vector3.zero);
			Vector3 vector = CalculateWorldspacePositionFromViewportCoords(plane, new Vector2(num, AnchorMin.y), camera);
			Vector3 vector2 = CalculateWorldspacePositionFromViewportCoords(plane, new Vector2(num2, AnchorMax.y), camera);
			float x = (num + num2) / 2f;
			float y = (AnchorMin.y + AnchorMax.y) / 2f;
			Vector3 center = CalculateWorldspacePositionFromViewportCoords(plane, new Vector2(x, y), camera);
			_bounds = new Bounds(center, new Vector3(vector.x - vector2.x, vector.y - vector2.y, vector.z - vector2.z));
		}

		private Vector3 CalculateWorldspacePositionFromViewportCoords(Plane plane, Vector2 screenPos01, Camera camera)
		{
			Ray ray = camera.ViewportPointToRay(screenPos01, Camera.MonoOrStereoscopicEye.Mono);
			if (plane.Raycast(ray, out var enter))
			{
				return ray.GetPoint(enter);
			}
			return Vector3.zero;
		}
	}

	[SerializeField]
	private string _name;

	[SerializeField]
	private GREPlayerNum _battlefieldSide;

	[SerializeField]
	private Transform _battlefieldCardHolder;

	[SerializeField]
	private bool _drawBounds;

	[SerializeField]
	private BoundsDefinition _boundsDefinition;

	[SerializeField]
	private List<UniversalBattlefieldGroup.Configuration> _groupConfigs;

	[SerializeField]
	private int _maxOverlapIterations = 20;

	private List<IUniversalBattlefieldGroup> _groups = new List<IUniversalBattlefieldGroup>();

	private Transform _transform;

	private readonly HashSet<IUniversalBattlefieldGroup> _collapsedGroups = new HashSet<IUniversalBattlefieldGroup>();

	private uint _expandedStackParentId;

	private DuelScene_CDC _scrollCard;

	private float? _touchPointX;

	private float _scrollOffsetX;

	private IObjectPool _pool;

	public IEnumerable<IUniversalBattlefieldGroup> AllGroups => _groups;

	public Transform Transform
	{
		get
		{
			if (!_transform)
			{
				_transform = new GameObject("Region Locator \"" + _name + "\"").transform;
				_transform.SetParent(_battlefieldCardHolder, worldPositionStays: false);
			}
			return _transform;
		}
	}

	public GREPlayerNum Controller => _battlefieldSide;

	public void Init(Vector3 tappedRotation, Vector3 declaredAttackOffset, ICardViewProvider cardViewProvider, IObjectPool pool, Func<GREPlayerNum, MatchManager.PlayerInfo> playerInfoProvider, AssetLookupSystem assetLookupSystem, CardHolderManager cardHolderManager, IEqualityComparer<DuelScene_CDC> canStackComparer)
	{
		_pool = pool;
		_boundsDefinition.CalculateBounds(Camera.main);
		ScreenEventController.Instance.OnScreenChanged += HandleScreenChange;
		foreach (UniversalBattlefieldGroup.Configuration groupConfig in _groupConfigs)
		{
			switch (groupConfig.GroupType)
			{
			case UniversalBattlefieldGroup.GroupType.Card:
				_groups.Add(new UniversalBattlefieldGroup(groupConfig, tappedRotation, declaredAttackOffset, cardViewProvider, pool, canStackComparer));
				break;
			case UniversalBattlefieldGroup.GroupType.Pet:
				_groups.Add(new UniversalBattlefieldPet(groupConfig, playerInfoProvider, assetLookupSystem));
				break;
			case UniversalBattlefieldGroup.GroupType.Command:
				_groups.Add(new UniversalBattlefieldGroupCommand(groupConfig, cardHolderManager));
				break;
			case UniversalBattlefieldGroup.GroupType.Prompted:
				_groups.Add(new UniversalBattlefieldGroupPrompted(groupConfig));
				break;
			case UniversalBattlefieldGroup.GroupType.Spacer:
				_groups.Add(new UniversalBattlefieldGroupSpacer(groupConfig));
				break;
			}
		}
	}

	public void Dispose()
	{
		if (ScreenEventController.Instance != null)
		{
			ScreenEventController.Instance.OnScreenChanged -= HandleScreenChange;
		}
	}

	public bool CardIsValid(ICardDataAdapter cardModel, IReadOnlyList<MtgPlayer> players)
	{
		foreach (IUniversalBattlefieldGroup group in _groups)
		{
			if (group.Config.CardIsValid(cardModel, players))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryAddCard(DuelScene_CDC cardView, GameManager gameManager, bool isFocusPlayer)
	{
		for (int i = 0; i < _groups.Count; i++)
		{
			if (_groups[i].TryAddCard(cardView, gameManager, isFocusPlayer))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryAddCard(ICardDataAdapter cardModel, GameManager gameManager, bool isFocusPlayer)
	{
		for (int i = 0; i < _groups.Count; i++)
		{
			if (_groups[i].TryAddCard(cardModel, gameManager, isFocusPlayer))
			{
				return true;
			}
		}
		return false;
	}

	public void Clear(IObjectPool pool)
	{
		foreach (IUniversalBattlefieldGroup group in _groups)
		{
			group.Clear(pool);
		}
	}

	public bool HandleCardClick(DuelScene_CDC cardView, WorkflowBase currentInteraction, out bool layoutStale)
	{
		layoutStale = false;
		if (currentInteraction is DeclareAttackersWorkflow || currentInteraction is DeclareBlockersWorkflow)
		{
			return false;
		}
		foreach (IUniversalBattlefieldGroup group in _groups)
		{
			UniversalBattlefieldStack universalBattlefieldStack = group.AllStacks.Find(cardView, (UniversalBattlefieldStack stack, DuelScene_CDC item) => stack.AllCards.Contains(item));
			if (universalBattlefieldStack == null)
			{
				continue;
			}
			if (_collapsedGroups.Contains(group))
			{
				_collapsedGroups.UnionWith(_groups.Where((IUniversalBattlefieldGroup group) => group.Config.Collapsible));
				_collapsedGroups.Remove(group);
				_touchPointX = null;
				_scrollCard = null;
				layoutStale = true;
				return true;
			}
			UniversalBattlefieldStack universalBattlefieldStack2 = group.VisibleStacks.Find(cardView, (UniversalBattlefieldStack stack, DuelScene_CDC item) => stack.AllCards.Contains(item));
			IEnumerable<DuelScene_CDC> enumerable = (currentInteraction as ITargetCDCListProviderWorkflow)?.GetTargetCDCs();
			IEnumerable<DuelScene_CDC> second = enumerable ?? Enumerable.Empty<DuelScene_CDC>();
			IEnumerable<DuelScene_CDC> source = universalBattlefieldStack2.AllCards.Intersect(second);
			if (source.Contains(cardView))
			{
				if (source.Count() == 1)
				{
					_expandedStackParentId = 0u;
					layoutStale = true;
					return false;
				}
				if (universalBattlefieldStack2.HasAttachmentOrExile)
				{
					_expandedStackParentId = universalBattlefieldStack.StackParentModel.InstanceId;
					layoutStale = true;
					return true;
				}
				_expandedStackParentId = 0u;
				layoutStale = true;
				return false;
			}
			if (universalBattlefieldStack.StackParentModel.InstanceId != _expandedStackParentId)
			{
				if (universalBattlefieldStack2.HasAttachmentOrExile)
				{
					_expandedStackParentId = universalBattlefieldStack.StackParentModel.InstanceId;
					layoutStale = true;
					return true;
				}
				_expandedStackParentId = 0u;
				layoutStale = true;
				return false;
			}
			_expandedStackParentId = 0u;
			layoutStale = true;
			return false;
		}
		return false;
	}

	public bool HandleCardDragBegin(DuelScene_CDC cardView, Vector2 pointerPos)
	{
		if (_groups.SelectMany((IUniversalBattlefieldGroup group) => group.VisibleStacks.SelectMany((UniversalBattlefieldStack stack) => stack.AllCards)).Contains(cardView))
		{
			_touchPointX = pointerPos.x;
			_scrollCard = cardView;
			return true;
		}
		return false;
	}

	public bool HandleCardDragSustain(DuelScene_CDC cardView, Vector2 pointerPos, float scrollScale, out IEnumerable<CardLayoutData> cardLayoutDatas)
	{
		if (cardView == _scrollCard)
		{
			float num = (pointerPos.x - _touchPointX.Value) * scrollScale;
			_scrollOffsetX += num;
			_touchPointX = pointerPos.x;
			foreach (IUniversalBattlefieldGroup group in _groups)
			{
				group.ApplyPositionalDelta(CdcVector3.right * num);
			}
			cardLayoutDatas = _groups.SelectMany((IUniversalBattlefieldGroup group) => group.CardLayoutDatas);
			return true;
		}
		cardLayoutDatas = null;
		return false;
	}

	public bool HandleCardDragEnd(DuelScene_CDC cardView)
	{
		if (cardView == _scrollCard)
		{
			ClampScrollOffsetX();
			_touchPointX = null;
			_scrollCard = null;
			return true;
		}
		return false;
	}

	public bool HandlePhaseChange()
	{
		if (_expandedStackParentId != 0)
		{
			_expandedStackParentId = 0u;
			return true;
		}
		return false;
	}

	private void HandleScreenChange()
	{
		_boundsDefinition.CalculateBounds(Camera.main);
	}

	public IEnumerable<CardLayoutData> GenerateLayoutDatas(MtgGameState gameState, WorkflowBase workflow, IReadOnlyCollection<CardLayoutData> solvedLayoutDatas)
	{
		float num = 0f;
		BoundsDefinition boundsDefinition = _boundsDefinition;
		Transform.position = Transform.parent.position + new Vector3(boundsDefinition.Bounds.center.x, boundsDefinition.Bounds.center.y, boundsDefinition.Bounds.center.z);
		IOrderedEnumerable<IUniversalBattlefieldGroup> orderedEnumerable = _groups.OrderBy((IUniversalBattlefieldGroup a) => a.Config.AnchorX);
		bool flag = true;
		foreach (IUniversalBattlefieldGroup group in _groups)
		{
			group.GenerateLayoutDatas(gameState, workflow, solvedLayoutDatas, collapse: false, _expandedStackParentId);
			if (group.Dimensions.x != 0f)
			{
				if (!flag && !group.IgnoreRegionSpacing)
				{
					num += boundsDefinition.SpacingX;
				}
				flag = group.IgnoreRegionSpacing;
				num += group.Dimensions.x;
			}
		}
		if (num > boundsDefinition.Bounds.size.x && _groups.Any((IUniversalBattlefieldGroup group) => group.Config.Collapsible))
		{
			if (_collapsedGroups.Count == 0)
			{
				IUniversalBattlefieldGroup item = null;
				uint num2 = 0u;
				foreach (IUniversalBattlefieldGroup group2 in _groups)
				{
					if (group2.Config.GroupType == UniversalBattlefieldGroup.GroupType.Card && group2.AllStacks.Count() != 0)
					{
						uint num3 = group2.AllStacks.SelectMany((UniversalBattlefieldStack stack) => stack.AllCards).Max((DuelScene_CDC card) => card.InstanceId);
						if (num3 > num2)
						{
							num2 = num3;
							item = group2;
						}
					}
				}
				_collapsedGroups.UnionWith(_groups.Where((IUniversalBattlefieldGroup group) => group.Config.Collapsible));
				_collapsedGroups.Remove(item);
			}
			num = 0f;
			flag = true;
			foreach (IUniversalBattlefieldGroup group3 in _groups)
			{
				bool collapse = _collapsedGroups.Contains(group3);
				group3.GenerateLayoutDatas(gameState, workflow, solvedLayoutDatas, collapse, _expandedStackParentId);
				if (group3.Dimensions.x != 0f)
				{
					if (!flag && !group3.IgnoreRegionSpacing)
					{
						num += boundsDefinition.SpacingX;
					}
					flag = group3.IgnoreRegionSpacing;
					num += group3.Dimensions.x;
				}
			}
		}
		else
		{
			_collapsedGroups.Clear();
		}
		if (num > boundsDefinition.Bounds.size.x)
		{
			Vector3 posDelta = _battlefieldCardHolder.position + boundsDefinition.Bounds.center + CdcVector3.left * num / 2f + CdcVector3.right * _scrollOffsetX;
			flag = true;
			foreach (IUniversalBattlefieldGroup item2 in orderedEnumerable)
			{
				if (item2.Dimensions.x != 0f)
				{
					if (!flag && !item2.IgnoreRegionSpacing)
					{
						posDelta += CdcVector3.right * boundsDefinition.SpacingX;
					}
					flag = item2.IgnoreRegionSpacing;
					item2.ApplyPositionalDelta(posDelta);
					posDelta += CdcVector3.right * item2.Dimensions.x;
				}
			}
		}
		else
		{
			Dictionary<IUniversalBattlefieldGroup, float> dictionary = _pool.PopObject<Dictionary<IUniversalBattlefieldGroup, float>>();
			AdjustGroupAnchorsToEliminateOverlap(boundsDefinition, orderedEnumerable, dictionary, _maxOverlapIterations, _pool);
			foreach (IUniversalBattlefieldGroup item3 in orderedEnumerable)
			{
				Vector3 posDelta2 = _battlefieldCardHolder.position + boundsDefinition.Bounds.center + CdcVector3.left * boundsDefinition.Bounds.extents.x + CdcVector3.right * boundsDefinition.Bounds.size.x * dictionary[item3] + CdcVector3.left * item3.Dimensions.x / 2f + CdcVector3.right * _scrollOffsetX;
				item3.ApplyPositionalDelta(posDelta2);
			}
			_pool.PushObject(dictionary);
		}
		if (_scrollCard == null)
		{
			ClampScrollOffsetX();
		}
		return _groups.SelectMany((IUniversalBattlefieldGroup group) => group.CardLayoutDatas);
	}

	private static void AdjustGroupAnchorsToEliminateOverlap(BoundsDefinition boundsDef, IEnumerable<IUniversalBattlefieldGroup> sortedGroups, Dictionary<IUniversalBattlefieldGroup, float> adjustedAnchorsX, int maxIterations, IObjectPool pool)
	{
		HashSet<IUniversalBattlefieldGroup> hashSet = pool.PopObject<HashSet<IUniversalBattlefieldGroup>>();
		foreach (IUniversalBattlefieldGroup sortedGroup in sortedGroups)
		{
			adjustedAnchorsX.Add(sortedGroup, sortedGroup.Config.AnchorX);
		}
		sortedGroups = sortedGroups.Where((IUniversalBattlefieldGroup group) => group.Dimensions.x > 0f);
		UniversalBattlefieldGroupSentinel universalBattlefieldGroupSentinel = pool.PopObject<UniversalBattlefieldGroupSentinel>();
		UniversalBattlefieldGroupSentinel universalBattlefieldGroupSentinel2 = pool.PopObject<UniversalBattlefieldGroupSentinel>();
		adjustedAnchorsX.Add(universalBattlefieldGroupSentinel, 0f);
		adjustedAnchorsX.Add(universalBattlefieldGroupSentinel2, 1f);
		hashSet.Add(universalBattlefieldGroupSentinel);
		hashSet.Add(universalBattlefieldGroupSentinel2);
		sortedGroups = sortedGroups.Prepend(universalBattlefieldGroupSentinel).Append(universalBattlefieldGroupSentinel2);
		int num = 0;
		bool flag;
		do
		{
			flag = false;
			float minX3;
			if (num % 2 == 0)
			{
				for (int num2 = 0; num2 < sortedGroups.Count() - 1; num2++)
				{
					IUniversalBattlefieldGroup universalBattlefieldGroup = sortedGroups.ElementAt(num2);
					IUniversalBattlefieldGroup universalBattlefieldGroup2 = sortedGroups.ElementAt(num2 + 1);
					getGroupMinMaxX(universalBattlefieldGroup, adjustedAnchorsX, boundsDef, out var minX, out var maxX);
					getGroupMinMaxX(universalBattlefieldGroup2, adjustedAnchorsX, boundsDef, out var minX2, out minX3);
					float num3 = ((universalBattlefieldGroup.IgnoreRegionSpacing || universalBattlefieldGroup2.IgnoreRegionSpacing) ? 0f : boundsDef.SpacingX) - minX2 + maxX;
					if (num3 < 0f || approximately(num3, 0f))
					{
						continue;
					}
					flag = true;
					if (hashSet.Contains(universalBattlefieldGroup))
					{
						moveAndLockGroupB(universalBattlefieldGroup, universalBattlefieldGroup2, adjustedAnchorsX, boundsDef, hashSet);
						continue;
					}
					if (hashSet.Contains(universalBattlefieldGroup2))
					{
						moveAndLockGroupA(universalBattlefieldGroup, universalBattlefieldGroup2, adjustedAnchorsX, boundsDef, hashSet);
						continue;
					}
					if (num2 - 1 >= 0)
					{
						IUniversalBattlefieldGroup universalBattlefieldGroup3 = sortedGroups.ElementAt(num2 - 1);
						if (universalBattlefieldGroup3 != null && hashSet.Contains(universalBattlefieldGroup3) && getGroupMinMaxX(universalBattlefieldGroup3, adjustedAnchorsX, boundsDef, out minX3, out var maxX2) && minX < maxX2 + boundsDef.SpacingX)
						{
							adjustedAnchorsX[universalBattlefieldGroup] = (maxX2 + boundsDef.SpacingX + universalBattlefieldGroup.Dimensions.x / 2f) / boundsDef.Bounds.size.x;
							hashSet.Add(universalBattlefieldGroup);
							adjustedAnchorsX[universalBattlefieldGroup2] = (adjustedAnchorsX[universalBattlefieldGroup] * boundsDef.Bounds.size.x + universalBattlefieldGroup.Dimensions.x / 2f + boundsDef.SpacingX + universalBattlefieldGroup2.Dimensions.x / 2f) / boundsDef.Bounds.size.x;
							hashSet.Add(universalBattlefieldGroup);
							continue;
						}
					}
					splitTheOverlap(universalBattlefieldGroup, universalBattlefieldGroup2, adjustedAnchorsX, boundsDef, num3);
				}
				continue;
			}
			for (int num4 = sortedGroups.Count() - 1; num4 > 0; num4--)
			{
				IUniversalBattlefieldGroup universalBattlefieldGroup4 = sortedGroups.ElementAt(num4);
				IUniversalBattlefieldGroup universalBattlefieldGroup5 = sortedGroups.ElementAt(num4 - 1);
				getGroupMinMaxX(universalBattlefieldGroup4, adjustedAnchorsX, boundsDef, out var minX4, out var maxX3);
				getGroupMinMaxX(universalBattlefieldGroup5, adjustedAnchorsX, boundsDef, out minX3, out var maxX4);
				float num5 = ((universalBattlefieldGroup5.IgnoreRegionSpacing || universalBattlefieldGroup4.IgnoreRegionSpacing) ? 0f : boundsDef.SpacingX) - minX4 + maxX4;
				if (num5 < 0f || approximately(num5, 0f))
				{
					continue;
				}
				flag = true;
				if (hashSet.Contains(universalBattlefieldGroup5))
				{
					moveAndLockGroupB(universalBattlefieldGroup5, universalBattlefieldGroup4, adjustedAnchorsX, boundsDef, hashSet);
					continue;
				}
				if (hashSet.Contains(universalBattlefieldGroup4))
				{
					moveAndLockGroupA(universalBattlefieldGroup5, universalBattlefieldGroup4, adjustedAnchorsX, boundsDef, hashSet);
					continue;
				}
				if (num4 + 1 < sortedGroups.Count())
				{
					IUniversalBattlefieldGroup universalBattlefieldGroup6 = sortedGroups.ElementAt(num4 + 1);
					if (universalBattlefieldGroup6 != null && hashSet.Contains(universalBattlefieldGroup6) && getGroupMinMaxX(universalBattlefieldGroup6, adjustedAnchorsX, boundsDef, out var minX5, out minX3) && maxX3 > minX5 - boundsDef.SpacingX)
					{
						adjustedAnchorsX[universalBattlefieldGroup4] = (minX5 - boundsDef.SpacingX - universalBattlefieldGroup4.Dimensions.x / 2f) / boundsDef.Bounds.size.x;
						hashSet.Add(universalBattlefieldGroup4);
						adjustedAnchorsX[universalBattlefieldGroup5] = (adjustedAnchorsX[universalBattlefieldGroup4] * boundsDef.Bounds.size.x - universalBattlefieldGroup4.Dimensions.x / 2f - boundsDef.SpacingX - universalBattlefieldGroup5.Dimensions.x / 2f) / boundsDef.Bounds.size.x;
						hashSet.Add(universalBattlefieldGroup5);
						continue;
					}
				}
				splitTheOverlap(universalBattlefieldGroup5, universalBattlefieldGroup4, adjustedAnchorsX, boundsDef, num5);
			}
		}
		while (++num < maxIterations && flag);
		pool.PushObject(hashSet);
		pool.PushObject(universalBattlefieldGroupSentinel, tryClear: false);
		pool.PushObject(universalBattlefieldGroupSentinel2, tryClear: false);
		static bool approximately(float a, float b)
		{
			return Mathf.Abs(a - b) < 0.01f;
		}
		static bool getGroupMinMaxX(IUniversalBattlefieldGroup group, Dictionary<IUniversalBattlefieldGroup, float> dictionary, BoundsDefinition boundsDefinition, out float reference, out float reference2)
		{
			reference = dictionary[group] * boundsDefinition.Bounds.size.x - group.Dimensions.x / 2f;
			reference2 = reference + group.Dimensions.x;
			return true;
		}
		static void moveAndLockGroupA(IUniversalBattlefieldGroup aGroup, IUniversalBattlefieldGroup bGroup, Dictionary<IUniversalBattlefieldGroup, float> dictionary, BoundsDefinition boundsDefinition, HashSet<IUniversalBattlefieldGroup> anchorLocked)
		{
			getGroupMinMaxX(bGroup, dictionary, boundsDefinition, out var minX6, out var _);
			bool flag2 = aGroup.IgnoreRegionSpacing || bGroup.IgnoreRegionSpacing;
			dictionary[aGroup] = (minX6 - aGroup.Dimensions.x / 2f - (flag2 ? 0f : boundsDefinition.SpacingX)) / boundsDefinition.Bounds.size.x;
			anchorLocked.Add(aGroup);
		}
		static void moveAndLockGroupB(IUniversalBattlefieldGroup aGroup, IUniversalBattlefieldGroup bGroup, Dictionary<IUniversalBattlefieldGroup, float> dictionary, BoundsDefinition boundsDefinition, HashSet<IUniversalBattlefieldGroup> anchorLocked)
		{
			getGroupMinMaxX(aGroup, dictionary, boundsDefinition, out var _, out var maxX5);
			bool flag2 = aGroup.IgnoreRegionSpacing || bGroup.IgnoreRegionSpacing;
			dictionary[bGroup] = (maxX5 + bGroup.Dimensions.x / 2f + (flag2 ? 0f : boundsDefinition.SpacingX)) / boundsDefinition.Bounds.size.x;
			anchorLocked.Add(bGroup);
		}
		static void splitTheOverlap(IUniversalBattlefieldGroup aGroup, IUniversalBattlefieldGroup bGroup, Dictionary<IUniversalBattlefieldGroup, float> dictionary, BoundsDefinition boundsDefinition, float totalOverlap)
		{
			dictionary[aGroup] -= totalOverlap / 2f / boundsDefinition.Bounds.size.x;
			dictionary[bGroup] += totalOverlap / 2f / boundsDefinition.Bounds.size.x;
		}
	}

	private void ClampScrollOffsetX()
	{
		BoundsDefinition boundsDefinition = _boundsDefinition;
		float num = 0f - boundsDefinition.Bounds.size.x;
		bool flag = true;
		foreach (IUniversalBattlefieldGroup group in _groups)
		{
			if (group.Dimensions.x != 0f)
			{
				if (!flag && !group.IgnoreRegionSpacing)
				{
					num += boundsDefinition.SpacingX;
				}
				flag = group.IgnoreRegionSpacing;
				num += group.Dimensions.x;
			}
		}
		float scrollOffsetX = _scrollOffsetX;
		_scrollOffsetX = ((num <= 0f) ? 0f : Mathf.Clamp(_scrollOffsetX, (0f - num) / 2f, num / 2f));
		foreach (IUniversalBattlefieldGroup group2 in _groups)
		{
			group2.ApplyPositionalDelta(CdcVector3.right * (_scrollOffsetX - scrollOffsetX));
		}
	}

	public void DrawGizmos()
	{
		if (!_drawBounds || !_battlefieldCardHolder)
		{
			return;
		}
		Bounds bounds = _boundsDefinition.Bounds;
		Gizmos.color = Color.green;
		Gizmos.DrawLine(_battlefieldCardHolder.position + bounds.center + CdcVector3.left * bounds.extents.x + CdcVector3.forward * bounds.extents.z, _battlefieldCardHolder.position + bounds.center + CdcVector3.right * bounds.extents.x + CdcVector3.forward * bounds.extents.z);
		Gizmos.DrawLine(_battlefieldCardHolder.position + bounds.center + CdcVector3.left * bounds.extents.x + CdcVector3.back * bounds.extents.z, _battlefieldCardHolder.position + bounds.center + CdcVector3.right * bounds.extents.x + CdcVector3.back * bounds.extents.z);
		Gizmos.DrawLine(_battlefieldCardHolder.position + bounds.center + CdcVector3.left * bounds.extents.x + CdcVector3.forward * bounds.extents.z, _battlefieldCardHolder.position + bounds.center + CdcVector3.left * bounds.extents.x + CdcVector3.back * bounds.extents.z);
		Gizmos.DrawLine(_battlefieldCardHolder.position + bounds.center + CdcVector3.right * bounds.extents.x + Vector3.forward * bounds.extents.z, _battlefieldCardHolder.position + bounds.center + CdcVector3.right * bounds.extents.x + Vector3.back * bounds.extents.z);
		foreach (IUniversalBattlefieldGroup group in _groups)
		{
			if (group.Dimensions.x != 0f)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(group.Position + CdcVector3.left * group.Dimensions.x / 2f + CdcVector3.forward * group.Dimensions.y / 2f, group.Position + CdcVector3.right * group.Dimensions.x / 2f + CdcVector3.forward * group.Dimensions.y / 2f);
				Gizmos.DrawLine(group.Position + CdcVector3.left * group.Dimensions.x / 2f + CdcVector3.back * group.Dimensions.y / 2f, group.Position + CdcVector3.right * group.Dimensions.x / 2f + CdcVector3.back * group.Dimensions.y / 2f);
				Gizmos.DrawLine(group.Position + CdcVector3.left * group.Dimensions.x / 2f + CdcVector3.forward * group.Dimensions.y / 2f, group.Position + CdcVector3.left * group.Dimensions.x / 2f + CdcVector3.back * group.Dimensions.y / 2f);
				Gizmos.DrawLine(group.Position + CdcVector3.right * group.Dimensions.x / 2f + CdcVector3.forward * group.Dimensions.y / 2f, group.Position + CdcVector3.right * group.Dimensions.x / 2f + CdcVector3.back * group.Dimensions.y / 2f);
			}
		}
	}
}
