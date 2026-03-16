using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Browser;
using AssetLookupTree.Payloads.DuelScene.Interactions;
using Google.Protobuf.Collections;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class KeywordData
{
	public readonly IReadOnlyDictionary<string, uint> IdsByKeywords;

	public readonly IReadOnlyList<string> SortedKeywords;

	public readonly IReadOnlyList<string> HintingOptions;

	private static List<CardType> _allCastableTypes = new List<CardType>
	{
		CardType.Artifact,
		CardType.Battle,
		CardType.Creature,
		CardType.Enchantment,
		CardType.Instant,
		CardType.Kindred,
		CardType.Planeswalker,
		CardType.Sorcery
	};

	public KeywordData(IReadOnlyDictionary<string, uint> idsByKeywords, IReadOnlyList<string> sortedKeywords, IReadOnlyList<string> hintingOptions)
	{
		IdsByKeywords = idsByKeywords ?? DictionaryExtensions.Empty<string, uint>();
		SortedKeywords = sortedKeywords ?? Array.Empty<string>();
		HintingOptions = hintingOptions ?? Array.Empty<string>();
	}

	public static KeywordData Generate(SelectNRequest request, MtgGameState latestGameState, IPromptEngine promptEngine, GameManager gameManager)
	{
		if (request.ListType == SelectionListType.Static || request.ListType == SelectionListType.StaticSubset)
		{
			return InitializeStatic(request, latestGameState, gameManager);
		}
		if (request.ListType == SelectionListType.Dynamic && request.IdType == IdType.PromptParameterIndex)
		{
			return InitializeDynamic(request, latestGameState, promptEngine, gameManager);
		}
		throw new ArgumentException($"Provided SelectNReq could not be used as a SelectNKeyword request. StaticList type {request.StaticList} is not supported.");
	}

	private static void AddCardSubTypesToHintingList(IEnumerable<IEnumerable<SubType>> cardSubTypeLists, IGreLocProvider greLocMan, IReadOnlyDictionary<string, uint> knownIds, List<string> hintingOptions)
	{
		foreach (IEnumerable<SubType> cardSubTypeList in cardSubTypeLists)
		{
			foreach (SubType item in cardSubTypeList)
			{
				string localizedTextForEnumValue = greLocMan.GetLocalizedTextForEnumValue("SubType", (int)item);
				if (knownIds.ContainsKey(localizedTextForEnumValue) && !hintingOptions.Contains(localizedTextForEnumValue))
				{
					hintingOptions.Add(localizedTextForEnumValue);
				}
			}
		}
	}

	private static void AddCardSubTypesToHintingList(IEnumerable<SubType> subTypeSet, IGreLocProvider greLocMan, IReadOnlyDictionary<string, uint> knownIds, List<string> hintingOptions)
	{
		foreach (SubType item in subTypeSet)
		{
			string localizedTextForEnumValue = greLocMan.GetLocalizedTextForEnumValue("SubType", (int)item);
			if (knownIds.ContainsKey(localizedTextForEnumValue) && !hintingOptions.Contains(localizedTextForEnumValue))
			{
				hintingOptions.Add(localizedTextForEnumValue);
			}
		}
	}

	private static KeywordData InitializeStatic(SelectNRequest request, MtgGameState latestGameState, GameManager gameManager)
	{
		ICardDatabaseAdapter cardDatabase;
		switch (request.StaticList)
		{
		case StaticList.SubTypes:
		case StaticList.BasicLandTypes:
		case StaticList.CreatureTypes:
			return StaticSubtypeKeywordData(request.Ids, request.SourceId, latestGameState, gameManager);
		case StaticList.CardNames:
			return StaticCardNamesKeywordData(request.Ids, latestGameState, request.Prompt.Parameters, gameManager);
		default:
		{
			cardDatabase = gameManager.CardDatabase;
			IGreLocProvider greLocProvider = gameManager.CardDatabase.GreLocProvider;
			Dictionary<string, uint> dictionary = new Dictionary<string, uint>();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			switch (request.StaticList)
			{
			case StaticList.Keywords:
				foreach (uint id in request.Ids)
				{
					string abilityTextByCardAbilityGrpId = cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(0u, id, Array.Empty<uint>());
					dictionary[abilityTextByCardAbilityGrpId] = id;
				}
				break;
			case StaticList.CardTypes:
			{
				foreach (int item in CardTypeTranslator.ConvertIdsToCardTypes(request.Ids))
				{
					string localizedTextForEnumValue3 = greLocProvider.GetLocalizedTextForEnumValue("CardType", item);
					dictionary[localizedTextForEnumValue3] = (uint)item;
				}
				PromptParameter promptParameter = request.Prompt.Parameters.FirstOrDefault((PromptParameter x) => x.ParameterName == "CardId");
				if (ShouldUseDeckBasedHinting(((promptParameter != null) ? latestGameState.GetCardById((uint)promptParameter.NumberValue) : null)?.TitleId ?? 0))
				{
					foreach (uint deckGrpId in GetDeckGrpIds(cardDatabase, gameManager))
					{
						CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(deckGrpId);
						if (cardPrintingById == null || CardUtilities.IsOmenOrAdventureParentFace(cardPrintingById.LinkedFaceType) || CardUtilities.IsFrontFaceOfDoubleSided(cardPrintingById.LinkedFaceType) || cardPrintingById.LinkedFaceType == LinkedFace.SplitHalf)
						{
							continue;
						}
						foreach (CardType type in cardPrintingById.Types)
						{
							string localizedTextForEnumValue4 = greLocProvider.GetLocalizedTextForEnumValue("CardType", (int)type);
							if (dictionary.ContainsKey(localizedTextForEnumValue4) && !list2.Contains(localizedTextForEnumValue4))
							{
								list2.Add(localizedTextForEnumValue4);
							}
						}
					}
					break;
				}
				if (HintAllCastableTypes())
				{
					foreach (CardType allCastableType in _allCastableTypes)
					{
						string localizedTextForEnumValue5 = greLocProvider.GetLocalizedTextForEnumValue("CardType", (int)allCastableType);
						if (dictionary.ContainsKey(localizedTextForEnumValue5))
						{
							list2.Add(localizedTextForEnumValue5);
						}
					}
					break;
				}
				List<MtgCardInstance> list4 = new List<MtgCardInstance>(latestGameState.VisibleCards.Values);
				list4.AddRange(latestGameState.RevealedCards.Values);
				foreach (MtgCardInstance item2 in list4)
				{
					foreach (CardType cardType in item2.CardTypes)
					{
						string localizedTextForEnumValue6 = greLocProvider.GetLocalizedTextForEnumValue("CardType", (int)cardType);
						if (dictionary.ContainsKey(localizedTextForEnumValue6) && !list2.Contains(localizedTextForEnumValue6))
						{
							list2.Add(localizedTextForEnumValue6);
						}
					}
				}
				break;
			}
			case StaticList.SuperTypes:
			{
				foreach (uint id2 in request.Ids)
				{
					string localizedTextForEnumValue7 = greLocProvider.GetLocalizedTextForEnumValue("SuperType", (int)id2);
					dictionary[localizedTextForEnumValue7] = id2;
				}
				List<MtgCardInstance> list5 = new List<MtgCardInstance>(latestGameState.VisibleCards.Values);
				list5.AddRange(latestGameState.RevealedCards.Values);
				foreach (MtgCardInstance item3 in list5)
				{
					foreach (SuperType supertype in item3.Supertypes)
					{
						string localizedTextForEnumValue8 = greLocProvider.GetLocalizedTextForEnumValue("SuperType", (int)supertype);
						if (dictionary.ContainsKey(localizedTextForEnumValue8) && !list2.Contains(localizedTextForEnumValue8))
						{
							list2.Add(localizedTextForEnumValue8);
						}
					}
				}
				list2.Sort();
				break;
			}
			case StaticList.CounterTypes:
			{
				foreach (uint id3 in request.Ids)
				{
					string localizedTextForEnumValue = greLocProvider.GetLocalizedTextForEnumValue("CounterType", (int)id3);
					dictionary[localizedTextForEnumValue] = id3;
				}
				List<MtgCardInstance> list3 = new List<MtgCardInstance>(latestGameState.VisibleCards.Values);
				list3.AddRange(latestGameState.RevealedCards.Values);
				foreach (MtgCardInstance item4 in list3)
				{
					foreach (CounterType key in item4.Counters.Keys)
					{
						string localizedTextForEnumValue2 = greLocProvider.GetLocalizedTextForEnumValue("CounterType", (int)key);
						if (dictionary.ContainsKey(localizedTextForEnumValue2) && !list2.Contains(localizedTextForEnumValue2))
						{
							list2.Add(localizedTextForEnumValue2);
						}
					}
				}
				list2.Sort();
				break;
			}
			default:
				throw new ArgumentException($"Provided SelectNReq could not be used as a SelectNKeyword request. StaticList type {request.StaticList} is not supported.");
			}
			list.AddRange(dictionary.Keys);
			list.Sort();
			return new KeywordData(dictionary, list, list2);
		}
		}
		bool HintAllCastableTypes()
		{
			return cardDatabase.GetPrintingFromInstance(latestGameState.GetTopCardOnStack()).Tags.Contains(MetaDataTag.HintAllCastableTypes);
		}
		static bool ShouldUseDeckBasedHinting(uint cardTitleId)
		{
			if (cardTitleId == 428606 || cardTitleId == 477101)
			{
				return true;
			}
			return false;
		}
	}

	private static IEnumerable<uint> GetDeckGrpIds(ICardDatabaseAdapter cardDatabase, GameManager gameManager)
	{
		HashSet<uint> grpIds = gameManager.GenericPool.PopObject<HashSet<uint>>();
		MatchManager matchManager = gameManager.MatchManager;
		foreach (uint deckGrpId in GetDeckGrpIds(matchManager.LocalPlayerInfo.DeckCards, cardDatabase.CardDataProvider))
		{
			if (grpIds.Add(deckGrpId))
			{
				yield return deckGrpId;
			}
		}
		grpIds.Clear();
		gameManager.GenericPool.PushObject(grpIds);
	}

	private static IEnumerable<uint> GetDeckGrpIds(IEnumerable<uint> deckGrpIds, ICardDataProvider cardDatabase)
	{
		foreach (uint grpId in deckGrpIds)
		{
			yield return grpId;
			CardPrintingData cardPrintingById = cardDatabase.GetCardPrintingById(grpId);
			if (cardPrintingById == null)
			{
				continue;
			}
			foreach (uint linkedFaceGrpId in cardPrintingById.LinkedFaceGrpIds)
			{
				yield return linkedFaceGrpId;
			}
		}
	}

	private static KeywordData InitializeDynamic(SelectNRequest request, MtgGameState latestGameState, IPromptEngine promptEngine, GameManager gameManager)
	{
		Dictionary<string, uint> dictionary = new Dictionary<string, uint>();
		List<string> list = new List<string>();
		List<string> hintingOptions = new List<string>();
		ICardDatabaseAdapter cardDatabase = gameManager.CardDatabase;
		RepeatedField<PromptParameter> parameters = request.ReqPrompt.Parameters;
		for (int i = 0; i < request.Ids.Count; i++)
		{
			string text = string.Empty;
			PromptParameter promptParameter = parameters[i];
			switch (promptParameter.Type)
			{
			case ParameterType.PromptId:
			{
				IBlackboard blackboard = gameManager.AssetLookupSystem.Blackboard;
				blackboard.Clear();
				blackboard.Prompt = request.ReqPrompt;
				blackboard.Prompt.PromptId = (uint)promptParameter.PromptId;
				if (gameManager.AssetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PromptPayload> loadedTree))
				{
					PromptPayload payload = loadedTree.GetPayload(blackboard);
					if (payload != null)
					{
						(string, string)[] array = new(string, string)[payload.ParameterProviders.Count];
						int num = 0;
						foreach (IPromptParameterProvider parameterProvider in payload.ParameterProviders)
						{
							if (parameterProvider.TryGetValue(request, gameManager, out var paramValue))
							{
								array[num] = (parameterProvider.GetKey(), paramValue);
							}
							num++;
						}
						text = Languages.ActiveLocProvider.GetLocalizedText(payload.Key, array);
						break;
					}
				}
				text = promptEngine.GetPromptText(promptParameter.PromptId);
				if (string.IsNullOrEmpty(text))
				{
					text = $"Missing Prompt {promptParameter.PromptId}";
				}
				break;
			}
			case ParameterType.Number:
				text = promptEngine.GetPromptText(promptParameter.NumberValue);
				break;
			case ParameterType.NonLocalizedString:
				text = promptParameter.StringValue;
				break;
			case ParameterType.Reference:
			{
				Reference reference = promptParameter.Reference;
				switch (reference.Type)
				{
				case ReferenceType.CatalogId:
					text = cardDatabase.CardTitleProvider.GetCardTitle(reference.Id);
					break;
				case ReferenceType.InstanceId:
				{
					MtgCardInstance cardById = latestGameState.GetCardById(reference.Id);
					text = ((cardById == null) ? $"Cannot find card w/ ID of {reference.Id} for SelectNRequest" : gameManager.CardDatabase.GreLocProvider.GetLocalizedText(cardById.TitleId));
					break;
				}
				case ReferenceType.LocalizationId:
					text = promptEngine.GetPromptText((int)reference.Id);
					break;
				case ReferenceType.PlayerSeatId:
					text = ((latestGameState.LocalPlayer.InstanceId == reference.Id) ? Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/Local_Player") : Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Browsers/Opponent"));
					break;
				}
				break;
			}
			default:
				throw new NotImplementedException($"Prompt ParameterType of {promptParameter.Type} not implemented");
			}
			dictionary[text] = request.Ids[i];
		}
		if (list.Count == 0)
		{
			list.AddRange(dictionary.Keys);
			list.Sort();
		}
		return new KeywordData(dictionary, list, hintingOptions);
	}

	private static KeywordData StaticSubtypeKeywordData(IEnumerable<uint> ids, uint sourceId, MtgGameState gameState, GameManager gameManager)
	{
		ICardDatabaseAdapter cardDatabase = gameManager.CardDatabase;
		IGreLocProvider greLocProvider = gameManager.CardDatabase.GreLocProvider;
		MatchManager matchManager = gameManager.MatchManager;
		Dictionary<string, uint> dictionary = new Dictionary<string, uint>();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		string enumName = "SubType";
		uint num = 306u;
		foreach (uint id in ids)
		{
			if (id != num)
			{
				string localizedTextForEnumValue = greLocProvider.GetLocalizedTextForEnumValue(enumName, (int)id);
				dictionary[localizedTextForEnumValue] = id;
			}
		}
		switch (gameState.ResolvingCardInstance?.TitleId ?? gameState.GetCardById(sourceId)?.TitleId ?? 0)
		{
		case 413961u:
			AddCardSubTypesToHintingList(gameState.OpponentBattlefieldCards.Select((MtgCardInstance x) => x.Subtypes), greLocProvider, dictionary, list2);
			break;
		case 477309u:
		case 1078817u:
			AddCardSubTypesToHintingList(gameState.LocalGraveyard.VisibleCards.Select((MtgCardInstance x) => x.Subtypes), greLocProvider, dictionary, list2);
			break;
		case 15863u:
		case 477251u:
		case 811969u:
		case 1078692u:
			AddCardSubTypesToHintingList(gameState.LocalPlayerBattlefieldCards.Select((MtgCardInstance x) => x.Subtypes), greLocProvider, dictionary, list2);
			break;
		case 1079150u:
			AddCardSubTypesToHintingList(from x in gameState.LocalPlayerBattlefieldCards
				where x.InstanceId != sourceId
				select x.Subtypes, greLocProvider, dictionary, list2);
			break;
		case 1078717u:
		{
			HashSet<SubType> hashSet5 = gameManager.GenericPool.PopObject<HashSet<SubType>>();
			hashSet5.UnionWith(gameState.LocalPlayerBattlefieldCards.SelectMany((MtgCardInstance x) => x.Subtypes));
			foreach (uint deckCard in matchManager.LocalPlayerInfo.DeckCards)
			{
				hashSet5.UnionWith(cardDatabase.CardDataProvider.GetCardPrintingById(deckCard).Subtypes);
			}
			foreach (MtgCardInstance visibleCard in gameState.Command.VisibleCards)
			{
				if (visibleCard.Owner.IsLocalPlayer)
				{
					hashSet5.UnionWith(visibleCard.Subtypes);
				}
			}
			AddCardSubTypesToHintingList(hashSet5, greLocProvider, dictionary, list2);
			hashSet5.Clear();
			gameManager.GenericPool.PushObject(hashSet5);
			break;
		}
		case 1079380u:
		case 1079412u:
		{
			HashSet<SubType> hashSet4 = gameManager.GenericPool.PopObject<HashSet<SubType>>();
			foreach (uint id2 in ids)
			{
				hashSet4.Add((SubType)id2);
			}
			AddCardSubTypesToHintingList(hashSet4, greLocProvider, dictionary, list2);
			hashSet4.Clear();
			gameManager.GenericPool.PushObject(hashSet4);
			break;
		}
		case 1079079u:
		{
			HashSet<SubType> hashSet3 = gameManager.GenericPool.PopObject<HashSet<SubType>>();
			foreach (uint deckCard2 in matchManager.LocalPlayerInfo.DeckCards)
			{
				hashSet3.UnionWith(cardDatabase.CardDataProvider.GetCardPrintingById(deckCard2).Subtypes);
			}
			AddCardSubTypesToHintingList(hashSet3, greLocProvider, dictionary, list2);
			hashSet3.Clear();
			gameManager.GenericPool.PushObject(hashSet3);
			break;
		}
		default:
			if (gameState.ReplacementEffects.ContainsKey(sourceId) && gameState.ReplacementEffects[sourceId].Exists((ReplacementEffectData x) => x.AbilityId == 176647))
			{
				HashSet<SubType> hashSet = gameManager.GenericPool.PopObject<HashSet<SubType>>();
				foreach (uint deckGrpId in GetDeckGrpIds(cardDatabase, gameManager))
				{
					if (cardDatabase.CardDataProvider.TryGetCardPrintingById(deckGrpId, out var card))
					{
						hashSet.UnionWith(card.Subtypes);
					}
				}
				foreach (MtgCardInstance visibleCard2 in gameState.LocalHand.VisibleCards)
				{
					hashSet.UnionWith(visibleCard2.Subtypes);
					if (!cardDatabase.CardDataProvider.TryGetCardPrintingById(visibleCard2.GrpId, out var card2) || card2.LinkedFacePrintings == null)
					{
						continue;
					}
					foreach (CardPrintingData linkedFacePrinting in card2.LinkedFacePrintings)
					{
						hashSet.UnionWith(linkedFacePrinting.Subtypes);
					}
				}
				foreach (MtgCardInstance visibleCard3 in gameState.Command.VisibleCards)
				{
					hashSet.UnionWith(visibleCard3.Subtypes);
					if (!cardDatabase.CardDataProvider.TryGetCardPrintingById(visibleCard3.GrpId, out var card3) || card3.LinkedFacePrintings == null)
					{
						continue;
					}
					foreach (CardPrintingData linkedFacePrinting2 in card3.LinkedFacePrintings)
					{
						hashSet.UnionWith(linkedFacePrinting2.Subtypes);
					}
				}
				AddCardSubTypesToHintingList(hashSet, greLocProvider, dictionary, list2);
				hashSet.Clear();
				gameManager.GenericPool.PushObject(hashSet);
			}
			else if (gameState.ReplacementEffects.ContainsKey(sourceId) && gameState.ReplacementEffects[sourceId].Exists((ReplacementEffectData x) => x.AbilityId == 167203))
			{
				HashSet<SubType> hashSet2 = gameManager.GenericPool.PopObject<HashSet<SubType>>();
				foreach (uint deckGrpId2 in GetDeckGrpIds(cardDatabase, gameManager))
				{
					CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(deckGrpId2);
					if (cardPrintingById == null)
					{
						continue;
					}
					hashSet2.UnionWith(cardPrintingById.Subtypes);
					if (cardPrintingById.LinkedFacePrintings == null)
					{
						continue;
					}
					foreach (CardPrintingData linkedFacePrinting3 in cardPrintingById.LinkedFacePrintings)
					{
						hashSet2.UnionWith(linkedFacePrinting3.Subtypes);
					}
				}
				foreach (MtgCardInstance visibleCard4 in gameState.LocalHand.VisibleCards)
				{
					hashSet2.UnionWith(visibleCard4.Subtypes);
				}
				foreach (MtgCardInstance localPlayerBattlefieldCard in gameState.LocalPlayerBattlefieldCards)
				{
					hashSet2.UnionWith(localPlayerBattlefieldCard.Subtypes);
				}
				AddCardSubTypesToHintingList(hashSet2, greLocProvider, dictionary, list2);
				hashSet2.Clear();
				gameManager.GenericPool.PushObject(hashSet2);
			}
			else
			{
				List<MtgCardInstance> list3 = new List<MtgCardInstance>(50);
				list3.AddRange(gameState.VisibleCards.Values);
				list3.AddRange(gameState.RevealedCards.Values);
				AddCardSubTypesToHintingList(list3.Select((MtgCardInstance x) => x.Subtypes), greLocProvider, dictionary, list2);
			}
			break;
		}
		list2.Sort();
		list.AddRange(dictionary.Keys);
		list.Sort();
		return new KeywordData(dictionary, list, list2);
	}

	private static KeywordData StaticCardNamesKeywordData(IEnumerable<uint> ids, MtgGameState gameState, IEnumerable<PromptParameter> promptParams, GameManager gameManager)
	{
		ICardDatabaseAdapter cardDatabase = gameManager.CardDatabase;
		Dictionary<string, uint> dictionary = new Dictionary<string, uint>();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (CardPrintingData value in cardDatabase.DatabaseUtilities.GetAllPrintings().Values)
		{
			if ((ids.Count() == 0 || ids.Contains(value.TitleId)) && !value.IsToken && !value.IsRebalanced && !cardDatabase.GreLocProvider.GetLocalizedText(value.TitleId).Contains("//") && !value.ExpansionCode.Equals("ArenaSUP"))
			{
				string localizedText = cardDatabase.GreLocProvider.GetLocalizedText(value.TitleId);
				if (localizedText != null && !dictionary.ContainsKey(localizedText))
				{
					dictionary[localizedText] = value.TitleId;
				}
			}
		}
		bool includeBackOfDoubleFaceCards = true;
		bool includeSplitChildren = true;
		bool includeAdventureChildren = true;
		bool includeLands = true;
		bool includeControlledCreatures = false;
		bool includeNonVisibleSpecializeChildren = true;
		bool includeNonOpponentCards = true;
		bool flag = false;
		PromptParameter promptParameter = promptParams.FirstOrDefault((PromptParameter x) => x.ParameterName == "CardId");
		if (promptParameter != null)
		{
			MtgCardInstance cardById = gameState.GetCardById((uint)promptParameter.NumberValue);
			if (cardById != null)
			{
				gameManager.AssetLookupSystem.Blackboard.Clear();
				gameManager.AssetLookupSystem.Blackboard.SetCardDataExtensive(new CardData(cardById, cardById.ObjectSourcePrinting));
				if (gameManager.AssetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<NameACardPrepopulationLogic> loadedTree))
				{
					NameACardPrepopulationLogic payload = loadedTree.GetPayload(gameManager.AssetLookupSystem.Blackboard);
					if (payload != null)
					{
						includeBackOfDoubleFaceCards = payload.IncludeBackOfDoubleFaceCards;
						includeSplitChildren = payload.IncludeSplitChildren;
						includeAdventureChildren = payload.IncludeAdventureChildren;
						includeLands = payload.IncludeLands;
						includeNonVisibleSpecializeChildren = payload.IncludeNonVisibleSpecializeChildren;
						includeNonOpponentCards = payload.IncludeNonOpponentCards;
						includeControlledCreatures = payload.IncludeControlledCreatures;
						flag = payload.FallbackOnDeckGrpIds;
					}
				}
			}
		}
		IEnumerable<uint> enumerable = gameState.SeenGrpIds;
		if (gameState.ResolvingCardInstance != null)
		{
			gameManager.AssetLookupSystem.Blackboard.Clear();
			gameManager.AssetLookupSystem.Blackboard.SetCardDataExtensive(new CardData(gameState.ResolvingCardInstance, gameState.ResolvingCardInstance.ObjectSourcePrinting));
			if (gameManager.AssetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<NameACardPrepopulationLogic> loadedTree2))
			{
				NameACardPrepopulationLogic payload2 = loadedTree2.GetPayload(gameManager.AssetLookupSystem.Blackboard);
				if (payload2 != null)
				{
					if (payload2.SetSourceGrpIdsToDeckGrpIds)
					{
						enumerable = GetDeckGrpIds(cardDatabase, gameManager);
					}
					includeBackOfDoubleFaceCards = payload2.IncludeBackOfDoubleFaceCards;
					includeSplitChildren = payload2.IncludeSplitChildren;
					includeAdventureChildren = payload2.IncludeAdventureChildren;
					includeLands = payload2.IncludeLands;
					includeControlledCreatures = payload2.IncludeControlledCreatures;
					includeNonVisibleSpecializeChildren = payload2.IncludeNonVisibleSpecializeChildren;
					includeNonOpponentCards = payload2.IncludeNonOpponentCards;
					flag = payload2.FallbackOnDeckGrpIds;
				}
			}
		}
		foreach (uint item in enumerable)
		{
			CardPrintingData cardPrinting = cardDatabase.CardDataProvider.GetCardPrintingById(item);
			if (ExcludeCard(cardPrinting))
			{
				continue;
			}
			if (includeControlledCreatures && (gameState.Battlefield.VisibleCards.Exists((MtgCardInstance ci) => ci.GrpId == cardPrinting.GrpId && ci.Zone.Type == ZoneType.Battlefield && ci.Owner.IsLocalPlayer) || (IsDualFaceType(cardPrinting.LinkedFaceType) && gameState.Battlefield.VisibleCards.Exists((MtgCardInstance ci) => cardPrinting.LinkedFaceGrpIds.Contains(ci.GrpId) && ci.Zone.Type == ZoneType.Battlefield && ci.Owner.IsLocalPlayer))))
			{
				string localizedText2 = cardDatabase.GreLocProvider.GetLocalizedText(cardPrinting.TitleId);
				if (dictionary.ContainsKey(localizedText2) && !list2.Contains(localizedText2))
				{
					list2.Add(localizedText2);
				}
			}
			if ((!ids.Any() && !flag) || ids.Contains(cardPrinting.TitleId))
			{
				uint titleId = cardPrinting.TitleId;
				if (cardPrinting.IsRebalanced)
				{
					titleId = cardDatabase.CardDataProvider.GetCardPrintingById(cardPrinting.RebalancedCardLink).TitleId;
				}
				string localizedText3 = cardDatabase.GreLocProvider.GetLocalizedText(titleId);
				if (dictionary.ContainsKey(localizedText3) && !list2.Contains(localizedText3))
				{
					list2.Add(localizedText3);
				}
			}
		}
		if (list2.Count() == 0 && flag)
		{
			enumerable = GetDeckGrpIds(cardDatabase, gameManager);
			foreach (uint item2 in enumerable)
			{
				CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(item2);
				if (!ExcludeCard(cardPrintingById))
				{
					uint titleId2 = cardPrintingById.TitleId;
					if (cardPrintingById.IsRebalanced)
					{
						titleId2 = cardDatabase.CardDataProvider.GetCardPrintingById(cardPrintingById.RebalancedCardLink).TitleId;
					}
					string localizedText4 = cardDatabase.GreLocProvider.GetLocalizedText(titleId2);
					if (!list2.Contains(localizedText4))
					{
						list2.Add(localizedText4);
					}
				}
			}
		}
		list2.Sort();
		list.AddRange(dictionary.Keys);
		list.Sort();
		return new KeywordData(dictionary, list, list2);
		bool ExcludeCard(CardPrintingData cardPrintingData)
		{
			if (cardPrintingData == null)
			{
				return true;
			}
			if (cardPrintingData.LinkedFaceType == LinkedFace.SplitHalf)
			{
				return true;
			}
			if (!includeBackOfDoubleFaceCards && cardPrintingData.LinkedFaceType == LinkedFace.DfcFront)
			{
				return true;
			}
			if (!includeSplitChildren && cardPrintingData.LinkedFaceType == LinkedFace.SplitCard)
			{
				return true;
			}
			if (!includeAdventureChildren && cardPrintingData.LinkedFaceType == LinkedFace.AdventureParent)
			{
				return true;
			}
			if (!includeLands && cardPrintingData.Types.Contains(CardType.Land))
			{
				return true;
			}
			if (includeControlledCreatures && !cardPrintingData.Types.Contains(CardType.Creature))
			{
				return true;
			}
			MtgCardInstance card;
			IEnumerable<uint> enumerable2 = from x in gameState.VisibleCards
				where gameState.TryGetCard(x.Key, out card) && (!card.Owner.IsLocalPlayer || !card.Controller.IsLocalPlayer)
				select x.Value.GrpId;
			IEnumerable<uint> enumerable3 = from x in gameState.RevealedCards
				where gameState.TryGetCard(x.Key, out card) && (!card.Owner.IsLocalPlayer || !card.Controller.IsLocalPlayer)
				select x.Value.GrpId;
			if (!includeNonOpponentCards && !enumerable2.Contains(cardPrintingData.GrpId) && !enumerable3.Contains(cardPrintingData.GrpId) && enumerable2.Intersect(cardPrintingData.LinkedFaceGrpIds).Count() == 0 && enumerable3.Intersect(cardPrintingData.LinkedFaceGrpIds).Count() == 0)
			{
				return true;
			}
			if (!includeNonVisibleSpecializeChildren && (cardPrintingData.LinkedFaceType == LinkedFace.SpecializeChild || cardPrintingData.LinkedFaceType == LinkedFace.SpecializeParent) && !gameState.VisibleCards.Select((KeyValuePair<uint, MtgCardInstance> c) => c.Value.GrpId).Contains(cardPrintingData.GrpId) && !gameState.RevealedCards.Select((KeyValuePair<uint, MtgCardInstance> c) => c.Value.GrpId).Contains(cardPrintingData.GrpId))
			{
				return true;
			}
			return false;
		}
		static bool IsDualFaceType(LinkedFace linkedFace)
		{
			if (linkedFace != LinkedFace.DfcFront && linkedFace != LinkedFace.DfcBack && linkedFace != LinkedFace.MdfcBack)
			{
				return linkedFace == LinkedFace.MdfcFront;
			}
			return true;
		}
	}
}
