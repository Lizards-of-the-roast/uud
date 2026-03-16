using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ChoiceResultUXEvent_Permanent : UXEvent
{
	private readonly ChoiceResultEvent _choiceResult;

	private readonly GameManager _gameManager;

	private readonly IVfxProvider _vfxProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly EntityViewManager _viewManager;

	private readonly ICardDataProvider _cardDatabase;

	private DuelScene_CDC _affectorCdc;

	private List<DuelScene_CDC> _optionCdcs = new List<DuelScene_CDC>();

	private List<DuelScene_CDC> _valueCdcs = new List<DuelScene_CDC>();

	public override bool IsBlocking => true;

	public ChoiceResultUXEvent_Permanent(ChoiceResultEvent cre, GameManager gameManager)
	{
		_choiceResult = cre;
		_gameManager = gameManager;
		_vfxProvider = gameManager.VfxProvider;
		_assetLookupSystem = _gameManager.AssetLookupSystem;
		_viewManager = _gameManager.ViewManager;
		_cardDatabase = _gameManager.CardDatabase.CardDataProvider;
	}

	public override bool CanExecute(List<UXEvent> currentlyRunningEvents)
	{
		return !currentlyRunningEvents.Exists((UXEvent x) => x.HasWeight);
	}

	public override void Execute()
	{
		_viewManager.TryGetCardView(_choiceResult.AffectorId, out _affectorCdc);
		foreach (uint choiceOption in _choiceResult.ChoiceOptions)
		{
			if (_viewManager.TryGetCardView(choiceOption, out var cardView))
			{
				_optionCdcs.Add(cardView);
			}
		}
		foreach (uint choiceValue in _choiceResult.ChoiceValues)
		{
			if (_viewManager.TryGetCardView(choiceValue, out var cardView2))
			{
				_valueCdcs.Add(cardView2);
			}
		}
		if (_valueCdcs.Count > 0)
		{
			_assetLookupSystem.Blackboard.Clear();
			if ((bool)_affectorCdc)
			{
				if (_gameManager.LatestGameState.Designations != null)
				{
					uint affectedId = _gameManager.LatestGameState.Designations.FirstOrDefault((DesignationData x) => x.Type == Designation.Ringbearer && _choiceResult.ChoiceValues.Contains(x.AffectedId)).AffectedId;
					if (_gameManager.LatestGameState.TryGetCard(affectedId, out var card))
					{
						_assetLookupSystem.Blackboard.LayeredEffects = card.LayeredEffects;
						if (!_cardDatabase.TryGetCardPrintingById(card.GrpId, out var card2))
						{
							card2 = card.ObjectSourcePrinting;
						}
						_assetLookupSystem.Blackboard.SetCardDataExtensive(new CardData(card, card2));
					}
				}
				if (_assetLookupSystem.Blackboard.CardData == null)
				{
					_assetLookupSystem.Blackboard.SetCardDataExtensive(_affectorCdc.Model);
				}
				_assetLookupSystem.Blackboard.CardHolderType = _affectorCdc.HolderType;
			}
			_assetLookupSystem.Blackboard.HighlightType = ConvertGreHighlightToClientHighlight(_choiceResult.ChoiceSentiment);
			ChoiceResult payload = _assetLookupSystem.TreeLoader.LoadTree<ChoiceResult>().GetPayload(_assetLookupSystem.Blackboard);
			_assetLookupSystem.Blackboard.Clear();
			if (payload != null)
			{
				foreach (DuelScene_CDC valueCdc in _valueCdcs)
				{
					_vfxProvider.PlayVFX(payload.VfxData, (_affectorCdc != null) ? _affectorCdc.Model : null, (valueCdc != null) ? valueCdc.Model.Instance : null);
					if ((bool)valueCdc && payload.SfxData.AudioEvents.Count > 0)
					{
						AudioManager.PlayAudio(payload.SfxData.AudioEvents, valueCdc.EffectsRoot.gameObject);
					}
				}
			}
		}
		Complete();
	}

	private static HighlightType ConvertGreHighlightToClientHighlight(Wotc.Mtgo.Gre.External.Messaging.HighlightType greHighlight)
	{
		return greHighlight switch
		{
			Wotc.Mtgo.Gre.External.Messaging.HighlightType.None => HighlightType.None, 
			Wotc.Mtgo.Gre.External.Messaging.HighlightType.Cold => HighlightType.Cold, 
			Wotc.Mtgo.Gre.External.Messaging.HighlightType.Tepid => HighlightType.Tepid, 
			Wotc.Mtgo.Gre.External.Messaging.HighlightType.Hot => HighlightType.Hot, 
			Wotc.Mtgo.Gre.External.Messaging.HighlightType.Counterspell => HighlightType.AutoPay, 
			Wotc.Mtgo.Gre.External.Messaging.HighlightType.Random => HighlightType.Selected, 
			_ => HighlightType.None, 
		};
	}
}
