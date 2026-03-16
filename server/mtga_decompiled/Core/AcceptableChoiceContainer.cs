using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

[Serializable]
public struct AcceptableChoiceContainer
{
	[Serializable]
	public class InnerListOfCriteria
	{
		public List<AcceptableChoice> criteria;
	}

	[Serializable]
	public class AcceptableChoice
	{
		[Tooltip("If action affects more than one target, use this to differentiate. Make sure you have the order correct. If multiple acceptable targets have the same position, priority will be in list order.")]
		public int targetIndex = 1;

		public TargetType targetType;

		public TurnInformation.ActivePlayer targetController;

		public TargetFilter targetFilter;

		public TargetFilterComparer filterComparer;

		public string filterParameter;

		private List<uint> _referenceIdCache = new List<uint>();

		private static Dictionary<int, string> previousFilterParameters = new Dictionary<int, string>();

		private static BotTool _botTool;

		private static BotTool BotTool
		{
			get
			{
				if (_botTool == null)
				{
					_botTool = Pantry.Get<BotTool>();
				}
				return _botTool;
			}
		}

		private DeckHeuristic DeckHeuristic => BotTool.DeckHeuristic;

		public CardType TargetTypeToCardType(TargetType tType)
		{
			return tType switch
			{
				TargetType.Creature => CardType.Creature, 
				TargetType.Enchantment => CardType.Enchantment, 
				TargetType.Artifact => CardType.Artifact, 
				TargetType.Land => CardType.Land, 
				_ => CardType.None, 
			};
		}

		private List<MtgCardInstance> FilterListByType(IReadOnlyList<MtgCardInstance> cards, CardType type)
		{
			List<MtgCardInstance> list = new List<MtgCardInstance>();
			foreach (MtgCardInstance card in cards)
			{
				if (card.CardTypes.Contains(type))
				{
					list.Add(card);
				}
			}
			return list;
		}

		public bool AcceptableTargetExists(string heuristicCardTitle, MtgGameState gameState, Archetype archetype, ICardDatabaseAdapter cardDatabase, bool canTargetIndestructible = false)
		{
			if (targetController != TurnInformation.ActivePlayer.AI)
			{
				switch (archetype)
				{
				case Archetype.BurnToFace:
				{
					List<MtgPlayer> otherTargetablePlayers = new List<MtgPlayer>();
					if (IsAcceptablePlayerTarget(gameState.Opponent, otherTargetablePlayers))
					{
						return true;
					}
					break;
				}
				case Archetype.Recursion:
					foreach (MtgCardInstance visibleCard in gameState.OpponentGraveyard.VisibleCards)
					{
						_ = visibleCard;
						if (IsAcceptableRecursion(gameState, gameState.OpponentGraveyard.VisibleCards, cardDatabase))
						{
							return true;
						}
					}
					break;
				case Archetype.Removal:
					foreach (MtgCardInstance opponentBattlefieldCard in gameState.OpponentBattlefieldCards)
					{
						if (IsAcceptableRemovalTarget(gameState, heuristicCardTitle, opponentBattlefieldCard, gameState.OpponentBattlefieldCards, cardDatabase, canTargetIndestructible))
						{
							return true;
						}
					}
					break;
				case Archetype.Boon:
					foreach (MtgCardInstance item in FilterListByType(gameState.OpponentBattlefieldCards, CardType.Creature))
					{
						if (item.CardTypes.Contains(CardType.Creature) && IsAcceptableRemovalTarget(gameState, heuristicCardTitle, item, gameState.OpponentBattlefieldCards, cardDatabase, canTargetIndestructible))
						{
							if (item.AttachedWithIds.Count != 0)
							{
								return false;
							}
							return true;
						}
					}
					break;
				default:
					foreach (MtgCardInstance opponentBattlefieldCard2 in gameState.OpponentBattlefieldCards)
					{
						if (IsCardOfTypeAcceptablePermanentType(gameState, opponentBattlefieldCard2, gameState.OpponentBattlefieldCards, CardType.Creature, cardDatabase))
						{
							return true;
						}
						if (IsCardOfTypeAcceptablePermanentType(gameState, opponentBattlefieldCard2, gameState.OpponentBattlefieldCards, CardType.Land, cardDatabase))
						{
							return true;
						}
						if (IsCardOfTypeAcceptablePermanentType(gameState, opponentBattlefieldCard2, gameState.OpponentBattlefieldCards, CardType.Artifact, cardDatabase))
						{
							return true;
						}
						if (IsCardOfTypeAcceptablePermanentType(gameState, opponentBattlefieldCard2, gameState.OpponentBattlefieldCards, CardType.Enchantment, cardDatabase))
						{
							return true;
						}
					}
					break;
				}
			}
			if (targetController != TurnInformation.ActivePlayer.Player)
			{
				switch (archetype)
				{
				case Archetype.CombatTrick:
				{
					if (gameState.AttackInfo.Count <= 0)
					{
						return false;
					}
					List<MtgCardInstance> list = FilterListByType(gameState.OpponentBattlefieldCards, CardType.Creature);
					bool isLocalPlayer = gameState.GetCardById(gameState.AttackInfo.First().Key).Controller.IsLocalPlayer;
					foreach (MtgCardInstance item2 in list)
					{
						if (IsAcceptableCombatTrickTarget(heuristicCardTitle, item2, gameState, isLocalPlayer, cardDatabase))
						{
							return true;
						}
					}
					foreach (MtgCardInstance item3 in FilterListByType(gameState.LocalPlayerBattlefieldCards, CardType.Creature))
					{
						if (IsAcceptableCombatTrickTarget(heuristicCardTitle, item3, gameState, isLocalPlayer, cardDatabase))
						{
							return true;
						}
					}
					break;
				}
				case Archetype.Recursion:
					foreach (MtgCardInstance visibleCard2 in gameState.LocalGraveyard.VisibleCards)
					{
						_ = visibleCard2;
						if (IsAcceptableRecursion(gameState, gameState.LocalGraveyard.VisibleCards, cardDatabase))
						{
							return true;
						}
					}
					break;
				case Archetype.Removal:
					foreach (MtgCardInstance localPlayerBattlefieldCard in gameState.LocalPlayerBattlefieldCards)
					{
						if (IsAcceptableRemovalTarget(gameState, heuristicCardTitle, localPlayerBattlefieldCard, gameState.LocalPlayerBattlefieldCards, cardDatabase, canTargetIndestructible))
						{
							return true;
						}
					}
					break;
				case Archetype.Boon:
					foreach (MtgCardInstance item4 in FilterListByType(gameState.LocalPlayerBattlefieldCards, CardType.Creature))
					{
						if (IsAcceptablePermanentTarget(gameState, item4, gameState.LocalPlayerBattlefieldCards, CardType.Creature, cardDatabase))
						{
							if (item4.AttachedWithIds.Count != 0)
							{
								return false;
							}
							return true;
						}
					}
					break;
				default:
				{
					IReadOnlyList<MtgCardInstance> localPlayerBattlefieldCards = gameState.LocalPlayerBattlefieldCards;
					foreach (MtgCardInstance localPlayerBattlefieldCard2 in gameState.LocalPlayerBattlefieldCards)
					{
						if (IsCardOfTypeAcceptablePermanentType(gameState, localPlayerBattlefieldCard2, localPlayerBattlefieldCards, CardType.Creature, cardDatabase))
						{
							return true;
						}
						if (IsCardOfTypeAcceptablePermanentType(gameState, localPlayerBattlefieldCard2, localPlayerBattlefieldCards, CardType.Land, cardDatabase))
						{
							return true;
						}
						if (IsCardOfTypeAcceptablePermanentType(gameState, localPlayerBattlefieldCard2, localPlayerBattlefieldCards, CardType.Artifact, cardDatabase))
						{
							return true;
						}
						if (IsCardOfTypeAcceptablePermanentType(gameState, localPlayerBattlefieldCard2, localPlayerBattlefieldCards, CardType.Enchantment, cardDatabase))
						{
							return true;
						}
					}
					break;
				}
				}
			}
			return false;
		}

		public bool IsAcceptableTarget(CardHeuristic rootHeuristic, MtgGameState gameState, MtgEntity possibleTarget, List<Target> possibleTargets, ICardDatabaseAdapter cardDatabase)
		{
			if (possibleTarget is MtgCardInstance && targetType != TargetType.Player)
			{
				List<MtgCardInstance> list = new List<MtgCardInstance>();
				foreach (Target possibleTarget2 in possibleTargets)
				{
					if (possibleTarget.InstanceId != possibleTarget2.TargetInstanceId && gameState.GetCardById(possibleTarget2.TargetInstanceId) != null)
					{
						list.Add(gameState.GetCardById(possibleTarget2.TargetInstanceId));
					}
				}
				foreach (CardType cardType in ((MtgCardInstance)possibleTarget).CardTypes)
				{
					if (rootHeuristic.Archetype == Archetype.CombatTrick)
					{
						bool isLocalPlayer = gameState.GetCardById(gameState.AttackInfo.First().Key).Controller.IsLocalPlayer;
						if (IsAcceptableCombatTrickTarget(rootHeuristic._cardTitle, (MtgCardInstance)possibleTarget, gameState, isLocalPlayer, cardDatabase))
						{
							return true;
						}
					}
					else if (IsAcceptablePermanentTarget(gameState, (MtgCardInstance)possibleTarget, list, cardType, cardDatabase))
					{
						return true;
					}
				}
			}
			else if (possibleTarget is MtgPlayer)
			{
				List<MtgPlayer> list2 = new List<MtgPlayer>();
				foreach (Target possibleTarget3 in possibleTargets)
				{
					if (possibleTarget.InstanceId != possibleTarget3.TargetInstanceId && gameState.GetPlayerById(possibleTarget3.TargetInstanceId) != null)
					{
						list2.Add(gameState.GetPlayerById(possibleTarget3.TargetInstanceId));
					}
				}
				return IsAcceptablePlayerTarget((MtgPlayer)possibleTarget, list2);
			}
			return false;
		}

		private bool IsAcceptablePlayerTarget(MtgPlayer targetPlayer, List<MtgPlayer> otherTargetablePlayers)
		{
			if (targetType != TargetType.Player)
			{
				return false;
			}
			if (targetPlayer.ClientPlayerEnum switch
			{
				GREPlayerNum.LocalPlayer => 2, 
				GREPlayerNum.Opponent => 1, 
				_ => 3, 
			} != (int)targetController)
			{
				return false;
			}
			if (targetFilter == TargetFilter.None)
			{
				return true;
			}
			if (targetFilter == TargetFilter.Life)
			{
				return FulFillsIntParameterFilter(targetPlayer.LifeTotal, otherTargetablePlayers.Select((MtgPlayer player) => player.LifeTotal).ToList());
			}
			Debug.LogWarning("A target filter that does not apply to players has been chosen in CardHeuristic. Returning default (false).");
			return false;
		}

		public bool TargetMatchesAcceptableTypes(MtgCardInstance target)
		{
			return target.CardTypes.Contains(TargetTypeToCardType(targetType));
		}

		private bool IsAcceptablePermanentTarget(MtgGameState gameState, MtgCardInstance targetCard, IReadOnlyList<MtgCardInstance> otherTargetableCards, CardType cardType, ICardDatabaseAdapter cardDatabase)
		{
			if (!TargetMatchesAcceptableTypes(targetCard))
			{
				return false;
			}
			TurnInformation.ActivePlayer activePlayer = ((!targetCard.Controller.IsLocalPlayer) ? TurnInformation.ActivePlayer.Player : TurnInformation.ActivePlayer.AI);
			if (targetCard.AttachedWithIds.Count != 0)
			{
				foreach (uint attachedWithId in targetCard.AttachedWithIds)
				{
					if (gameState.TryGetCard(attachedWithId, out var card) && DeckHeuristic.GetCardHeuristicArchetype(card.TitleId, cardDatabase) == Archetype.Removal)
					{
						return false;
					}
				}
			}
			if (activePlayer != targetController)
			{
				return false;
			}
			IReadOnlyList<MtgCardInstance> readOnlyList = otherTargetableCards;
			if (targetController == TurnInformation.ActivePlayer.AI)
			{
				List<MtgCardInstance> list = new List<MtgCardInstance>();
				foreach (MtgCardInstance otherTargetableCard in otherTargetableCards)
				{
					if (otherTargetableCard.Controller.ClientPlayerEnum == GREPlayerNum.LocalPlayer && otherTargetableCard.CardTypes.Contains(cardType))
					{
						list.Add(otherTargetableCard);
					}
				}
				readOnlyList = list;
			}
			else if (targetController == TurnInformation.ActivePlayer.Player)
			{
				List<MtgCardInstance> list2 = new List<MtgCardInstance>();
				foreach (MtgCardInstance otherTargetableCard2 in otherTargetableCards)
				{
					if (otherTargetableCard2.Controller.ClientPlayerEnum == GREPlayerNum.Opponent && otherTargetableCard2.CardTypes.Contains(cardType))
					{
						list2.Add(otherTargetableCard2);
					}
				}
				readOnlyList = list2;
			}
			bool flag = targetCard.CardTypes.Contains(CardType.Creature);
			if (targetFilter == TargetFilter.None)
			{
				return true;
			}
			if (targetFilter == TargetFilter.Power)
			{
				if (flag)
				{
					return FulFillsIntParameterFilter(targetCard.Power.Value, readOnlyList.Select((MtgCardInstance mtgCardInstance) => mtgCardInstance.Power.Value).ToList());
				}
				return false;
			}
			if (targetFilter == TargetFilter.Toughness)
			{
				if (flag)
				{
					return FulFillsIntParameterFilter(targetCard.Toughness.Value, readOnlyList.Select((MtgCardInstance mtgCardInstance) => mtgCardInstance.Toughness.Value).ToList());
				}
				return false;
			}
			if (targetFilter == TargetFilter.ToughnessMinusDamage)
			{
				if (flag)
				{
					return FulFillsIntParameterFilter(targetCard.Toughness.Value - (int)targetCard.Damage, readOnlyList.Select((MtgCardInstance mtgCardInstance) => mtgCardInstance.Toughness.Value - (int)mtgCardInstance.Damage).ToList());
				}
				return false;
			}
			if (targetFilter == TargetFilter.Color)
			{
				return false;
			}
			if (targetFilter == TargetFilter.ConvertedManaCost)
			{
				int num = 0;
				foreach (ManaQuantity item in targetCard.ManaCostOverride)
				{
					num += (int)item.Count;
				}
				List<int> list3 = new List<int>();
				foreach (MtgCardInstance item2 in readOnlyList)
				{
					int num2 = 0;
					foreach (ManaQuantity item3 in item2.ManaCostOverride)
					{
						num2 += (int)item3.Count;
					}
					list3.Add(num2);
				}
				return FulFillsIntParameterFilter(num, list3);
			}
			if (targetFilter == TargetFilter.SuperType)
			{
				bool result = false;
				foreach (SuperType supertype in targetCard.Supertypes)
				{
					if (FulfillsStringParameterFilter(supertype.ToString()))
					{
						result = true;
						break;
					}
				}
				return result;
			}
			if (targetFilter == TargetFilter.SubType)
			{
				bool result2 = false;
				foreach (SubType subtype in targetCard.Subtypes)
				{
					if (FulfillsStringParameterFilter(subtype.ToString()))
					{
						result2 = true;
						break;
					}
				}
				return result2;
			}
			if (flag && targetFilter == TargetFilter.Blocked)
			{
				return targetCard.BlockedByIds.Count > 0;
			}
			if (flag && targetFilter == TargetFilter.Unblocked)
			{
				return targetCard.BlockingIds.Count == 0;
			}
			if (flag && targetFilter == TargetFilter.Blocking)
			{
				return targetCard.BlockingIds.Count > 0;
			}
			if (flag && targetFilter == TargetFilter.Attacking)
			{
				return targetCard.AttackState == AttackState.Attacking;
			}
			if (targetFilter == TargetFilter.Untapped)
			{
				return !targetCard.IsTapped;
			}
			return false;
		}

		private bool IsAcceptableRemovalTarget(MtgGameState gameState, string heuristicCardTitle, MtgCardInstance cardInstance, IReadOnlyList<MtgCardInstance> otherTargetableCards, ICardDatabaseAdapter cardDatabase, bool canTargetIndestructible = false)
		{
			if (!canTargetIndestructible && cardInstance.Abilities.Exists((AbilityPrintingData ability) => ability.Id == 104))
			{
				return false;
			}
			foreach (MtgEntity item in cardInstance.TargetedBy)
			{
				if (item is MtgCardInstance mtgCardInstance && string.Equals(heuristicCardTitle, cardDatabase.GreLocProvider.GetLocalizedText(mtgCardInstance.TitleId)))
				{
					return false;
				}
			}
			bool flag = false;
			if (IsCardOfTypeAcceptablePermanentType(gameState, cardInstance, otherTargetableCards, CardType.Creature, cardDatabase))
			{
				flag = true;
			}
			else if (IsCardOfTypeAcceptablePermanentType(gameState, cardInstance, otherTargetableCards, CardType.Land, cardDatabase))
			{
				flag = true;
			}
			else if (IsCardOfTypeAcceptablePermanentType(gameState, cardInstance, otherTargetableCards, CardType.Artifact, cardDatabase))
			{
				flag = true;
			}
			else if (IsCardOfTypeAcceptablePermanentType(gameState, cardInstance, otherTargetableCards, CardType.Enchantment, cardDatabase))
			{
				flag = true;
			}
			if (flag)
			{
				if (IsCardAttachedWithRemovalHeuristicArchetype(cardInstance))
				{
					return false;
				}
				return true;
			}
			return false;
			bool IsCardAttachedWithRemovalHeuristicArchetype(MtgCardInstance testInstance)
			{
				if (testInstance.AttachedWithIds.Count != 0)
				{
					foreach (uint attachedWithId in testInstance.AttachedWithIds)
					{
						if (gameState.TryGetCard(attachedWithId, out var card) && DeckHeuristic.GetCardHeuristicArchetype(card.TitleId, cardDatabase) == Archetype.Removal)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public static bool InvolvedInCombat(MtgCardInstance target)
		{
			if (target.AttackState != AttackState.Attacking)
			{
				return target.BlockState == BlockState.Blocking;
			}
			return true;
		}

		public bool IsAcceptableCombatTrickTarget(string heuristicCardTitle, MtgCardInstance targetCard, MtgGameState gameState, bool OnTheAttack, ICardDatabaseAdapter cardDatabase)
		{
			if (!InvolvedInCombat(targetCard))
			{
				return false;
			}
			if (targetFilter != TargetFilter.Blocked && targetFilter != TargetFilter.Blocking)
			{
				return IsAcceptablePermanentTarget(gameState, targetCard, new List<MtgCardInstance>(), CardType.Creature, cardDatabase);
			}
			if (!IsAcceptablePermanentTarget(gameState, targetCard, new List<MtgCardInstance>(), CardType.Creature, cardDatabase))
			{
				return false;
			}
			foreach (MtgEntity item in targetCard.TargetedBy)
			{
				if (item is MtgCardInstance mtgCardInstance && string.Equals(heuristicCardTitle, cardDatabase.GreLocProvider.GetLocalizedText(mtgCardInstance.TitleId)))
				{
					return false;
				}
			}
			if (!IsCardOfTypeAcceptablePermanentType(gameState, targetCard, new List<MtgCardInstance>(), CardType.Creature, cardDatabase))
			{
				return false;
			}
			SimpleGameStateConstruction simpleGameStateConstruction = new SimpleGameStateConstruction(DeckHeuristic, gameState.ActivePlayer.IsLocalPlayer ? gameState.Opponent.InstanceId : gameState.LocalPlayer.InstanceId, gameState, cardDatabase.AbilityDataProvider);
			uint num = uint.MaxValue;
			Dictionary<uint, FinalizedCombatPacket> dictionary = new Dictionary<uint, FinalizedCombatPacket>();
			foreach (KeyValuePair<uint, AttackInfo> item2 in gameState.AttackInfo)
			{
				List<uint> list = item2.Value.OrderedBlockers.Select((OrderedDamageAssignment orderedBlocker) => orderedBlocker.InstanceId).ToList();
				if (list.Contains(targetCard.InstanceId) || targetCard.InstanceId == item2.Key)
				{
					num = item2.Key;
				}
				dictionary.Add(item2.Key, new FinalizedCombatPacket(item2.Key, list));
			}
			if (num == uint.MaxValue)
			{
				return false;
			}
			SimpleGameStateConstruction resultantGameStateFromFullConfiguration = simpleGameStateConstruction.GetResultantGameStateFromFullConfiguration(dictionary);
			Dictionary<uint, Dictionary<string, object>> dictionary2 = new Dictionary<uint, Dictionary<string, object>>();
			dictionary2.Add(targetCard.InstanceId, ParseCombatTrick());
			dictionary[num].CreatureToModificationsToBeApplied = dictionary2;
			SimpleGameStateConstruction resultantGameStateFromFullConfiguration2 = simpleGameStateConstruction.GetResultantGameStateFromFullConfiguration(dictionary);
			float num2 = resultantGameStateFromFullConfiguration.ScoreChange(resultantGameStateFromFullConfiguration2);
			if (OnTheAttack && num2 < 0f)
			{
				return true;
			}
			if (!OnTheAttack && num2 > 0f)
			{
				return true;
			}
			return false;
		}

		public bool IsCardOfTypeAcceptablePermanentType(MtgGameState gameState, MtgCardInstance cardInstance, IReadOnlyList<MtgCardInstance> otherTargetableCards, CardType cardtype, ICardDatabaseAdapter cardDatabase)
		{
			bool result = false;
			if (cardInstance.CardTypes.Contains(cardtype) && IsAcceptablePermanentTarget(gameState, cardInstance, otherTargetableCards, cardtype, cardDatabase))
			{
				result = true;
			}
			return result;
		}

		private bool IsAcceptableRecursion(MtgGameState gameState, List<MtgCardInstance> otherTargetableCards, ICardDatabaseAdapter cardDatabase)
		{
			foreach (MtgCardInstance otherTargetableCard in otherTargetableCards)
			{
				if (IsCardOfTypeAcceptablePermanentType(gameState, otherTargetableCard, otherTargetableCards, CardType.Creature, cardDatabase))
				{
					return true;
				}
				if (IsCardOfTypeAcceptablePermanentType(gameState, otherTargetableCard, otherTargetableCards, CardType.Land, cardDatabase))
				{
					return true;
				}
				if (IsCardOfTypeAcceptablePermanentType(gameState, otherTargetableCard, otherTargetableCards, CardType.Artifact, cardDatabase))
				{
					return true;
				}
				if (IsCardOfTypeAcceptablePermanentType(gameState, otherTargetableCard, otherTargetableCards, CardType.Enchantment, cardDatabase))
				{
					return true;
				}
			}
			return false;
		}

		public Dictionary<string, object> ParseCombatTrick()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			string[] array = filterParameter.Split('{', '}');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(':');
				if (array2[0] == "P/T")
				{
					string[] array3 = array2[1].Split('/');
					int.TryParse(array3[0], out var result);
					dictionary.Add("Power", result);
					int.TryParse(array3[1], out var result2);
					dictionary.Add("Toughness", result2);
				}
				else if (array2[0] == "GainAbility")
				{
					if (!dictionary.ContainsKey("GainAbility"))
					{
						dictionary.Add("GainAbility", new HashSet<uint>());
					}
					int.TryParse(array2[1], out var result3);
					((HashSet<uint>)dictionary["GainAbility"]).Add((uint)result3);
				}
			}
			return dictionary;
		}

		private bool FulFillsIntParameterFilter(int target, List<int> otherTargetsInt)
		{
			AddPreviousFilterParameter(filterParameter);
			int result = 0;
			string[] array = filterParameter.Split(':');
			if (array.Length > 1)
			{
				if (array[0].Contains("TargetID"))
				{
					int.TryParse(previousFilterParameters[Convert.ToInt32(array[1])], out result);
				}
			}
			else
			{
				int.TryParse(filterParameter, out result);
			}
			switch (filterComparer)
			{
			case TargetFilterComparer.None:
				return true;
			case TargetFilterComparer.EqualTo:
				return target == result;
			case TargetFilterComparer.LessThan:
				return target < result;
			case TargetFilterComparer.LessThanOrEqualTo:
				return target <= result;
			case TargetFilterComparer.GreaterThan:
				return target > result;
			case TargetFilterComparer.GreaterThanOrEqualTo:
				return target >= result;
			case TargetFilterComparer.SmallestAvailable:
				if (otherTargetsInt.Count == 0)
				{
					return true;
				}
				foreach (int item in otherTargetsInt)
				{
					if (target > item)
					{
						return false;
					}
					AddPreviousFilterParameter(target.ToString());
				}
				return true;
			case TargetFilterComparer.BiggestAvailable:
				if (otherTargetsInt.Count == 0)
				{
					return true;
				}
				foreach (int item2 in otherTargetsInt)
				{
					if (target < item2)
					{
						return false;
					}
					AddPreviousFilterParameter(target.ToString());
				}
				return true;
			default:
				return false;
			}
		}

		private bool FulfillsStringParameterFilter(string targetString)
		{
			if (filterComparer != TargetFilterComparer.EqualTo)
			{
				Debug.LogWarning("Trying to use a filter comparer other than \"Equal To\" for string comparison in CardHeuristic.");
			}
			return targetString.Contains(filterParameter);
		}

		private void AddPreviousFilterParameter(string newFilterParam)
		{
			if (targetIndex == 1)
			{
				previousFilterParameters.Clear();
			}
			if (previousFilterParameters.ContainsKey(targetIndex))
			{
				previousFilterParameters[targetIndex] = newFilterParam;
			}
			else
			{
				previousFilterParameters.Add(targetIndex, newFilterParam);
			}
		}
	}

	public List<InnerListOfCriteria> acceptableChoices;
}
