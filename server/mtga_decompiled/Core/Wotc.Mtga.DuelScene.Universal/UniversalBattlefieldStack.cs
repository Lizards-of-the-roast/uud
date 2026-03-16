using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Universal;

public class UniversalBattlefieldStack : IBattlefieldStack
{
	private MtgGameState _gameState;

	private WorkflowBase _workflow;

	private ICardViewProvider _cardViewProvider;

	private IObjectPool _pool;

	private List<DuelScene_CDC> _tmpCardList;

	public readonly List<CardLayoutData> CardLayoutDatas = new List<CardLayoutData>();

	public int? StackCount;

	public List<DuelScene_CDC> AllCards { get; private set; } = new List<DuelScene_CDC>();

	public List<DuelScene_CDC> StackedCards { get; private set; } = new List<DuelScene_CDC>();

	public DuelScene_CDC StackParent { get; private set; }

	public ICardDataAdapter StackParentModel { get; private set; }

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

	public uint Age
	{
		get
		{
			uint result = uint.MaxValue;
			if (AttachmentCount > 0 || ExileCount > 0)
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

	public int AttachmentCount => AllCards.Where(delegate(DuelScene_CDC cardView)
	{
		ICardDataAdapter model = cardView.Model;
		if (model == null || model.Instance.AttachedToId != 0)
		{
			ICardDataAdapter model2 = cardView.Model;
			if (model2 == null)
			{
				return true;
			}
			return model2.ZoneType != ZoneType.Exile;
		}
		return false;
	}).Count();

	public int ExileCount => AllCards.Where(delegate(DuelScene_CDC cardView)
	{
		ICardDataAdapter model = cardView.Model;
		if (model == null || model.Instance.AttachedToId != 0)
		{
			ICardDataAdapter model2 = cardView.Model;
			if (model2 == null)
			{
				return false;
			}
			return model2.ZoneType == ZoneType.Exile;
		}
		return false;
	}).Count();

	public void RefreshAbilitiesBasedOnStackPosition()
	{
		float value = 0f;
		if ((bool)StackParent && StackParent.Model != null && StackParent.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield)
		{
			value = StackParent.UpdateAbilityVisuals<PersistVFX, PersistSfx>(display: true, animate: true, null);
			StackParent.UpdateTopCardRelevantVisuals(display: true);
			StackParent.UpdateCounterVisibility(display: true);
		}
		foreach (DuelScene_CDC stackedCard in StackedCards)
		{
			if ((bool)stackedCard && stackedCard.Model != null && stackedCard.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield)
			{
				stackedCard.UpdateAbilityVisuals<PersistVFX, PersistSfx>(display: false, !HasAttachmentOrExile, value);
				stackedCard.UpdateTopCardRelevantVisuals(display: false);
				stackedCard.UpdateCounterVisibility(AttachmentCount > 0 || ExileCount > 0);
			}
		}
	}

	public void Init(DuelScene_CDC parent, MtgGameState gameState, WorkflowBase workflow, ICardViewProvider cardViewProvider, IObjectPool pool)
	{
		StackParent = parent;
		if ((bool)parent)
		{
			AllCards.Add(parent);
			Init(parent.Model, gameState, workflow, cardViewProvider, pool);
		}
		else
		{
			Init((ICardDataAdapter)null, gameState, workflow, cardViewProvider, pool);
		}
		if ((bool)parent && parent.IsDirty)
		{
			parent.ImmediateUpdate();
		}
	}

	public void Init(ICardDataAdapter parent, MtgGameState gameState, WorkflowBase workflow, ICardViewProvider cardViewProvider, IObjectPool pool)
	{
		StackParentModel = parent;
		_gameState = gameState;
		_workflow = workflow;
		_cardViewProvider = cardViewProvider;
		_pool = pool;
	}

	public void Init(IBattlefieldStack stack, MtgGameState gameState, WorkflowBase workflow, ICardViewProvider cardViewProvider, IObjectPool pool)
	{
		Init(stack.StackParent, gameState, workflow, cardViewProvider, pool);
		StackedCards.AddRange(stack.StackedCards);
		AllCards.AddRange(stack.StackedCards);
		Sort();
	}

	public void Clear()
	{
		_gameState = null;
		_workflow = null;
		_cardViewProvider = null;
		_pool = null;
		StackCount = null;
		CardLayoutDatas.Clear();
		AllCards.Clear();
		StackedCards.Clear();
		StackParent = null;
		StackParentModel = null;
	}

	public void Sort()
	{
		if (HasAttachmentOrExile)
		{
			_tmpCardList = _pool.PopObject<List<DuelScene_CDC>>();
			if (StackParentModel != null)
			{
				MtgCardInstance cardById = _gameState.GetCardById(StackParentModel.InstanceId);
				if (cardById != null)
				{
					addCards(AttachmentAndExileStackGroupData.GenerateGroups(cardById, _gameState, _cardViewProvider, setParent: false), _tmpCardList);
				}
			}
			if ((bool)StackParent && !_tmpCardList.Contains(StackParent))
			{
				_tmpCardList.Insert(0, StackParent);
			}
			if (_tmpCardList.Count > 0)
			{
				AllCards.Sort(CompareIndex);
			}
			_pool.PushObject(_tmpCardList);
		}
		else if (IsBlockStack)
		{
			AllCards.Sort(CompareDistinguishedObjects);
		}
		else
		{
			AllCards.Sort(CompareAvailableActions);
		}
		if (!HasAttachmentOrExile)
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

	private bool TryGetActionsAvailableWorkflow(out ActionsAvailableWorkflow workflow)
	{
		WorkflowBase workflow2 = _workflow;
		workflow = workflow2 as ActionsAvailableWorkflow;
		if (workflow == null && workflow2 is PayCostWorkflow payCostWorkflow)
		{
			foreach (WorkflowBase childWorkflow in payCostWorkflow.ChildWorkflows)
			{
				workflow = childWorkflow as ActionsAvailableWorkflow;
				if (workflow != null)
				{
					break;
				}
			}
		}
		return workflow != null;
	}

	private int CompareDistinguishedObjects(DuelScene_CDC a, DuelScene_CDC b)
	{
		bool value = (a.Model?.Instance?.DistinguishedByIds.Count).GetValueOrDefault() > 0;
		int num = ((b.Model?.Instance?.DistinguishedByIds.Count).GetValueOrDefault() > 0).CompareTo(value);
		if (num == 0)
		{
			num = CompareAge(a, b);
		}
		return num;
	}

	private int CompareAge(DuelScene_CDC a, DuelScene_CDC b)
	{
		return a.InstanceId.CompareTo(b.InstanceId);
	}

	private int CompareAvailableActions(DuelScene_CDC a, DuelScene_CDC b)
	{
		int num = 0;
		if (TryGetActionsAvailableWorkflow(out var workflow))
		{
			bool flag = workflow.GetActionsForId(a.InstanceId).Exists((GreInteraction x) => x.IsActive);
			bool value = workflow.GetActionsForId(b.InstanceId).Exists((GreInteraction x) => x.IsActive);
			num = -flag.CompareTo(value);
		}
		if (num == 0)
		{
			num = CompareDistinguishedObjects(a, b);
		}
		return num;
	}

	private int CompareIndex(DuelScene_CDC a, DuelScene_CDC b)
	{
		return _tmpCardList.IndexOf(a).CompareTo(_tmpCardList.IndexOf(b));
	}
}
