using System;
using System.Collections.Generic;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;

[Serializable]
public class SecondaryLayoutContainer
{
	private GameManager _gameManager;

	private IObjectPool _objectPool;

	private CardHolderBase _cardHolder;

	[SerializeField]
	public Vector3 Position;

	[SerializeField]
	public Vector3 Rotation;

	[SerializeField]
	public bool UseScaleOverride;

	[SerializeField]
	public Vector3 ScaleOverride;

	[SerializeField]
	public Vector3 BoundsOffset;

	[SerializeField]
	public Vector3 Bounds;

	[SerializeField]
	public Vector3 PullOutOffset;

	[SerializeField]
	public float LoweredPullOutScale;

	[SerializeField]
	public Vector3 LoweredOffset;

	[SerializeField]
	public bool UseTopCardPosition;

	[SerializeField]
	public float FanRadius = 10f;

	[SerializeField]
	public float FanOverlapOffset = -0.12f;

	[SerializeField]
	public float FanOverlapRotation = -5f;

	[SerializeField]
	public float FanTiltRatio = 1f;

	[SerializeField]
	public float FanVerticalOffset;

	[SerializeField]
	public float FanMaxDeltaAngle = 5f;

	[SerializeField]
	public float FanTotalDeltaAngle = 20f;

	private readonly HashSet<uint> _cardsToLayout = new HashSet<uint>();

	private readonly List<DuelScene_CDC> _currentlyTargetedCards = new List<DuelScene_CDC>();

	private WorkflowBase _lastWorkflow;

	private bool _moveFanLower;

	private uint _secondaryIdToCheck;

	public CardLayout_Fan CardLayout { get; private set; }

	public void Init(GameManager gameManager, IObjectPool objectPool, CardHolderBase cardHolder)
	{
		_gameManager = gameManager;
		_objectPool = objectPool;
		_cardHolder = cardHolder;
		CardLayout = new CardLayout_Fan
		{
			Radius = FanRadius,
			OverlapOffset = FanOverlapOffset,
			OverlapRotation = FanOverlapRotation,
			TiltRatio = FanTiltRatio,
			VerticalOffset = FanVerticalOffset,
			MaxDeltaAngle = FanMaxDeltaAngle,
			TotalDeltaAngle = FanTotalDeltaAngle
		};
	}

	public bool UpdateSecondaryLayoutIdList(DuelScene_CDC focusedCard = null)
	{
		HashSet<uint> hashSet = _objectPool.PopObject<HashSet<uint>>();
		foreach (DuelScene_CDC cardView in _cardHolder.CardViews)
		{
			if (cardView.Model.Instance.TargetedByIds.Count > 0)
			{
				hashSet.Add(cardView.InstanceId);
			}
		}
		hashSet.UnionWith(GetWorkflowIds(_gameManager.CurrentInteraction));
		foreach (ChoosingAttachmentsInfo choosingAttachmentsInfo in _gameManager.CurrentGameState.ChoosingAttachmentsInfos)
		{
			if (choosingAttachmentsInfo.AffectorId == _gameManager.CurrentGameState.LocalPlayer.InstanceId)
			{
				hashSet.UnionWith(choosingAttachmentsInfo.AffectedIds);
			}
		}
		if (focusedCard == null || (focusedCard.CurrentCardHolder != null && focusedCard.CurrentCardHolder.CardHolderType == _cardHolder.CardHolderType))
		{
			MtgCardInstance topCardOnStack = _gameManager.CurrentGameState.GetTopCardOnStack();
			if (topCardOnStack != null)
			{
				focusedCard = _gameManager.ViewManager.GetCardView(topCardOnStack.InstanceId);
			}
		}
		if (focusedCard != null)
		{
			HashSet<uint> hashSet2 = _gameManager.GenericPool.PopObject<HashSet<uint>>();
			CardViewUtilities.GetListOfReferencedCards(focusedCard, _gameManager, hashSet2);
			hashSet.UnionWith(hashSet2);
			hashSet2.Clear();
			_gameManager.GenericPool.PushObject(hashSet2, tryClear: false);
		}
		HashSet<uint> hashSet3 = _objectPool.PopObject<HashSet<uint>>();
		foreach (uint item2 in hashSet)
		{
			uint item = (_secondaryIdToCheck = item2);
			if (!_cardHolder.CardViews.Exists(CardViewInstanceIdEqualsSecondaryIdToCheck))
			{
				hashSet3.Add(item);
			}
		}
		hashSet.ExceptWith(hashSet3);
		hashSet3.Clear();
		_objectPool.PushObject(hashSet3, tryClear: false);
		bool result = hashSet.Count > 0 || _cardsToLayout.Count > 0;
		_cardsToLayout.Clear();
		_cardsToLayout.UnionWith(hashSet);
		hashSet.Clear();
		_objectPool.PushObject(hashSet, tryClear: false);
		return result;
	}

	public void GenerateData(ref List<CardLayoutData> cardHolderLayoutData, ref List<CardLayoutData> secondaryLayoutData)
	{
		UpdateSecondaryLayoutIdList(CardHoverController.HoveredCard);
		List<DuelScene_CDC> cardsToLayout = _cardHolder.CardViews.FindAll((DuelScene_CDC x) => _cardsToLayout.Contains(x.InstanceId));
		cardHolderLayoutData.RemoveAll((CardLayoutData x) => cardsToLayout.Contains(x.Card));
		CardLayoutData cardLayoutData = null;
		if (UseTopCardPosition && cardHolderLayoutData.Count > 0)
		{
			int index = 0;
			for (int num = 0; num < cardHolderLayoutData.Count; num++)
			{
				float z = cardHolderLayoutData[index].Position.z;
				if (cardHolderLayoutData[num].Position.z <= z)
				{
					index = num;
				}
			}
			cardLayoutData = cardHolderLayoutData[index];
		}
		Vector3 position = Position;
		if (cardLayoutData != null)
		{
			position += cardLayoutData.Position;
		}
		Quaternion rotation = Quaternion.Euler(Rotation);
		Vector3 center = _cardHolder.transform.position + BoundsOffset + new Vector3(0f, 0f - position.z, 0f);
		Rect screenRect = new Bounds(center, Bounds).GetScreenRect(CurrentCamera.Value);
		if (_cardHolder.PlayerNum == GREPlayerNum.LocalPlayer)
		{
			_currentlyTargetedCards.Clear();
			if (_gameManager.CurrentInteraction is ITargetCDCListProviderWorkflow targetCDCListProviderWorkflow)
			{
				_currentlyTargetedCards.AddRange(targetCDCListProviderWorkflow.GetTargetCDCs());
			}
			if (_gameManager.CurrentInteraction != null && _gameManager.CurrentInteraction != _lastWorkflow)
			{
				_moveFanLower = false;
			}
			foreach (DuelScene_CDC currentlyTargetedCard in _currentlyTargetedCards)
			{
				BoxCollider collider = currentlyTargetedCard.Collider;
				if (!(collider == null))
				{
					Rect screenRect2 = collider.bounds.GetScreenRect(CurrentCamera.Value);
					if (screenRect.Intersects(screenRect2, out var _))
					{
						_moveFanLower = true;
						break;
					}
				}
			}
			_currentlyTargetedCards.Clear();
		}
		if (_gameManager.CurrentInteraction != null)
		{
			_lastWorkflow = _gameManager.CurrentInteraction;
		}
		if (_moveFanLower)
		{
			position += LoweredOffset;
		}
		secondaryLayoutData.Clear();
		CardLayout.GenerateData(cardsToLayout, ref secondaryLayoutData, position, rotation);
		foreach (CardLayoutData secondaryLayoutDatum in secondaryLayoutData)
		{
			if (UseScaleOverride)
			{
				secondaryLayoutDatum.Scale = ScaleOverride;
			}
			foreach (ChoosingAttachmentsInfo choosingAttachmentsInfo in _gameManager.CurrentGameState.ChoosingAttachmentsInfos)
			{
				if (choosingAttachmentsInfo.AffectorId == _gameManager.CurrentGameState.LocalPlayer.InstanceId && -1 < choosingAttachmentsInfo.CurrentAffectedIdsIndex && choosingAttachmentsInfo.CurrentAffectedIdsIndex < choosingAttachmentsInfo.SelectedIds.Count && choosingAttachmentsInfo.AffectedIds[choosingAttachmentsInfo.CurrentAffectedIdsIndex] == secondaryLayoutDatum.Card.InstanceId)
				{
					Vector3 pullOutOffset = PullOutOffset;
					if (_moveFanLower)
					{
						pullOutOffset *= LoweredPullOutScale;
					}
					secondaryLayoutDatum.Position += pullOutOffset;
				}
			}
		}
	}

	public void DrawGizmos(List<CardLayoutData> previousLayoutData)
	{
		if (_cardHolder.PlayerNum != GREPlayerNum.LocalPlayer)
		{
			return;
		}
		Vector3 position = Position;
		if (UseTopCardPosition && previousLayoutData.Count > 0)
		{
			int index = 0;
			for (int i = 0; i < previousLayoutData.Count; i++)
			{
				float z = previousLayoutData[index].Position.z;
				if (previousLayoutData[i].Position.z <= z)
				{
					index = i;
				}
			}
			position += previousLayoutData[index].Position;
		}
		Vector3 center = _cardHolder.transform.position + BoundsOffset + new Vector3(0f, 0f - position.z, 0f);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(center, Bounds);
	}

	private bool CardViewInstanceIdEqualsSecondaryIdToCheck(DuelScene_CDC cardView)
	{
		if ((bool)cardView)
		{
			return cardView.InstanceId == _secondaryIdToCheck;
		}
		return false;
	}

	public static IEnumerable<uint> GetWorkflowIds(WorkflowBase currentInteraction)
	{
		if (currentInteraction is IParentWorkflow parentWorkflow)
		{
			foreach (WorkflowBase childWorkflow in parentWorkflow.ChildWorkflows)
			{
				foreach (uint workflowId in GetWorkflowIds(childWorkflow))
				{
					yield return workflowId;
				}
			}
		}
		if (!(currentInteraction is ISecondaryLayoutIdListProvider secondaryLayoutIdListProvider))
		{
			yield break;
		}
		foreach (uint secondaryLayoutId in secondaryLayoutIdListProvider.GetSecondaryLayoutIds())
		{
			yield return secondaryLayoutId;
		}
	}
}
