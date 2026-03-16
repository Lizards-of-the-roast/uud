using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using WorkflowVisuals;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Universal;

public sealed class UniversalBattlefieldGroup : IUniversalBattlefieldGroup
{
	public enum GroupType
	{
		Card,
		Pet,
		Command,
		Prompted,
		Spacer
	}

	[Serializable]
	public class Configuration
	{
		[SerializeField]
		private string _name;

		[SerializeField]
		private GroupType _groupType;

		[SerializeField]
		[HideInInspector]
		private ListValidator _validator = new ListValidator();

		[SerializeField]
		[HideInInspector]
		private BattlefieldRegionType _regionType;

		[SerializeField]
		[HideInInspector]
		private GREPlayerNum _regionController;

		[SerializeField]
		private bool _collapsible;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("Where this Group centers itself within its Region's bounds. x=0 is left edge, x=1 is right edge.")]
		private float _anchorX = 0.5f;

		[SerializeField]
		[Tooltip("Scale applied to CDCs added to this group")]
		private Vector3 _scale = Vector3.one;

		[SerializeField]
		[Tooltip("Horizontal spacing between content in this group")]
		private float _spacingX;

		[SerializeField]
		[Tooltip("Horizontal spacing between attachments/exiles when stack expanded")]
		private float _attachExileSpacingX;

		public string Name => _name;

		public GroupType GroupType => _groupType;

		public BattlefieldRegionType RegionType => _regionType;

		public GREPlayerNum RegionController => _regionController;

		public bool Collapsible => _collapsible;

		public float AnchorX => _anchorX;

		public Vector3 Scale => _scale;

		public float SpacingX => _spacingX;

		public float AttachExileSpacingX => _attachExileSpacingX;

		public bool CardIsValid(ICardDataAdapter cardData, IReadOnlyList<MtgPlayer> players = null, bool isFocusPlayer = true)
		{
			return _validator.IsValidInGroup(new ValidatorBlackboard
			{
				CardData = cardData,
				Players = players,
				IsFocusPlayer = isFocusPlayer
			});
		}
	}

	private static StackBlockerAndAgeComparer _blockerStackAgeComparer = new StackBlockerAndAgeComparer();

	private List<UniversalBattlefieldStack> _stacks = new List<UniversalBattlefieldStack>();

	private List<UniversalBattlefieldStack> _virtualStacks = new List<UniversalBattlefieldStack>();

	private readonly Vector3 _tappedRotation;

	private readonly Vector3 _declaredAttackOffset;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly IObjectPool _pool;

	private readonly IEqualityComparer<DuelScene_CDC> _canStackComparer;

	public Vector2 Dimensions { get; private set; }

	public Vector3 Position { get; private set; }

	public IEnumerable<UniversalBattlefieldStack> VisibleStacks
	{
		get
		{
			if (_virtualStacks.Count <= 0)
			{
				return _stacks;
			}
			return _virtualStacks;
		}
	}

	public IEnumerable<UniversalBattlefieldStack> AllStacks => _stacks;

	public IEnumerable<CardLayoutData> CardLayoutDatas => _stacks.Concat(_virtualStacks).SelectMany((UniversalBattlefieldStack stack) => stack.CardLayoutDatas);

	public bool IgnoreRegionSpacing => false;

	public Configuration Config { get; private set; }

	public UniversalBattlefieldGroup(Configuration config, Vector3 tappedRotation, Vector3 declaredAttackOffset, ICardViewProvider cardViewProvider, IObjectPool pool, IEqualityComparer<DuelScene_CDC> canStackComparer)
	{
		Config = config;
		_tappedRotation = tappedRotation;
		_declaredAttackOffset = declaredAttackOffset;
		_cardViewProvider = cardViewProvider ?? NullCardViewProvider.Default;
		_pool = pool ?? NullObjectPool.Default;
		_canStackComparer = canStackComparer;
	}

	public bool TryAddCard(DuelScene_CDC cardView, GameManager gameManager, bool isFocusPlayer)
	{
		if (TryGetAttachedToStack(cardView.Model, out var attachedToStack))
		{
			attachedToStack.AllCards.Add(cardView);
			attachedToStack.StackedCards.Add(cardView);
			return true;
		}
		if (Config.CardIsValid(cardView.Model, gameManager.LatestGameState.Players, isFocusPlayer))
		{
			if (gameManager.CurrentGameState.GetEntityById(cardView.Model.Instance.AttachedToId) is MtgCardInstance mtgCardInstance)
			{
				ZoneType type = mtgCardInstance.Zone.Type;
				if (type != ZoneType.Graveyard && type != ZoneType.Exile)
				{
					goto IL_013c;
				}
			}
			foreach (UniversalBattlefieldStack stack in _stacks)
			{
				if ((bool)stack.StackParent && _canStackComparer.Equals(stack.StackParent, cardView) && !stack.HasAttachmentOrExile)
				{
					stack.StackedCards.Add(cardView);
					stack.AllCards.Add(cardView);
					return true;
				}
			}
			UniversalBattlefieldStack universalBattlefieldStack = gameManager.GenericPool.PopObject<UniversalBattlefieldStack>();
			universalBattlefieldStack.Init(cardView, gameManager.CurrentGameState, gameManager.CurrentInteraction, gameManager.ViewManager, gameManager.GenericPool);
			_stacks.Add(universalBattlefieldStack);
			return true;
		}
		goto IL_013c;
		IL_013c:
		return false;
	}

	public bool TryAddCard(ICardDataAdapter cardModel, GameManager gameManager, bool isFocusPlayer)
	{
		if (Config.CardIsValid(cardModel, gameManager.LatestGameState.Players, isFocusPlayer))
		{
			UniversalBattlefieldStack universalBattlefieldStack = gameManager.GenericPool.PopObject<UniversalBattlefieldStack>();
			universalBattlefieldStack.Init(cardModel, gameManager.CurrentGameState, gameManager.CurrentInteraction, gameManager.ViewManager, gameManager.GenericPool);
			_stacks.Add(universalBattlefieldStack);
			return true;
		}
		return false;
	}

	private bool TryGetAttachedToStack(ICardDataAdapter card, out UniversalBattlefieldStack attachedToStack)
	{
		attachedToStack = null;
		foreach (UniversalBattlefieldStack stack in _stacks)
		{
			if (stack.StackParentModel.InstanceId == card.Instance.AttachedToId || stack.StackedCards.Exists((DuelScene_CDC stackCard) => stackCard.InstanceId == card.Instance.AttachedToId))
			{
				attachedToStack = stack;
				return true;
			}
		}
		return false;
	}

	public void Clear(IObjectPool pool)
	{
		PoolStacks(_stacks, pool);
		PoolStacks(_virtualStacks, pool);
	}

	private void PoolStacks(List<UniversalBattlefieldStack> stacks, IObjectPool pool)
	{
		foreach (UniversalBattlefieldStack stack in stacks)
		{
			stack.Clear();
			pool.PushObject(stack, tryClear: false);
		}
		stacks.Clear();
	}

	public void GenerateLayoutDatas(MtgGameState gameState, WorkflowBase workflow, IReadOnlyCollection<CardLayoutData> solvedLayoutDatas, bool collapse, uint expandedStackParentId)
	{
		_stacks.ForEach(delegate(UniversalBattlefieldStack stack)
		{
			int num3 = stack.AllCards.Count();
			stack.StackCount = ((num3 > 3) ? new int?(num3) : ((int?)null));
		});
		_stacks.ForEach(delegate(UniversalBattlefieldStack stack)
		{
			stack.Sort();
		});
		_blockerStackAgeComparer.Init(gameState, solvedLayoutDatas);
		_blockerStackAgeComparer.TitleIdToStackAge.Clear();
		ComputeAgesForStacks(_stacks, _blockerStackAgeComparer.TitleIdToStackAge);
		_stacks.Sort(_blockerStackAgeComparer);
		_stacks.ForEach(delegate(UniversalBattlefieldStack stack)
		{
			stack.CardLayoutDatas.Clear();
		});
		_virtualStacks.ForEach(delegate(UniversalBattlefieldStack stack)
		{
			stack.CardLayoutDatas.Clear();
		});
		PoolStacks(_virtualStacks, _pool);
		if (collapse && Config.Collapsible)
		{
			HashSet<UniversalBattlefieldStack> hashSet = _pool.PopObject<HashSet<UniversalBattlefieldStack>>();
			foreach (UniversalBattlefieldStack stack in _stacks)
			{
				if (stack.AllCards.Exists(gameState, workflow, targetedByGamestateOrWorkflow))
				{
					hashSet.Add(stack);
				}
			}
			IEnumerable<UniversalBattlefieldStack> source = _stacks.Where((UniversalBattlefieldStack stack) => stack.StackParentModel.IsTapped && (bool)stack.StackParent).Except(hashSet);
			IEnumerable<UniversalBattlefieldStack> source2 = _stacks.Where((UniversalBattlefieldStack stack) => !stack.StackParentModel.IsTapped && (bool)stack.StackParent).Except(hashSet);
			IEnumerable<DuelScene_CDC> enumerable = source.Select((UniversalBattlefieldStack stack) => stack.StackParent);
			IEnumerable<DuelScene_CDC> enumerable2 = source2.Select((UniversalBattlefieldStack stack) => stack.StackParent);
			foreach (UniversalBattlefieldStack item in hashSet)
			{
				UniversalBattlefieldStack universalBattlefieldStack = createVirtualStackFromCards(item.AllCards, gameState, workflow, _cardViewProvider, _pool);
				_virtualStacks.Add(universalBattlefieldStack);
				setVirtualStackCount(universalBattlefieldStack, item.AllCards.Count, universalBattlefieldStack.AllCards.Count);
			}
			int num = enumerable.Count();
			if (num > 0)
			{
				UniversalBattlefieldStack universalBattlefieldStack2 = createVirtualStackFromCards(enumerable, gameState, workflow, _cardViewProvider, _pool);
				_virtualStacks.Add(universalBattlefieldStack2);
				setVirtualStackCount(universalBattlefieldStack2, source.SelectMany((UniversalBattlefieldStack stack) => stack.AllCards).Count(), num);
			}
			int num2 = enumerable2.Count();
			if (num2 > 0)
			{
				UniversalBattlefieldStack universalBattlefieldStack3 = createVirtualStackFromCards(enumerable2, gameState, workflow, _cardViewProvider, _pool);
				_virtualStacks.Add(universalBattlefieldStack3);
				setVirtualStackCount(universalBattlefieldStack3, source2.SelectMany((UniversalBattlefieldStack stack) => stack.AllCards).Count(), num2);
			}
			foreach (UniversalBattlefieldStack stack2 in _stacks)
			{
				foreach (DuelScene_CDC allCard in stack2.AllCards)
				{
					if (!_virtualStacks.Exists(allCard, (UniversalBattlefieldStack virtualStack, DuelScene_CDC card) => virtualStack.AllCards.Contains(card)))
					{
						stack2.CardLayoutDatas.Add(new CardLayoutData(allCard, CdcVector3.down * 5f, Quaternion.identity, Config.Scale, isVisibleInLayout: false));
					}
				}
			}
			Dimensions = GenerateLayoutInternal(_virtualStacks, _tappedRotation, _declaredAttackOffset, Config);
			Position = CdcVector3.right * Dimensions.x / 2f;
			hashSet.Clear();
			_pool.PushObject(hashSet);
			return;
		}
		HashSet<UniversalBattlefieldStack> hashSet2 = _pool.PopObject<HashSet<UniversalBattlefieldStack>>();
		foreach (UniversalBattlefieldStack stack3 in _stacks)
		{
			if (stack3.AllCards.Where(delegate(DuelScene_CDC cdc)
			{
				ICardDataAdapter model = cdc.Model;
				return model == null || model.Instance.AttachedToId != 0;
			}).Exists(gameState, workflow, targetedByGamestateOrWorkflow))
			{
				hashSet2.Add(stack3);
			}
		}
		UniversalBattlefieldStack universalBattlefieldStack4 = _stacks.Find(expandedStackParentId, (UniversalBattlefieldStack stack, uint num3) => stack.StackParentModel.InstanceId == num3);
		foreach (UniversalBattlefieldStack stack4 in _stacks)
		{
			if (stack4.StackParentModel.InstanceId == expandedStackParentId)
			{
				foreach (DuelScene_CDC item2 in Enumerable.Reverse(universalBattlefieldStack4.StackedCards))
				{
					_virtualStacks.Add(createVirtualStackFromCard(item2, gameState, workflow, _cardViewProvider, _pool));
				}
				if ((bool)universalBattlefieldStack4.StackParent)
				{
					_virtualStacks.Add(createVirtualStackFromCard(universalBattlefieldStack4.StackParent, gameState, workflow, _cardViewProvider, _pool));
				}
			}
			else if (hashSet2.Contains(stack4))
			{
				foreach (DuelScene_CDC item3 in Enumerable.Reverse(stack4.StackedCards))
				{
					_virtualStacks.Add(createVirtualStackFromCard(item3, gameState, workflow, _cardViewProvider, _pool));
				}
				if ((bool)stack4.StackParent)
				{
					_virtualStacks.Add(createVirtualStackFromCard(stack4.StackParent, gameState, workflow, _cardViewProvider, _pool));
				}
			}
			else
			{
				UniversalBattlefieldStack universalBattlefieldStack5 = _pool.PopObject<UniversalBattlefieldStack>();
				universalBattlefieldStack5.Init(stack4, gameState, workflow, _cardViewProvider, _pool);
				universalBattlefieldStack5.StackCount = stack4.StackCount;
				_virtualStacks.Add(universalBattlefieldStack5);
			}
		}
		Dimensions = GenerateLayoutInternal(_virtualStacks, _tappedRotation, _declaredAttackOffset, Config);
		Position = CdcVector3.right * Dimensions.x / 2f;
		static UniversalBattlefieldStack createVirtualStackFromCard(DuelScene_CDC cardView, MtgGameState gameState2, WorkflowBase workflow2, ICardViewProvider cardViewProvider, IObjectPool pool)
		{
			UniversalBattlefieldStack universalBattlefieldStack6 = pool.PopObject<UniversalBattlefieldStack>();
			universalBattlefieldStack6.Init(cardView, gameState2, workflow2, cardViewProvider, pool);
			universalBattlefieldStack6.Sort();
			return universalBattlefieldStack6;
		}
		static UniversalBattlefieldStack createVirtualStackFromCards(IEnumerable<DuelScene_CDC> cardViews, MtgGameState gameState2, WorkflowBase workflow2, ICardViewProvider cardViewProvider, IObjectPool pool)
		{
			UniversalBattlefieldStack universalBattlefieldStack6 = createVirtualStackFromCard(cardViews.First(), gameState2, workflow2, cardViewProvider, pool);
			universalBattlefieldStack6.StackedCards.AddRange(cardViews.Skip(1));
			universalBattlefieldStack6.AllCards.AddRange(cardViews.Skip(1));
			universalBattlefieldStack6.Sort();
			return universalBattlefieldStack6;
		}
		static void setVirtualStackCount(UniversalBattlefieldStack virtualStack, int actualCount, int virtualCount)
		{
			int? stackCount = ((actualCount > virtualCount) ? new int?(actualCount) : ((virtualCount > 3) ? new int?(virtualCount) : ((int?)null)));
			virtualStack.StackCount = stackCount;
		}
		static bool targetedByGamestateOrWorkflow(DuelScene_CDC cdc, MtgGameState mtgGameState, WorkflowBase workflowBase)
		{
			foreach (TargetSpec item4 in mtgGameState.TargetInfo)
			{
				if (item4.Affector == cdc.InstanceId)
				{
					return true;
				}
				if (item4.Affected.Contains(cdc.InstanceId))
				{
					return true;
				}
			}
			IEnumerable<Arrows.LineData> enumerable3 = workflowBase?.Arrows.LineDatas;
			foreach (Arrows.LineData item5 in enumerable3 ?? Enumerable.Empty<Arrows.LineData>())
			{
				if (cdc.InstanceId == item5.SourceEntityId)
				{
					return true;
				}
				if (cdc.InstanceId == item5.TargetEntityId)
				{
					return true;
				}
			}
			return false;
		}
	}

	private static Vector2 GenerateLayoutInternal(IEnumerable<UniversalBattlefieldStack> stacks, Vector3 tappedRotation, Vector3 declaredAttackOffset, Configuration config)
	{
		float num = 0f;
		float num2 = 0f;
		Vector3 zero = Vector3.zero;
		foreach (UniversalBattlefieldStack stack in stacks)
		{
			DuelScene_CDC duelScene_CDC = ((stack.StackParent != null) ? stack.StackParent : stack.StackedCards[0]);
			Bounds getColliderBounds = duelScene_CDC.ActiveScaffold.GetColliderBounds;
			Vector3 vector = getScale(duelScene_CDC.Model, config.Scale);
			float num3 = getColliderBounds.size.x * vector.x;
			float num4 = getColliderBounds.size.z * vector.z;
			num2 = Mathf.Max(num2, getColliderBounds.size.y * vector.y);
			float num5 = num3;
			if (stack.AllCards.Count <= 1)
			{
				if ((bool)stack.StackParent)
				{
					stack.CardLayoutDatas.Add(new CardLayoutData
					{
						Card = stack.StackParent,
						Position = zero + CdcVector3.right * num5 / 2f + getAttackerOffset(stack.StackParentModel, declaredAttackOffset, config.RegionController),
						Rotation = getRotation(stack.StackParentModel, tappedRotation),
						Scale = getScale(stack.StackParentModel, config.Scale),
						CardGameObject = stack.StackParent.gameObject
					});
				}
			}
			else
			{
				num5 *= 1.25f;
				int num6 = Mathf.Clamp(stack.StackedCards.Count, 0, 3);
				if ((bool)stack.StackParent)
				{
					stack.CardLayoutDatas.Add(new CardLayoutData
					{
						Card = stack.StackParent,
						Position = zero + CdcVector3.right * num5 + CdcVector3.left * num3 / 2f + CdcVector3.up * num4 * num6 + getAttackerOffset(stack.StackParentModel, declaredAttackOffset, config.RegionController),
						Rotation = getRotation(stack.StackParentModel, tappedRotation),
						Scale = getScale(stack.StackParentModel, config.Scale),
						IsVisibleInLayout = true,
						CardGameObject = stack.StackParent.gameObject
					});
				}
				for (int i = 0; i < stack.StackedCards.Count; i++)
				{
					DuelScene_CDC duelScene_CDC2 = stack.StackedCards[i];
					if (i < num6)
					{
						Vector3 vector2 = CdcVector3.up * num4 * (num6 - i - 1);
						float num7 = (num5 - num3) / (float)num6;
						Vector3 vector3 = CdcVector3.right * num5 + CdcVector3.left * num3 + CdcVector3.left * num7 * (i + 1) + CdcVector3.right * getScale(duelScene_CDC2.Model, config.Scale).x * duelScene_CDC2.ActiveScaffold.GetColliderBounds.size.x / 2f;
						stack.CardLayoutDatas.Add(new CardLayoutData
						{
							Card = duelScene_CDC2,
							Position = zero + vector3 + vector2 + getAttackerOffset(duelScene_CDC.Model, declaredAttackOffset, config.RegionController),
							Rotation = getRotation(duelScene_CDC2.Model, tappedRotation),
							Scale = getScale(duelScene_CDC2.Model, config.Scale),
							IsVisibleInLayout = true,
							CardGameObject = duelScene_CDC2.gameObject
						});
					}
					else
					{
						stack.CardLayoutDatas.Add(new CardLayoutData
						{
							Card = duelScene_CDC2,
							Position = CdcVector3.down * 5f,
							Scale = getScale(duelScene_CDC2.Model, config.Scale),
							IsVisibleInLayout = false,
							CardGameObject = duelScene_CDC2.gameObject
						});
					}
				}
			}
			float num8 = ((duelScene_CDC.Model.Instance.AttachedToId == 0) ? config.SpacingX : config.AttachExileSpacingX);
			zero += CdcVector3.right * (num5 + num8);
			num += num5 + ((num > 0f) ? num8 : 0f);
		}
		return new Vector2(num, num2);
		static Vector3 getAttackerOffset(ICardDataAdapter card, Vector3 vector4, GREPlayerNum controller)
		{
			if (card.Instance.AttackState != AttackState.Declared && card.Instance.AttackState != AttackState.Attacking)
			{
				return Vector3.zero;
			}
			return vector4 * ((controller == GREPlayerNum.LocalPlayer) ? 1f : (-1f));
		}
		static Quaternion getRotation(ICardDataAdapter card, Vector3 euler)
		{
			if (!card.Instance.IsTapped)
			{
				return Quaternion.identity;
			}
			return Quaternion.Euler(euler);
		}
		static Vector3 getScale(ICardDataAdapter card, Vector3 configScale)
		{
			return configScale * ((card.ZoneType == ZoneType.Exile) ? 0.6f : 1f);
		}
	}

	private static void ComputeAgesForStacks(List<UniversalBattlefieldStack> stacks, Dictionary<uint, uint> titleIdToStackAge)
	{
		titleIdToStackAge.Clear();
		foreach (UniversalBattlefieldStack stack in stacks)
		{
			uint titleId = ((IBattlefieldStack)stack).StackParentModel.TitleId;
			if (titleIdToStackAge.ContainsKey(titleId))
			{
				if (titleIdToStackAge[titleId] > ((IBattlefieldStack)stack).Age)
				{
					titleIdToStackAge[titleId] = ((IBattlefieldStack)stack).Age;
				}
			}
			else
			{
				titleIdToStackAge.Add(titleId, ((IBattlefieldStack)stack).Age);
			}
		}
	}

	public void ApplyPositionalDelta(Vector3 posDelta)
	{
		Position += posDelta;
		foreach (CardLayoutData cardLayoutData in CardLayoutDatas)
		{
			if (cardLayoutData != null)
			{
				cardLayoutData.Position += posDelta;
			}
		}
	}
}
