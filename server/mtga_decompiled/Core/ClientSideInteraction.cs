using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Browser;
using GreClient.CardData;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using MovementSystem;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public static class ClientSideInteraction
{
	private static readonly IModalGreInteractionComparer _modalActionComparer = new ModalGreInteractionComparer();

	private static IBrowser _modalSelectionBrowser = null;

	private static DuelScene_CDC _overriddenCard;

	private static ICardDataAdapter _originalCardData;

	private static ICardDataAdapter _overrideCardData;

	public static readonly Dictionary<DuelScene_CDC, GreInteraction> ClientSideChoiceMap = new Dictionary<DuelScene_CDC, GreInteraction>(10);

	private const float IdealPointScale = 0.33f;

	public static void HandleActions(IEntityView entity, List<GreInteraction> interactions, Action<GreInteraction> submitAction, GameManager gameManager, bool showConfirm)
	{
		interactions = GetTrimmedInteractionList(interactions, gameManager.CardDatabase.AbilityDataProvider);
		if (!interactions.TrueForAll((GreInteraction x) => !x.IsActive))
		{
			if (entity is DuelScene_CDC cdc)
			{
				HandleCardActions(cdc, interactions, submitAction, gameManager, showConfirm);
			}
			else if (entity is DuelScene_AvatarView)
			{
				HandlePlayerActions(interactions, submitAction);
			}
		}
	}

	private static void HandleCardActions(DuelScene_CDC cdc, List<GreInteraction> interactions, Action<GreInteraction> submitAction, GameManager gameManager, bool showConfirm)
	{
		if (interactions.Count == 1)
		{
			GreInteraction greInteraction = interactions[0];
			Wotc.Mtgo.Gre.External.Messaging.Action greAction = greInteraction.GreAction;
			if (showConfirm && HasPlayWarnings(cdc, greAction))
			{
				ShowConfirmation(cdc, gameManager, greAction, submitAction);
			}
			else if (ActionsAvailableWorkflowUtils_ColorPicker.CanUseColorPicker(gameManager.CardDatabase.AbilityDataProvider, greInteraction))
			{
				ActionsAvailableWorkflowUtils_ColorPicker.ShowManaColorSelection(cdc, interactions, submitAction, gameManager.UIManager.ManaColorSelector, gameManager.CardDatabase.AbilityDataProvider, gameManager.CardHolderManager);
			}
			else
			{
				submitAction(greInteraction);
			}
		}
		else if (ActionsAvailableWorkflowUtils_ColorPicker.CanUseColorPicker(gameManager.CardDatabase.AbilityDataProvider, interactions.ToArray()))
		{
			if (ActionsAvailableWorkflowUtils_ColorPicker.HasMultipleTriggeredAbilities(gameManager.CardDatabase.AbilityDataProvider, gameManager.CardDatabase.CardDataProvider, interactions.ToArray()))
			{
				ActionsAvailableWorkflowUtils_ColorPicker.ShowSourceInstanceManaColorSelection(cdc, interactions, submitAction, gameManager.UIManager.ManaColorSelector, gameManager.CardDatabase.AbilityDataProvider, gameManager.CardHolderManager);
			}
			else
			{
				ActionsAvailableWorkflowUtils_ColorPicker.ShowManaColorSelection(cdc, interactions, submitAction, gameManager.UIManager.ManaColorSelector, gameManager.CardDatabase.AbilityDataProvider, gameManager.CardHolderManager);
			}
		}
		else
		{
			ModalSelection(cdc, interactions, ShowSecondaryConfirmationIfNecessary, gameManager);
		}
		void ShowSecondaryConfirmationIfNecessary(GreInteraction chosenAction)
		{
			if (showConfirm && ShouldForceSecondaryConfirmation(chosenAction, cdc))
			{
				ShowConfirmation(cdc, gameManager, chosenAction.GreAction, submitAction);
			}
			else
			{
				submitAction?.Invoke(chosenAction);
			}
		}
	}

	private static void HandlePlayerActions(IReadOnlyList<GreInteraction> interactions, Action<GreInteraction> submitAction)
	{
		GreInteraction greInteraction = interactions.LastOrDefault((GreInteraction x) => x.IsActive);
		if (greInteraction != null)
		{
			submitAction?.Invoke(greInteraction);
		}
	}

	private static bool ShouldForceSecondaryConfirmation(GreInteraction interaction, DuelScene_CDC cdc)
	{
		if (interaction != null && interaction.Type == ActionType.Activate && interaction.GreAction != null && interaction.GreAction.Highlight == Wotc.Mtgo.Gre.External.Messaging.HighlightType.Cold)
		{
			return !cdc.Model.Instance.PlayWarnings.Exists((ShouldntPlayData warning) => warning.Reasons[0] == ShouldntPlayData.ReasonType.RedundantActivation);
		}
		return false;
	}

	private static void ModalSelection(DuelScene_CDC cdc, List<GreInteraction> interactions, Action<GreInteraction> submitInteraction, GameManager gameManager)
	{
		IContext context = gameManager.Context;
		ISplineMovementSystem splineMovementSystem = gameManager.SplineMovementSystem;
		ICardHolderProvider cardHolderProvider = context.Get<ICardHolderProvider>() ?? NullCardHolderProvider.Default;
		ICardMovementController cardMovementController = context.Get<ICardMovementController>() ?? NullCardMovementController.Default;
		ICardDatabaseAdapter cardDatabase = gameManager.CardDatabase;
		MtgGameState latestGameState = gameManager.LatestGameState;
		AssetLookupSystem assetLookupSystem = gameManager.AssetLookupSystem;
		ICardBuilder<DuelScene_CDC> cardBuilder = context.Get<ICardBuilder<DuelScene_CDC>>() ?? NullCardBuilder<DuelScene_CDC>.Default;
		IModalBrowserCardHeaderProvider modalBrowserCardHeaderProvider = context.Get<IModalBrowserCardHeaderProvider>() ?? NullBrowserCardHeaderProvider.Default;
		IAutoSubmitActionCalculator obj = context.Get<IAutoSubmitActionCalculator>() ?? NullAutoSubmitActionCalculator.Default;
		IListFilter<GreInteraction> listFilter = context.Get<IListFilter<GreInteraction>>();
		gameManager.BrowserManager.CloseCurrentBrowser();
		ClientSideChoiceMap.Clear();
		List<GreInteraction> trimmedInteractions = new List<GreInteraction>(interactions);
		listFilter.Filter(ref trimmedInteractions);
		if (obj.TryGetAutoSubmitAction(trimmedInteractions, out var toSubmit))
		{
			submitInteraction(toSubmit);
			return;
		}
		_modalActionComparer.SetCompareParams(cdc.Model);
		trimmedInteractions.Sort(_modalActionComparer);
		_modalActionComparer.ClearCompareParams();
		List<DuelScene_CDC> list = new List<DuelScene_CDC>();
		List<DuelScene_CDC> list2 = new List<DuelScene_CDC>();
		Dictionary<DuelScene_CDC, HighlightType> dictionary = new Dictionary<DuelScene_CDC, HighlightType>();
		Dictionary<DuelScene_CDC, Wotc.Mtgo.Gre.External.Messaging.Action> dictionary2 = new Dictionary<DuelScene_CDC, Wotc.Mtgo.Gre.External.Messaging.Action>();
		Dictionary<DuelScene_CDC, AbilityPrintingData> dictionary3 = new Dictionary<DuelScene_CDC, AbilityPrintingData>();
		List<DuelScene_CDC> list3 = new List<DuelScene_CDC>();
		Dictionary<DuelScene_CDC, int> dictionary4 = new Dictionary<DuelScene_CDC, int>();
		HashSet<uint> hashSet = new HashSet<uint>();
		Dictionary<DuelScene_CDC, GreInteraction> callbackMapping = new Dictionary<DuelScene_CDC, GreInteraction>();
		bool cardAsButton = false;
		List<uint> abilitiesToRemove = new List<uint>(1);
		abilitiesToRemove.AddRange(from x in trimmedInteractions
			where x.Type == ActionType.Activate
			select x.GreAction.AbilityGrpId);
		abilitiesToRemove.AddRange(from x in trimmedInteractions
			where x.Type == ActionType.Cast
			select x.GreAction.AlternativeGrpId);
		abilitiesToRemove.AddRange(from x in trimmedInteractions
			where x.Type == ActionType.Special
			select x.GreAction.AbilityGrpId);
		CardData originalCardData = (CardData)cdc.Model;
		MtgCardInstance copy = originalCardData.Instance.GetCopy();
		CardPrintingData printing = originalCardData.Printing;
		IReadOnlyList<(uint, uint)> readOnlyList = printing.AbilityIds;
		IReadOnlyList<uint> readOnlyList2 = printing.LinkedFaceGrpIds;
		if (abilitiesToRemove.Any())
		{
			copy.Abilities.RemoveAll((AbilityPrintingData x) => abilitiesToRemove.Contains(x.Id));
			readOnlyList = printing.AbilityIds.Where(((uint Id, uint TextId) x) => !abilitiesToRemove.Contains(x.Id)).ToArray();
		}
		if (CardUtilities.IsModalChildFacet(printing.LinkedFaceType))
		{
			copy.LinkedFaceInstances.Clear();
			readOnlyList2 = Array.Empty<uint>();
		}
		foreach (AbilityPrintingData addedAbility in originalCardData.AddedAbilities)
		{
			originalCardData.PrintingAbilityIds.Contains(addedAbility.Id);
		}
		if (trimmedInteractions.Exists((GreInteraction x) => x.GreAction.AlternativeGrpId == 37 || x.GreAction.AlternativeGrpId == 307) && trimmedInteractions.Count > 1 && (from x in trimmedInteractions
			group x by x.GreAction.InstanceId).Exists((IGrouping<uint, GreInteraction> x) => x.Count() > 1))
		{
			copy.Abilities.RemoveAll((AbilityPrintingData x) => x.BaseId == 37 || x.BaseId == 307);
		}
		List<IClientSideOptionModelMutator> list4 = new List<IClientSideOptionModelMutator>();
		CardPrintingRecord record = printing.Record;
		IReadOnlyList<(uint, uint)> abilityIds = readOnlyList;
		IReadOnlyList<uint> linkedFaceGrpIds = readOnlyList2;
		CardPrintingData cardPrintingData = new CardPrintingData(printing, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, abilityIds, null, linkedFaceGrpIds));
		foreach (GreInteraction item2 in trimmedInteractions)
		{
			Wotc.Mtgo.Gre.External.Messaging.Action action = item2.GreAction;
			DuelScene_CDC duelScene_CDC = null;
			IReadOnlyList<ManaQuantity> readOnlyList3 = action.ConvertedActionManaCost();
			CardData cardData = null;
			if ((action.IsPlayAction() || action.IsCastAction()) && action.AlternativeGrpId == 0)
			{
				CardData normalModalCardData = ModalCardUtils.GetNormalModalCardData(action, originalCardData, copy, cardPrintingData, readOnlyList3, cardDatabase, trimmedInteractions);
				duelScene_CDC = ModalCardUtils.GetModalCDC(item2.Type, normalModalCardData, cdc, splineMovementSystem, cardBuilder, cardHolderProvider, ref cardAsButton);
				if (cardAsButton && _overriddenCard == null)
				{
					_overriddenCard = cdc;
					_originalCardData = originalCardData;
					_overrideCardData = normalModalCardData;
				}
			}
			else
			{
				AbilityPrintingData abilityPrintingData = null;
				if (ModalCardUtils.IsTurnFaceUpAbilityButNotKeyword(item2.Type, action.AbilityGrpId, in originalCardData.Instance.FaceDownState))
				{
					(cardData, abilityPrintingData) = ModalCardUtils.CreateTurnFaceUpAbilityModal(originalCardData, new List<ManaRequirement>(action.ManaCost), cardDatabase, action.AbilityGrpId);
				}
				else if (action.AbilityGrpId == 349 || action.AbilityGrpId == 350)
				{
					cardData = ModalCardUtils.CreateUnlockRoomSubCard(action, originalCardData, cardDatabase, readOnlyList3, gameManager.CardDatabase.ClientLocProvider);
				}
				else if (action.AlternativeGrpId != 0)
				{
					(cardData, abilityPrintingData) = AlternativeCastModalCardUtils.GetAlternativeCastModalData(action, originalCardData, copy, cardPrintingData, readOnlyList3, latestGameState, cardDatabase);
				}
				else if (item2.Type == ActionType.Activate || item2.Type == ActionType.ActivateMana || item2.Type == ActionType.MakePayment || item2.Type == ActionType.Special)
				{
					AddedAbilityData addedAbilityData = originalCardData.Instance.AbilityAdders.Find((AddedAbilityData x) => x.AbilityId == action.AbilityGrpId);
					bool num = readOnlyList.Exists(((uint, uint) x) => x.Item1 == action.AbilityGrpId);
					CardData cardData2 = null;
					if ((!num || hashSet.Contains(action.AbilityGrpId)) && addedAbilityData.SourceId != 0)
					{
						MtgCardInstance cardById = latestGameState.GetCardById(addedAbilityData.SourceId);
						if (cardById != null && cardById.CatalogId == WellKnownCatalogId.None)
						{
							cardData2 = CardDataExtensions.CreateWithDatabase(cardById, cardDatabase);
						}
						else if (addedAbilityData.SourceGrpId >= 11)
						{
							CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(addedAbilityData.SourceGrpId);
							if (cardPrintingById != null)
							{
								cardData2 = new CardData(cardPrintingById.CreateInstance(), cardPrintingById);
							}
						}
					}
					hashSet.Add(action.AbilityGrpId);
					uint abilityGrpId = action.AbilityGrpId;
					if (cardData2 != null)
					{
						abilityPrintingData = cardData2.Abilities.FirstOrDefault((AbilityPrintingData x) => x.Id == abilityGrpId);
					}
					if (abilityPrintingData == null)
					{
						abilityPrintingData = originalCardData.Abilities.FirstOrDefault((AbilityPrintingData x) => x.Id == abilityGrpId);
					}
					if (abilityPrintingData == null)
					{
						abilityPrintingData = gameManager.CardDatabase.AbilityDataProvider.GetAbilityPrintingById(abilityGrpId);
					}
					if (abilityPrintingData == null)
					{
						gameManager.Logger.ModalSelectionError(action.AbilityGrpId, originalCardData.GrpId, cardDatabase.GreLocProvider.GetLocalizedText(originalCardData.TitleId), "Invalid ability grpId on interaction");
						continue;
					}
					CardData cardData3 = cardData2 ?? originalCardData;
					cardData = CardDataExtensions.CreateAbilityCard(abilityPrintingData, cardData3, cardDatabase);
					cardData.Instance.ParentId = cardData3.InstanceId;
					cardData.Instance.Parent = cardData3.Instance.GetCopy();
					cardData.Instance.SkinCode = cardData3.SkinCode;
					cardData.Instance.SleeveCode = cardData3.SleeveCode;
					cardData.Instance.ObjectSourceGrpId = cardData3.GrpId;
					(cardData.RulesTextOverride as AbilityTextOverride)?.AddSubstitution("Cost", "{" + ManaUtilities.ConvertToOldSchoolManaText(ManaUtilities.ConvertManaCostsToList(action.ManaCost)) + "}");
					if (abilityPrintingData.BaseId == 208)
					{
						AbilityPrintingData abilityPrintingById = cardDatabase.AbilityDataProvider.GetAbilityPrintingById(208u);
						string localizedText = cardDatabase.GreLocProvider.GetLocalizedText(abilityPrintingById.TextId);
						string item = cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(0u, abilityPrintingData.Id, Array.Empty<uint>()).Replace(localizedText, string.Empty).Trim();
						string localizedText2 = gameManager.LocManager.GetLocalizedText("DuelScene/Browsers/Foretell_Special_Action_Body", ("foretellCost", item));
						localizedText2 = ManaUtilities.ConvertManaSymbols(localizedText2);
						cardData.RulesTextOverride = new RawTextOverride(localizedText2);
					}
					else if (abilityPrintingData.SubCategory == AbilitySubCategory.ClassLevel)
					{
						CardPrintingData cardPrintingById2 = cardDatabase.CardDataProvider.GetCardPrintingById(action.GrpId);
						cardData.RulesTextOverride = new AbilityTextOverride(cardDatabase, cardData3.TitleId).AddSource(action.GrpId).AddAbility(abilityPrintingData).AddAbility(abilityPrintingData.GetClassGrantedAbilities(cardPrintingById2 ?? cardPrintingData));
					}
					else if (abilityPrintingData.Id == 172070)
					{
						cardData.Instance.Counters = cardData3.Instance.Counters;
					}
				}
				else
				{
					cardData = new CardData(new MtgCardInstance
					{
						ObjectType = GameObjectType.Ability,
						TitleId = originalCardData.TitleId
					}, new CardPrintingData(originalCardData.Printing, new CardPrintingRecord(0u, 0u, originalCardData.ImageAssetPath)));
					cardData.Instance.ParentId = originalCardData.InstanceId;
					cardData.Instance.Parent = originalCardData.Instance.GetCopy();
					cardData.Instance.SkinCode = originalCardData.SkinCode;
					cardData.Instance.SleeveCode = originalCardData.SleeveCode;
					if (latestGameState.TryGetCard(action.InstanceId, out var card))
					{
						cardData.RulesTextOverride = new GreLocTextOverride(cardDatabase.GreLocProvider, card.TitleId);
					}
					else
					{
						cardData.RulesTextOverride = new RawTextOverride(item2.Type.ToString());
					}
				}
				cardData.Instance.InstanceId = 0u;
				if (readOnlyList3 != null)
				{
					cardData.Instance.ManaCostOverride = readOnlyList3;
				}
				duelScene_CDC = cardBuilder.CreateCDC(cardData);
				duelScene_CDC.CurrentCardHolder = cardHolderProvider.GetCardHolder(GREPlayerNum.Invalid, CardHolderType.Invalid);
				splineMovementSystem.MoveInstant(duelScene_CDC.Root, new IdealPoint(cdc.Root)
				{
					Scale = Vector3.one * 0.33f
				});
				if (abilityPrintingData != null)
				{
					int value = originalCardData.Abilities.IndexOf(abilityPrintingData);
					dictionary4.Add(duelScene_CDC, value);
					if (abilityPrintingData.Category == AbilityCategory.AlternativeCost)
					{
						list4.Add(new AlternateCostEffectRemover(cardData, abilityPrintingData));
					}
					dictionary3[duelScene_CDC] = abilityPrintingData;
				}
			}
			if (item2.IsActive)
			{
				list.Add(duelScene_CDC);
				callbackMapping.Add(duelScene_CDC, item2);
				ClientSideChoiceMap[duelScene_CDC] = item2;
			}
			else
			{
				list2.Add(duelScene_CDC);
			}
			list3.Add(duelScene_CDC);
			dictionary[duelScene_CDC] = ModalHighlighting.HighlightForGreInteraction(item2, (cardData ?? originalCardData).Instance.GetCopy());
			dictionary2[duelScene_CDC] = action;
		}
		if (cdc.CurrentCardHolder.CardHolderType != CardHolderType.Battlefield && _overriddenCard == null)
		{
			cardMovementController.MoveCard(cdc, cardHolderProvider.GetCardHolderByZoneId(latestGameState.Stack.Id));
		}
		if (trimmedInteractions.Count == 1)
		{
			IClientLocProvider clientLocProvider = cardDatabase.ClientLocProvider;
			OptionalActionBrowserProvider_ClientSide.OptionalActionBrowserData obj2 = new OptionalActionBrowserProvider_ClientSide.OptionalActionBrowserData
			{
				CardViews = list
			};
			obj2.AbilityByCardView.Merge(dictionary3);
			obj2.GreActionByCardView.Merge(dictionary2);
			obj2.Header = clientLocProvider.GetLocalizedText("DuelScene/ClientPrompt/Are_You_Sure_Title");
			obj2.SubHeader = clientLocProvider.GetLocalizedText(cdc.Model.Instance.PlayWarnings.GetSubHeaderKey());
			obj2.NoText = "DuelScene/ClientPrompt/ClientPrompt_Button_No";
			obj2.GetBrowserCardHeaderData = modalBrowserCardHeaderProvider.GetBrowserCardInfo;
			obj2.OnNoAction = delegate
			{
				MtgZone zone = cdc.Model.Zone;
				ICardHolder cardHolder = ((zone != null) ? cardHolderProvider.GetCardHolderByZoneId(zone.Id) : cardHolderProvider.GetCardHolder(GREPlayerNum.Invalid, CardHolderType.Invalid));
				cdc.SetModel(originalCardData);
				ClientSideChoiceMap.Clear();
				if (cdc.CurrentCardHolder != cardHolder)
				{
					cardMovementController.MoveCard(cdc, cardHolder);
				}
			};
			obj2.YesText = "DuelScene/ClientPrompt/ClientPrompt_Button_Yes";
			obj2.OnYesAction = delegate
			{
				cdc.SetModel(originalCardData);
				ClientSideChoiceMap.Clear();
				submitInteraction(trimmedInteractions[0]);
			};
			obj2.NoInteractionPerformedCloseAction = delegate
			{
				cdc.SetModel(originalCardData);
				ClientSideChoiceMap.Clear();
			};
			OptionalActionBrowserProvider_ClientSide optionalActionBrowserProvider_ClientSide = new OptionalActionBrowserProvider_ClientSide(obj2);
			IBrowser openedBrowser = gameManager.BrowserManager.OpenBrowser(optionalActionBrowserProvider_ClientSide);
			optionalActionBrowserProvider_ClientSide.SetOpenedBrowser(openedBrowser);
			return;
		}
		Action<DuelScene_CDC> onSelectionMade = delegate(DuelScene_CDC selection)
		{
			bool flag = selection == null;
			bool flag2 = !flag && callbackMapping.ContainsKey(selection);
			if (flag || flag2)
			{
				CleanUpClientSideModal(cdc, selection, gameManager);
				bool showConfirm = false;
				bool flag3 = false;
				List<GreInteraction> list5 = new List<GreInteraction>();
				if (selection != null && callbackMapping.TryGetValue(selection, out var value2))
				{
					showConfirm = HasLegendaryWarning(selection, value2.GreAction);
					flag3 = true;
					list5.Add(value2);
				}
				gameManager.BrowserManager.CloseCurrentBrowser();
				if (flag3)
				{
					HandleActions(cdc, list5, submitInteraction, gameManager, showConfirm);
				}
				if (flag)
				{
					if (gameManager.CurrentInteraction is ActionsAvailableWorkflow_Browser actionsAvailableWorkflow_Browser)
					{
						actionsAvailableWorkflow_Browser.ApplyInteraction();
					}
					else if (gameManager.CurrentInteraction is ResolutionCastWorkflow resolutionCastWorkflow)
					{
						resolutionCastWorkflow.ApplyInteraction();
					}
				}
			}
		};
		string localizedText3 = cardDatabase.ClientLocProvider.GetLocalizedText("DuelScene/Browsers/Choose_Option_One");
		string localizedText4 = cardDatabase.ClientLocProvider.GetLocalizedText("DuelScene/Browsers/Choose_Option_Text");
		if (originalCardData.IsOmenCard() && trimmedInteractions.Count == 2 && trimmedInteractions.Exists((GreInteraction i) => i.Type == ActionType.CastOmen))
		{
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.SetCardDataExtensive(originalCardData);
			if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Header> loadedTree))
			{
				Header payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
				if (payload != null)
				{
					localizedText3 = cardDatabase.ClientLocProvider.GetLocalizedText(payload.LocKey);
				}
			}
			if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<SubHeader> loadedTree2))
			{
				SubHeader payload2 = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
				if (payload2 != null)
				{
					localizedText4 = cardDatabase.ClientLocProvider.GetLocalizedText(payload2.LocKey);
				}
			}
		}
		ModalBrowserProvider_ClientSide.ModalBrowserData modalBrowserData = new ModalBrowserProvider_ClientSide.ModalBrowserData
		{
			Header = localizedText3,
			SubHeader = localizedText4,
			CancelText = "DuelScene/ClientPrompt/ClientPrompt_Button_Cancel",
			Highlights = dictionary,
			OnSelectionMade = onSelectionMade,
			CanCancel = true,
			NoInteractionPerformedCloseAction = delegate
			{
				CleanUpClientSideModal(cdc, null, gameManager);
			},
			GetBrowserCardHeaderData = modalBrowserCardHeaderProvider.GetBrowserCardInfo
		};
		foreach (IClientSideOptionModelMutator item3 in list4)
		{
			if (item3 == null)
			{
				continue;
			}
			foreach (DuelScene_CDC item4 in list3)
			{
				ICardDataAdapter targetModel = item4.Model;
				if (item3.Mutate(ref targetModel))
				{
					item4.SetModel(targetModel);
				}
			}
		}
		modalBrowserData.Selectable.AddRange(list);
		modalBrowserData.NonSelectable.AddRange(list2);
		modalBrowserData.SortedCardList.AddRange(list3);
		modalBrowserData.AbilityByCardView.Merge(dictionary3);
		modalBrowserData.GreActionByCardView.Merge(dictionary2);
		ModalBrowserProvider_ClientSide modalBrowserProvider_ClientSide = new ModalBrowserProvider_ClientSide(modalBrowserData, gameManager.ViewManager);
		_modalSelectionBrowser = gameManager.BrowserManager.OpenBrowser(modalBrowserProvider_ClientSide);
		modalBrowserProvider_ClientSide.SetOpenedBrowser(_modalSelectionBrowser);
		_modalSelectionBrowser.ShownHandlers += OnModalBrowserShown;
		_modalSelectionBrowser.HiddenHandlers += OnModalBrowserHidden;
	}

	private static void OnModalBrowserHidden()
	{
		if (!(_overriddenCard == null) && _originalCardData != null)
		{
			_overriddenCard.SetModel(_originalCardData);
		}
	}

	private static void OnModalBrowserShown()
	{
		if (!(_overriddenCard == null) && _overrideCardData != null)
		{
			_overriddenCard.SetModel(_overrideCardData);
		}
	}

	private static void CleanUpClientSideModal(DuelScene_CDC cdc, DuelScene_CDC selection, GameManager gameManager)
	{
		IContext context = gameManager.Context;
		ICardHolderProvider cardHolderProvider = context.Get<ICardHolderProvider>() ?? NullCardHolderProvider.Default;
		ICardMovementController cardMovementController = context.Get<ICardMovementController>() ?? NullCardMovementController.Default;
		if (_overriddenCard != null && _originalCardData != null)
		{
			_overriddenCard.SetModel(_originalCardData);
		}
		ClientSideChoiceMap.Clear();
		if (cdc.CurrentCardHolder.CardHolderType == CardHolderType.Stack)
		{
			cardMovementController.MoveCard(cdc, cdc.PreviousCardHolder);
		}
		else if (selection != null && selection.Model != null && selection.PreviousCardHolder != null && selection.Model.LinkedFaceType != LinkedFace.PrototypeParent && selection.Model.LinkedFaceType != LinkedFace.MdfcFront && selection.Model.ObjectType == GameObjectType.Card && selection.PreviousCardHolder.CardHolderType != CardHolderType.Battlefield && !selection.Model.CardTypes.Contains(CardType.Land))
		{
			cardMovementController.MoveCard(cdc, gameManager.LatestGameState.Stack);
			cdc.PreviousCardHolder = cardHolderProvider.GetCardHolderByZoneId(cdc.Model.Zone.Id);
		}
		if (_modalSelectionBrowser != null)
		{
			_overriddenCard = null;
			_originalCardData = null;
			_overrideCardData = null;
			_modalSelectionBrowser.ShownHandlers -= OnModalBrowserShown;
			_modalSelectionBrowser.HiddenHandlers -= OnModalBrowserHidden;
			_modalSelectionBrowser = null;
		}
	}

	public static List<GreInteraction> GetTrimmedInteractionList(List<GreInteraction> interactionList, IAbilityDataProvider abilityDataProvider)
	{
		if (interactionList.Count == 1)
		{
			return interactionList;
		}
		if (interactionList.Exists((GreInteraction x) => x.Type == ActionType.Cast || x.Type == ActionType.CastLeft || x.Type == ActionType.CastRight))
		{
			return interactionList.FindAll((GreInteraction x) => x.GreAction.IsCastAction() || x.GreAction.IsPlayAction() || x.Type == ActionType.Activate || x.Type == ActionType.Special);
		}
		for (int num = interactionList.Count - 1; num > -1; num--)
		{
			Wotc.Mtgo.Gre.External.Messaging.Action greAction = interactionList[num].GreAction;
			if (greAction.ManaPaymentOptions.Count == 0 && greAction.AbilityGrpId == 121983)
			{
				interactionList.RemoveAt(num);
			}
		}
		for (int num2 = interactionList.Count - 1; num2 >= 0; num2--)
		{
			GreInteraction greInteraction = interactionList[num2];
			for (int num3 = num2 - 1; num3 >= 0; num3--)
			{
				GreInteraction greInteraction2 = interactionList[num3];
				if (greInteraction.GreAction.IsRedundantDuplicateActivatedAbility(greInteraction2.GreAction, abilityDataProvider) || greInteraction.GreAction.IsRedundantManaPayment(greInteraction2.GreAction))
				{
					if (greInteraction.IsActive != greInteraction2.IsActive && greInteraction2.IsActive)
					{
						interactionList.RemoveAt(num2);
						break;
					}
					interactionList.RemoveAt(num3);
					num2--;
				}
			}
		}
		return interactionList;
	}

	private static void ShowConfirmation(DuelScene_CDC cdc, GameManager gameManager, Wotc.Mtgo.Gre.External.Messaging.Action action, Action<GreInteraction> onActionTaken)
	{
		IClientLocProvider clientLocProvider = gameManager.CardDatabase.ClientLocProvider;
		AssetLookupSystem assetLookupSystem = gameManager.AssetLookupSystem;
		string text = null;
		string text2 = null;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cdc.Model);
		assetLookupSystem.Blackboard.GreActionType = action.ActionType;
		assetLookupSystem.Blackboard.GreAction = action;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Confirmation_Header> loadedTree))
		{
			Confirmation_Header payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				text = clientLocProvider.GetLocalizedText(payload.LocKey, payload.BuildParameters(assetLookupSystem.Blackboard));
				goto IL_00dc;
			}
		}
		text = clientLocProvider.GetLocalizedText("DuelScene/ClientPrompt/Are_You_Sure_Title");
		goto IL_00dc;
		IL_00dc:
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Confirmation_SubHeader> loadedTree2))
		{
			Confirmation_SubHeader payload2 = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				text2 = clientLocProvider.GetLocalizedText(payload2.LocKey, payload2.BuildParameters(assetLookupSystem.Blackboard));
				goto IL_01ba;
			}
		}
		ICardDataAdapter model = cdc.Model;
		text2 = ((model == null || !(model.Instance?.PlayWarnings?.Count > 0)) ? clientLocProvider.GetLocalizedText("DuelScene/ClientPrompt/Click_Yes_No_Text") : clientLocProvider.GetLocalizedText(cdc.Model.Instance.PlayWarnings.GetSubHeaderKey()));
		goto IL_01ba;
		IL_01ba:
		assetLookupSystem.Blackboard.Clear();
		OptionalActionBrowserProvider_ClientSide.OptionalActionBrowserData optionalActionBrowserData = new OptionalActionBrowserProvider_ClientSide.OptionalActionBrowserData();
		optionalActionBrowserData.CardViews = new List<DuelScene_CDC> { cdc };
		DuelScene_CDC key = cdc;
		optionalActionBrowserData.AbilityByCardView[key] = gameManager.CardDatabase.AbilityDataProvider.GetAbilityPrintingById(action.AbilityGrpId);
		DuelScene_CDC key2 = cdc;
		optionalActionBrowserData.GreActionByCardView[key2] = action;
		optionalActionBrowserData.Header = text;
		optionalActionBrowserData.SubHeader = text2;
		optionalActionBrowserData.YesText = "DuelScene/ClientPrompt/ClientPrompt_Button_Yes";
		optionalActionBrowserData.OnYesAction = delegate
		{
			onActionTaken(new GreInteraction(action));
		};
		optionalActionBrowserData.NoText = "DuelScene/ClientPrompt/ClientPrompt_Button_No";
		optionalActionBrowserData.OnNoAction = delegate
		{
			IContext context = gameManager.Context;
			ICardHolderProvider cardHolderProvider = context.Get<ICardHolderProvider>() ?? NullCardHolderProvider.Default;
			ICardMovementController cardMovementController = context.Get<ICardMovementController>() ?? NullCardMovementController.Default;
			if (cardHolderProvider.TryGetCardHolder<ICardHolder>(cdc.Model.Zone.Id, out var result) && cdc.CurrentCardHolder != result)
			{
				cardMovementController.MoveCard(cdc, result);
			}
		};
		OptionalActionBrowserProvider_ClientSide optionalActionBrowserProvider_ClientSide = new OptionalActionBrowserProvider_ClientSide(optionalActionBrowserData);
		IBrowser openedBrowser = gameManager.BrowserManager.OpenBrowser(optionalActionBrowserProvider_ClientSide);
		optionalActionBrowserProvider_ClientSide.SetOpenedBrowser(openedBrowser);
	}

	public static bool HasPlayWarnings(IEntityView entity, params Wotc.Mtgo.Gre.External.Messaging.Action[] actions)
	{
		if (actions.Length == 1 && entity is DuelScene_CDC cdc)
		{
			return HasPlayWarnings(cdc, actions[0]);
		}
		return false;
	}

	private static bool HasPlayWarnings(DuelScene_CDC cdc, Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		if (cdc.Model.Instance.PlayWarnings.SelectMany((ShouldntPlayData warning) => warning.Reasons).Exists((ShouldntPlayData.ReasonType reason) => reason != ShouldntPlayData.ReasonType.EntersTapped && reason != ShouldntPlayData.ReasonType.StartingPlayer && reason != ShouldntPlayData.ReasonType.RedundantActivation))
		{
			if (action.ActionType != ActionType.Play && action.ActionType != ActionType.Cast)
			{
				return action.ActionType == ActionType.Activate;
			}
			return true;
		}
		return false;
	}

	private static bool HasLegendaryWarning(DuelScene_CDC cdc, Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		if (cdc.Model == null)
		{
			return false;
		}
		if (cdc.Model.Instance == null)
		{
			return false;
		}
		if (!cdc.Model.Instance.PlayWarnings.Exists((ShouldntPlayData warning) => warning.Reasons.Contains(ShouldntPlayData.ReasonType.Legendary)))
		{
			return false;
		}
		if (action.ActionType != ActionType.Play && action.ActionType != ActionType.Cast && action.ActionType != ActionType.Activate)
		{
			return false;
		}
		return true;
	}
}
