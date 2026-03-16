using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public class MutateOptionalActionBrowser : CardBrowserBase, IGroupedCardProvider
{
	private readonly OptionalActionWorkflow_Mutate _workflow;

	private readonly List<CardData> _overPileData = new List<CardData>(2);

	private readonly List<CardData> _underPileData = new List<CardData>(2);

	private readonly List<DuelScene_CDC> _overPileViews = new List<DuelScene_CDC>(2);

	private readonly List<DuelScene_CDC> _underPileViews = new List<DuelScene_CDC>(2);

	private Vector3 _overPilePosition = new Vector3(3.5f, 0f, 0f);

	private Vector3 _underPilePosition = new Vector3(-3.5f, 0f, 0f);

	private Vector3 _cardViewOffset = new Vector3(0.75f, -0.75f, -0.25f);

	private Vector3 _cardGroupAScale = Vector3.one;

	private Vector3 _cardGroupBScale = Vector3.one;

	private readonly HashSet<string> _fakeCardKeys = new HashSet<string>();

	private CardLayout_MultiLayout _multiLayout;

	public MutateOptionalActionBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		_workflow = cardBrowserProvider as OptionalActionWorkflow_Mutate;
		base.AllowsHoverInteractions = true;
		CreateFakeCardDataForMutationOptions();
	}

	private void CreateFakeCardDataForMutationOptions()
	{
		MtgCardInstance mtgCardInstance = _workflow.RecipientModel.Printing.CreateInstance();
		mtgCardInstance.Abilities = new List<AbilityPrintingData>();
		mtgCardInstance.SkinCode = _workflow.RecipientModel.SkinCode;
		mtgCardInstance.FaceDownState = FaceDownState.CopyFaceDownState(_workflow.RecipientModel.Instance.FaceDownState);
		if (mtgCardInstance.FaceDownState.IsFaceDown)
		{
			mtgCardInstance.BaseGrpId = _workflow.RecipientModel.Instance.BaseGrpId;
			mtgCardInstance.OverlayGrpId = _workflow.RecipientModel.Instance.OverlayGrpId;
		}
		CardData item = SetupBackCard(mtgCardInstance, _workflow.RecipientModel.Printing);
		_overPileData.Add(item);
		MtgCardInstance mtgCardInstance2 = _workflow.SourceModel.Printing.CreateInstance();
		mtgCardInstance2.SkinCode = _workflow.SourceModel.SkinCode;
		mtgCardInstance2.BaseGrpId = _workflow.RecipientModel.Instance.BaseGrpId;
		mtgCardInstance2.OverlayGrpId = _workflow.SourceModel.Instance.GrpId;
		mtgCardInstance2.Counters.Clear();
		mtgCardInstance2.Abilities = GatherAllAbilities(_workflow.RecipientModel.Instance, _workflow.SourceModel.Printing, _gameManager.CardDatabase.CardDataProvider);
		mtgCardInstance2.MutationChildren = GatherAllMutationsOver(_workflow.RecipientModel.Instance, _workflow.SourceModel.Instance);
		mtgCardInstance2.MutationChildrenIds.UnionWith(mtgCardInstance2.MutationChildren.ConvertAll((MtgCardInstance x) => x.InstanceId));
		CardData item2 = new CardData(mtgCardInstance2, _workflow.SourceModel.Printing);
		_overPileData.Add(item2);
		MtgCardInstance mtgCardInstance3 = _workflow.SourceModel.Printing.CreateInstance();
		mtgCardInstance3.Abilities = new List<AbilityPrintingData>();
		mtgCardInstance3.SkinCode = _workflow.SourceModel.SkinCode;
		CardData item3 = SetupBackCard(mtgCardInstance3, _workflow.SourceModel.Printing);
		_underPileData.Add(item3);
		MtgCardInstance mtgCardInstance4 = _workflow.RecipientModel.Printing.CreateInstance();
		mtgCardInstance4.SkinCode = _workflow.RecipientModel.SkinCode;
		mtgCardInstance4.FaceDownState = FaceDownState.CopyFaceDownState(_workflow.RecipientModel.Instance.FaceDownState);
		mtgCardInstance4.BaseGrpId = _workflow.RecipientModel.Instance.BaseGrpId;
		mtgCardInstance4.OverlayGrpId = _workflow.RecipientModel.Instance.OverlayGrpId;
		mtgCardInstance4.Counters.Clear();
		if (mtgCardInstance4.FaceDownState.ReasonFaceDown == ReasonFaceDown.Morph)
		{
			mtgCardInstance4.OverlayGrpId = _workflow.RecipientModel.Instance.OverlayGrpId;
		}
		mtgCardInstance4.Abilities = GatherAllAbilities(_workflow.RecipientModel.Instance, _workflow.SourceModel.Printing, _gameManager.CardDatabase.CardDataProvider);
		mtgCardInstance4.MutationChildren = GatherAllMutationsUnder(_workflow.RecipientModel.Instance, _workflow.SourceModel.Instance);
		mtgCardInstance4.MutationChildrenIds.UnionWith(mtgCardInstance4.MutationChildren.ConvertAll((MtgCardInstance x) => x.InstanceId));
		CardData item4 = new CardData(mtgCardInstance4, _workflow.RecipientModel.Printing);
		_underPileData.Add(item4);
		static List<AbilityPrintingData> GatherAllAbilities(MtgCardInstance battlefieldInstance, CardPrintingData newCard, ICardDataProvider cdb)
		{
			List<AbilityPrintingData> list = new List<AbilityPrintingData>(10);
			if (battlefieldInstance.OverlayGrpId.HasValue)
			{
				CardPrintingData cardPrintingById = cdb.GetCardPrintingById(battlefieldInstance.OverlayGrpId.Value);
				if (cardPrintingById != null)
				{
					list.AddRange(cardPrintingById.Abilities);
					if (cardPrintingById.Abilities.Exists((AbilityPrintingData x) => x.BaseId == 203))
					{
						CardPrintingData cardPrintingById2 = cdb.GetCardPrintingById(battlefieldInstance.BaseGrpId);
						if (cardPrintingById2 != null)
						{
							foreach (AbilityPrintingData basePrintingAbility in cardPrintingById2.Abilities)
							{
								if (battlefieldInstance.Abilities.Exists((AbilityPrintingData x) => x.Id == basePrintingAbility.Id))
								{
									list.Add(basePrintingAbility);
								}
							}
						}
					}
					goto IL_00fa;
				}
			}
			CardPrintingData cardPrintingById3 = cdb.GetCardPrintingById(battlefieldInstance.GrpId);
			if (cardPrintingById3 != null)
			{
				list.AddRange(cardPrintingById3.Abilities);
			}
			goto IL_00fa;
			IL_00fa:
			if (newCard != null)
			{
				list.AddRange(newCard.Abilities);
			}
			foreach (MtgCardInstance mutationChild in battlefieldInstance.MutationChildren)
			{
				if (battlefieldInstance.OverlayGrpId != mutationChild.GrpId)
				{
					CardPrintingData cardPrintingById4 = cdb.GetCardPrintingById(mutationChild.GrpId);
					if (cardPrintingById4 != null)
					{
						list.AddRange(cardPrintingById4.Abilities);
					}
				}
			}
			return list;
		}
		static List<MtgCardInstance> GatherAllMutationsOver(MtgCardInstance battlefieldInstance, MtgCardInstance newCard)
		{
			List<MtgCardInstance> list = new List<MtgCardInstance>(5);
			foreach (MtgCardInstance mutationChild2 in battlefieldInstance.MutationChildren)
			{
				list.Add(mutationChild2);
			}
			list.Add(newCard);
			return list;
		}
		static List<MtgCardInstance> GatherAllMutationsUnder(MtgCardInstance battlefieldInstance, MtgCardInstance newCard)
		{
			List<MtgCardInstance> list = new List<MtgCardInstance>(5);
			uint? num = null;
			if (battlefieldInstance.OverlayGrpId.HasValue && battlefieldInstance.MutationChildren.Count > 0 && battlefieldInstance.MutationChildren.Exists((MtgCardInstance x) => x.GrpId == battlefieldInstance.OverlayGrpId))
			{
				num = battlefieldInstance.OverlayGrpId.Value;
			}
			foreach (MtgCardInstance mutationChild3 in battlefieldInstance.MutationChildren)
			{
				if (num.HasValue && mutationChild3.GrpId == num.Value)
				{
					num = null;
				}
				else
				{
					list.Add(mutationChild3);
				}
			}
			list.Add(newCard);
			return list;
		}
		static CardData SetupBackCard(MtgCardInstance instance, CardPrintingData printing)
		{
			CardPrintingRecord record = printing.Record;
			IReadOnlyList<(uint, uint)> abilityIds = Array.Empty<(uint, uint)>();
			return new CardData(instance, new CardPrintingData(printing, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, abilityIds)))
			{
				RulesTextOverride = new RawTextOverride(" ")
			};
		}
	}

	protected override void InitCardHolder()
	{
		base.InitCardHolder();
		cardHolder.CardGroupProvider = this;
	}

	protected override ICardLayout GetCardHolderLayout()
	{
		return _multiLayout;
	}

	public List<List<DuelScene_CDC>> GetCardGroups()
	{
		if (base.IsVisible)
		{
			return new List<List<DuelScene_CDC>>(2) { _overPileViews, _underPileViews };
		}
		List<DuelScene_CDC> item = new List<DuelScene_CDC>(0);
		return new List<List<DuelScene_CDC>> { item, item };
	}

	protected override void InitializeUIElements()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(_workflow.GetHeaderText());
		component.SetSubheaderText(_workflow.GetSubHeaderText());
		GameObject browserElement = GetBrowserElement("CardGroupAMarker");
		_overPilePosition = browserElement.transform.localPosition;
		_cardGroupAScale = browserElement.transform.localScale;
		GameObject browserElement2 = GetBrowserElement("CardGroupBMarker");
		_underPilePosition = browserElement2.transform.localPosition;
		_cardGroupBScale = browserElement.transform.localScale;
		_cardViewOffset = GetBrowserElement("CardSpacingMarker").transform.localPosition;
		CreateMultiLayout();
		base.InitializeUIElements();
	}

	private void CreateMultiLayout()
	{
		_multiLayout = new CardLayout_MultiLayout(new List<ICardLayout>(2)
		{
			new CardLayout_Horizontal
			{
				Spacing = _cardViewOffset,
				Scale = _cardGroupAScale
			},
			new CardLayout_Horizontal
			{
				Spacing = _cardViewOffset,
				Scale = _cardGroupBScale
			}
		}, new List<Vector3>(2) { _overPilePosition, _underPilePosition }, new List<Quaternion>(2)
		{
			Quaternion.identity,
			Quaternion.identity
		}, this);
	}

	protected override void ReleaseCards()
	{
		foreach (string fakeCardKey in _fakeCardKeys)
		{
			entityViewManager?.DeleteFakeCard(fakeCardKey);
		}
		_overPileViews.Clear();
		_underPileViews.Clear();
		cardHolder.CardGroupProvider = null;
		base.ReleaseCards();
	}

	protected override void SetupCards()
	{
		cardViews = _workflow.GetCardsToDisplay();
		_fakeCardKeys.Add("OverPile_Back");
		_fakeCardKeys.Add("OverPile_Front");
		_overPileViews.Add(entityViewManager.CreateFakeCard("OverPile_Back", _overPileData[0]));
		_overPileViews.Add(entityViewManager.CreateFakeCard("OverPile_Front", _overPileData[1]));
		cardViews.AddRange(_overPileViews);
		_fakeCardKeys.Add("UnderPile_Back");
		_fakeCardKeys.Add("UnderPile_Front");
		_underPileViews.Add(entityViewManager.CreateFakeCard("UnderPile_Back", _underPileData[0]));
		_underPileViews.Add(entityViewManager.CreateFakeCard("UnderPile_Front", _underPileData[1]));
		cardViews.AddRange(_underPileViews);
		MoveCardViewsToBrowser(cardViews);
	}
}
