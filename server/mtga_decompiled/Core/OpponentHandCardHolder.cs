using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class OpponentHandCardHolder : BaseHandCardHolder, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IHoverableZone
{
	protected class CardSorter
	{
		public List<DuelScene_CDC> _allCards = new List<DuelScene_CDC>();

		private List<DuelScene_CDC> _unknownCards = new List<DuelScene_CDC>();

		private List<DuelScene_CDC> _knownCards = new List<DuelScene_CDC>();

		private List<DuelScene_CDC> _unknownNearHandCastables = new List<DuelScene_CDC>();

		private List<DuelScene_CDC> _knownNearHandCastables = new List<DuelScene_CDC>();

		public void TrackCard(DuelScene_CDC card)
		{
			_allCards.Add(card);
		}

		public void UnTrackCard(DuelScene_CDC card)
		{
			_allCards.Remove(card);
		}

		public List<DuelScene_CDC> GetSortedCards(bool shuffleUnknown = false)
		{
			_unknownCards.Clear();
			_knownCards.Clear();
			_unknownNearHandCastables.Clear();
			_knownNearHandCastables.Clear();
			while (_allCards.Count > 0)
			{
				DuelScene_CDC duelScene_CDC = _allCards[0];
				getTargetList(duelScene_CDC).Add(duelScene_CDC);
				_allCards.RemoveAt(0);
			}
			if (shuffleUnknown)
			{
				_unknownCards.Shuffle();
			}
			_knownNearHandCastables = _knownNearHandCastables.OrderBy((DuelScene_CDC x) => x.Model.ZoneType).ToList();
			_allCards.AddRange(_unknownCards);
			_allCards.AddRange(_knownCards);
			_allCards.AddRange(_unknownNearHandCastables);
			_allCards.AddRange(_knownNearHandCastables);
			return _allCards;
			List<DuelScene_CDC> getTargetList(DuelScene_CDC cardView)
			{
				bool flag = !cardView.VisualModel.IsDisplayedFaceDown;
				ICardDataAdapter model = cardView.Model;
				if (model != null && model.ZoneType == ZoneType.Hand)
				{
					if (!flag)
					{
						return _unknownCards;
					}
					return _knownCards;
				}
				if (!flag)
				{
					return _unknownNearHandCastables;
				}
				return _knownNearHandCastables;
			}
		}
	}

	protected CardSorter _sorter = new CardSorter();

	private readonly Dictionary<uint, ICardDataAdapter> _allRevealData = new Dictionary<uint, ICardDataAdapter>();

	private readonly Dictionary<uint, ICardDataAdapter> _revealDataBank = new Dictionary<uint, ICardDataAdapter>();

	private readonly Dictionary<uint, DuelScene_CDC> _cardViewsByRevealedId = new Dictionary<uint, DuelScene_CDC>();

	[SerializeField]
	private CardLayout_Hand _collapsedFanLayout;

	[SerializeField]
	private Vector3 _mindControlCenterPoint;

	private Vector3 _centerPointOverride = Vector3.zero;

	private List<DuelScene_CDC> _zoneCardViews => base.CardViews.FindAll(delegate(DuelScene_CDC x)
	{
		ICardDataAdapter model = x.Model;
		return model != null && model.ZoneType == ZoneType.Hand;
	});

	public event Action<MtgZone> Hovered;

	protected override void Awake()
	{
		base.Awake();
		base.Layout = (_handLayout = new CardLayout_Hand(_collapsedFanLayout));
	}

	public void Shuffle()
	{
		_isDirty = false;
		base.LayoutNowInternal(_sorter.GetSortedCards(shuffleUnknown: true), layoutInstantly: true);
		_isUpdated = true;
	}

	protected override void OnPreLayout()
	{
		SortCardViews();
		base.OnPreLayout();
	}

	protected override void LayoutNowInternal(List<DuelScene_CDC> cardsToLayout, bool layoutInstantly = false)
	{
		base.LayoutNowInternal(_sorter.GetSortedCards(), layoutInstantly);
	}

	protected override Vector3 GetLayoutCenterPoint()
	{
		return _centerPointOverride;
	}

	protected override void HandleAddedCard(DuelScene_CDC cardView)
	{
		base.HandleAddedCard(cardView);
		_sorter.TrackCard(cardView);
		HandleAddedCardReveal(cardView);
	}

	protected virtual void HandleAddedCardReveal(DuelScene_CDC cardView)
	{
		if (_zoneCardViews.Contains(cardView))
		{
			OnCardUpdated(cardView);
		}
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		base.RemoveCard(cardView);
		_sorter.UnTrackCard(cardView);
		RemoveCardReveal(cardView);
	}

	protected virtual void RemoveCardReveal(DuelScene_CDC cardView)
	{
		ICardDataAdapter revealOverride = cardView.RevealOverride;
		if (revealOverride != null)
		{
			cardView.RemoveRevealOverride();
			_cardViewsByRevealedId.Remove(revealOverride.InstanceId);
			BankRevealedData(revealOverride);
		}
	}

	public override void OnCardUpdated(DuelScene_CDC cardView)
	{
		if (cardView.Model.IsDisplayedFaceDown)
		{
			if (cardView.RevealOverride == null && _revealDataBank.Count > 0 && _zoneCardViews.Contains(cardView))
			{
				ICardDataAdapter cardDataAdapter = null;
				foreach (ICardDataAdapter value in _revealDataBank.Values)
				{
					if (MayApplyBankedStamp(value.GrpId))
					{
						if (cardDataAdapter == null)
						{
							cardDataAdapter = value;
						}
						else if (value.GrpId == cardView.PreviousGrpId)
						{
							cardDataAdapter = value;
							break;
						}
					}
				}
				if (cardDataAdapter != null)
				{
					uint instanceId = cardDataAdapter.InstanceId;
					_revealDataBank.Remove(instanceId);
					_cardViewsByRevealedId[instanceId] = cardView;
					cardView.ApplyRevealOverride(cardDataAdapter);
					LayoutNow();
				}
			}
		}
		else if (cardView.RevealOverride != null)
		{
			ICardDataAdapter revealOverride = cardView.RevealOverride;
			cardView.RemoveRevealOverride();
			_cardViewsByRevealedId.Remove(revealOverride.InstanceId);
			BankRevealedData(revealOverride);
			LayoutNow();
		}
		else
		{
			foreach (DuelScene_CDC zoneCardView in _zoneCardViews)
			{
				if (zoneCardView.RevealOverride != null && zoneCardView.RevealOverride.GrpId == cardView.Model.GrpId && !zoneCardView.RevealOverride.Instance.HasPerpetualAddedAbility())
				{
					ICardDataAdapter revealOverride2 = zoneCardView.RevealOverride;
					zoneCardView.RemoveRevealOverride();
					_cardViewsByRevealedId.Remove(revealOverride2.InstanceId);
					BankRevealedData(revealOverride2);
					LayoutNow();
					break;
				}
			}
		}
		base.OnCardUpdated(cardView);
	}

	public void AddRevealData(ICardDataAdapter toAdd)
	{
		_allRevealData[toAdd.InstanceId] = toAdd;
	}

	public void AddRevealData(ICardDataAdapter toAdd, DuelScene_CDC card)
	{
		_allRevealData[toAdd.InstanceId] = toAdd;
		_cardViewsByRevealedId[toAdd.InstanceId] = card;
		card.ApplyRevealOverride(toAdd);
	}

	public void UpdateRevealedData(ICardDataAdapter toUpdate)
	{
		uint instanceId = toUpdate.InstanceId;
		_allRevealData[instanceId] = toUpdate;
		DuelScene_CDC value;
		if (_revealDataBank.ContainsKey(instanceId))
		{
			_revealDataBank[instanceId] = toUpdate;
		}
		else if (_cardViewsByRevealedId.TryGetValue(instanceId, out value))
		{
			value.ApplyRevealOverride(toUpdate);
		}
	}

	public void BankRevealedData(ICardDataAdapter toAdd)
	{
		_revealDataBank[toAdd.InstanceId] = toAdd;
		if (!MayApplyBankedStamp(toAdd.GrpId))
		{
			return;
		}
		List<DuelScene_CDC> list = _zoneCardViews.FindAll((DuelScene_CDC x) => x.VisualModel.IsDisplayedFaceDown);
		if (list.Count == 0)
		{
			return;
		}
		DuelScene_CDC duelScene_CDC = list.Find((DuelScene_CDC x) => x.PreviousGrpId == toAdd.GrpId);
		if (duelScene_CDC == null)
		{
			duelScene_CDC = list.Find((DuelScene_CDC x) => x.PreviousGrpId == 0);
			if (duelScene_CDC == null)
			{
				duelScene_CDC = list[0];
			}
		}
		_revealDataBank.Remove(toAdd.InstanceId);
		duelScene_CDC.ApplyRevealOverride(toAdd);
		_cardViewsByRevealedId[toAdd.InstanceId] = duelScene_CDC;
		LayoutNow();
	}

	private bool MayApplyBankedStamp(uint grpid)
	{
		int num = 0;
		int num2 = 0;
		foreach (DuelScene_CDC zoneCardView in _zoneCardViews)
		{
			if (!zoneCardView.Model.IsDisplayedFaceDown && zoneCardView.Model.GrpId == grpid)
			{
				num++;
			}
		}
		foreach (ICardDataAdapter value in _allRevealData.Values)
		{
			if (value.GrpId == grpid)
			{
				num2++;
			}
		}
		return num2 - num > 0;
	}

	public void RemoveRevealedModel(uint idToRemove)
	{
		_allRevealData.Remove(idToRemove);
		_revealDataBank.Remove(idToRemove);
		if (_cardViewsByRevealedId.TryGetValue(idToRemove, out var value))
		{
			value.RemoveRevealOverride();
			_cardViewsByRevealedId.Remove(idToRemove);
			LayoutNow();
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		if ((bool)eventData.pointerEnter)
		{
			Transform eventTran = eventData.pointerEnter.transform;
			if ((bool)eventTran)
			{
				DuelScene_CDC duelScene_CDC = _cardViews.Find((DuelScene_CDC x) => x.CollisionRoot == eventTran);
				if ((bool)duelScene_CDC && duelScene_CDC.Model != null)
				{
					_ = duelScene_CDC.Model.ZoneType;
				}
			}
		}
		Vector3 centerPointOverride = (OwnerIsControlledByLocalPlayer() ? _mindControlCenterPoint : Vector3.zero);
		SetCenterPointOverride(centerPointOverride);
		this.Hovered?.Invoke(_zoneModel);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		SetCenterPointOverride(Vector3.zero);
		this.Hovered?.Invoke(null);
	}

	private void SetCenterPointOverride(Vector3 pos)
	{
		if (_centerPointOverride != pos)
		{
			_centerPointOverride = pos;
			LayoutNow();
		}
	}

	protected bool OwnerIsControlledByLocalPlayer()
	{
		if (_zoneModel == null)
		{
			return false;
		}
		if (_zoneModel.Owner == null)
		{
			return false;
		}
		return _zoneModel.Owner.ControlledByLocalPlayer;
	}

	protected override void OnDestroy()
	{
		this.Hovered = null;
		base.OnDestroy();
	}
}
