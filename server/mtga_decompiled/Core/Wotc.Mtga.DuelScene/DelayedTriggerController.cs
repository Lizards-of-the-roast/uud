using AssetLookupTree;
using AssetLookupTree.Payloads.MiniCDC;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class DelayedTriggerController : IDelayedTriggerController
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly ICardViewManager _cardViewManager;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	public DelayedTriggerController(ICardDatabaseAdapter cardDatabase, ICardViewManager cardViewManager, ICardHolderProvider cardHolderProvider, AssetLookupSystem assetLookupSystem)
	{
		_cardDatabase = cardDatabase;
		_cardViewManager = cardViewManager ?? NullCardViewManager.Default;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_assetLookupSystem = assetLookupSystem;
	}

	private bool ShouldSuppressMiniCDC(MtgCardInstance delayedTrigger)
	{
		_assetLookupSystem.Blackboard.Clear();
		if (delayedTrigger.Parent != null)
		{
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(delayedTrigger.Parent.GrpId);
			CardData cardDataExtensive = new CardData(delayedTrigger, cardPrintingById);
			_assetLookupSystem.Blackboard.SetCardDataExtensive(cardDataExtensive);
		}
		AbilityPrintingData abilityPrintingData = null;
		if (delayedTrigger.ObjectSourceGrpId != delayedTrigger.Parent?.GrpId)
		{
			abilityPrintingData = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(delayedTrigger.ObjectSourceGrpId);
		}
		else if (delayedTrigger.Abilities.Count > 0)
		{
			abilityPrintingData = delayedTrigger.Abilities[0];
		}
		if (abilityPrintingData != null)
		{
			_assetLookupSystem.Blackboard.Ability = abilityPrintingData;
		}
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<DelayedTriggerOverride> loadedTree))
		{
			DelayedTriggerOverride payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				return payload.SuppressMiniCDC;
			}
		}
		return false;
	}

	public void AddDelayedTrigger(MtgCardInstance delayedTrigger)
	{
		if (!ShouldSuppressMiniCDC(delayedTrigger))
		{
			if (_cardViewManager.TryGetCardView(delayedTrigger.InstanceId, out var cardView))
			{
				cardView.SetModel(CardDataExtensions.CreateWithDatabase(delayedTrigger, _cardDatabase));
			}
			else
			{
				CreateMiniCDC(delayedTrigger);
			}
		}
	}

	public void UpdateDelayedTrigger(MtgCardInstance delayedTrigger)
	{
		if (_cardViewManager.TryGetCardView(delayedTrigger.InstanceId, out var cardView))
		{
			cardView.SetModel(CardDataExtensions.CreateWithDatabase(delayedTrigger, _cardDatabase));
		}
	}

	public void RemoveDelayedTrigger(uint delayedTriggerId)
	{
		_cardViewManager.DeleteCard(delayedTriggerId);
	}

	private void CreateMiniCDC(MtgCardInstance delayedTrigger)
	{
		GREPlayerNum playerNum = ((delayedTrigger.Owner != null) ? delayedTrigger.Owner.ClientPlayerEnum : GREPlayerNum.Invalid);
		if (_cardHolderProvider.TryGetCardHolder(playerNum, CardHolderType.Command, out var cardHolder) && cardHolder is IGameEffectController gameEffectController)
		{
			DuelScene_CDC card = _cardViewManager.CreateCardView(CardDataExtensions.CreateWithDatabase(delayedTrigger, _cardDatabase));
			gameEffectController.AddGameEffect(card, GameEffectType.DelayedTrigger);
		}
	}
}
