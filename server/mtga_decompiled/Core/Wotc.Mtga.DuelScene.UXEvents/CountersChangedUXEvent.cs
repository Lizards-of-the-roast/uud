using System;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card.Planeswalker;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class CountersChangedUXEvent : UXEvent
{
	public readonly CounterType CounterType;

	private readonly GameManager _gameManager;

	private readonly IEntityViewManager _viewManager;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly AssetLookupSystem _altSystem;

	private readonly IVfxProvider _vfxProvider;

	public uint AffectorId { get; private set; }

	public uint AffectedId { get; private set; }

	public MtgEntity Affected { get; private set; }

	public MtgEntity Affector { get; private set; }

	public int ChangeAmount { get; private set; }

	public int OldAmount { get; private set; }

	public CountersChangedUXEvent(uint affectorId, uint affectedId, MtgEntity affector, MtgEntity affected, int oldCount, int changeAmount, CounterType counterType, GameManager gameManager)
	{
		AffectorId = affectorId;
		AffectedId = affectedId;
		Affector = affector;
		Affected = affected;
		ChangeAmount = changeAmount;
		OldAmount = oldCount;
		CounterType = counterType;
		_gameManager = gameManager;
		_viewManager = gameManager.ViewManager;
		_cardDatabase = gameManager.CardDatabase;
		_altSystem = gameManager.AssetLookupSystem;
		_vfxProvider = gameManager.VfxProvider;
	}

	public override void Execute()
	{
		MtgEntity affected = Affected;
		DuelScene_CDC cardView;
		if (!(affected is MtgCardInstance mtgCardInstance))
		{
			if (affected is MtgPlayer mtgPlayer && _viewManager.TryGetAvatarById(mtgPlayer.InstanceId, out var avatar))
			{
				avatar.UpdateCounters(mtgPlayer.Counters);
			}
		}
		else if (_viewManager.TryGetCardView(mtgCardInstance.InstanceId, out cardView))
		{
			if (ChangeAmount < 0 && CounterType == CounterType.Loyalty && AffectorId != AffectedId && Affector is MtgCardInstance mtgCardInstance2 && mtgCardInstance2.ParentId != AffectedId && cardView.Model.Counters.ContainsKey(CounterType.Loyalty) && cardView.Model.Counters[CounterType.Loyalty] > 0 && _altSystem.TreeLoader.TryLoadTree(out AssetLookupTree<HurtVfx> loadedTree) && _altSystem.TreeLoader.TryLoadTree(out AssetLookupTree<HurtSfx> loadedTree2))
			{
				_altSystem.Blackboard.Clear();
				_altSystem.Blackboard.SetCardDataExtensive(cardView.Model);
				_altSystem.Blackboard.CounterType = CounterType.Loyalty;
				_altSystem.Blackboard.CardHolderType = CardHolderType.Battlefield;
				_altSystem.Blackboard.DamageRecipientEntity = cardView.Model.Instance;
				_altSystem.Blackboard.DamageAmount = Math.Abs(ChangeAmount);
				HurtVfx payload = loadedTree.GetPayload(_altSystem.Blackboard);
				if (payload != null)
				{
					foreach (VfxData vfxData in payload.VfxDatas)
					{
						_vfxProvider.PlayVFX(vfxData, cardView.Model);
					}
				}
				HurtSfx payload2 = loadedTree2.GetPayload(_altSystem.Blackboard);
				if (payload2 != null)
				{
					AudioManager.PlayAudio(payload2.SfxData.AudioEvents, cardView.gameObject);
				}
				_altSystem.Blackboard.Clear();
			}
			CardData data = CardDataExtensions.CreateWithDatabase(mtgCardInstance, _cardDatabase);
			cardView.AddUpdatedProperty(PropertyType.Counters);
			cardView.SetModel(data);
			if (OldAmount == 0 && ChangeAmount > 0)
			{
				CDCPart_Counters cDCPart_Counters = cardView.FindPart<CDCPart_Counters>(AnchorPointType.Counters);
				if ((bool)cDCPart_Counters)
				{
					cDCPart_Counters.PlayCounterAddedFX(CounterType, _gameManager);
				}
			}
		}
		Complete();
	}
}
