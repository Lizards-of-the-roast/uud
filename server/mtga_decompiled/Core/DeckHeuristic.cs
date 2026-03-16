using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

[CreateAssetMenu(fileName = "DeckHeuristic", menuName = "Heuristic/Deck", order = 1)]
public class DeckHeuristic : ScriptableObject
{
	[Serializable]
	public struct AbilityWeight
	{
		[SerializeField]
		public AIUtilities.AbilityIds _ability;

		[SerializeField]
		public int _weight;
	}

	[Serializable]
	public struct CreaturesToLifeValueRatio
	{
		[SerializeField]
		public int _creatures;

		[SerializeField]
		public int _life;

		public CreaturesToLifeValueRatio(int creatures, int life)
		{
			_creatures = creatures;
			_life = life;
		}
	}

	public struct WeightedAction
	{
		public float _weight;

		public Wotc.Mtgo.Gre.External.Messaging.Action _action;

		public WeightedAction(Wotc.Mtgo.Gre.External.Messaging.Action action, float weight)
		{
			_action = action;
			_weight = weight;
		}
	}

	[SerializeField]
	public string GUID;

	[SerializeField]
	private List<CardHeuristic> _heuristics = new List<CardHeuristic>();

	[SerializeField]
	public List<AbilityWeight> _abilityWeightOverrides = new List<AbilityWeight>();

	[SerializeField]
	public float _aggressionPerAttackerWeight = 0.25f;

	[SerializeField]
	public float _multiBlockPenaltyPerCreatureBeyondFirst = 0.25f;

	[SerializeField]
	public CreaturesToLifeValueRatio _creaturesToLifeValueRatio = new CreaturesToLifeValueRatio(2, 1);

	protected Dictionary<uint, float> WeightedAbilities = new Dictionary<uint, float>
	{
		{ 1u, 2f },
		{ 3u, 2f },
		{ 6u, 1f },
		{ 7u, 0f },
		{ 8u, 1f },
		{ 10u, 2f },
		{ 12u, 2f },
		{ 14u, 1f },
		{ 15u, 2f },
		{ 1005u, 2f },
		{ 1208u, 1f },
		{ 17519u, 0f },
		{ 118813u, 2f },
		{ 122104u, 1f },
		{ 133509u, 2f },
		{ 133545u, 1f }
	};

	public void OnValidate()
	{
		UpdateAIConfig();
	}

	private void UpdateAIConfig()
	{
		foreach (AbilityWeight abilityWeightOverride in _abilityWeightOverrides)
		{
			WeightedAbilities[(uint)abilityWeightOverride._ability] = abilityWeightOverride._weight;
		}
	}

	public List<WeightedAction> GetReactions(MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		List<WeightedAction> list = new List<WeightedAction>();
		foreach (MtgCardInstance visibleCard in gameState.LocalHand.VisibleCards)
		{
			CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(visibleCard.GrpId);
			if (cardPrintingById == null)
			{
				continue;
			}
			uint titleId = cardPrintingById.TitleId;
			List<CardHeuristic> cardHeuristics = GetCardHeuristics(titleId, cardDatabase);
			foreach (ActionInfo action in visibleCard.Actions)
			{
				foreach (CardHeuristic item in cardHeuristics)
				{
					if (item.AttemptToHoldMana)
					{
						list.Add(new WeightedAction(action.Action, item.Weight));
					}
				}
			}
		}
		return list;
	}

	public List<WeightedAction> GetAcceptableActions(List<Wotc.Mtgo.Gre.External.Messaging.Action> actions, MtgGameState gameState, TurnInformation phaseHistory, ICardDatabaseAdapter cardDatabase)
	{
		List<WeightedAction> list = new List<WeightedAction>();
		foreach (Wotc.Mtgo.Gre.External.Messaging.Action action in actions)
		{
			CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(action.GrpId);
			if (cardPrintingById == null)
			{
				continue;
			}
			uint titleId = cardPrintingById.TitleId;
			List<CardHeuristic> cardHeuristics = GetCardHeuristics(titleId, cardDatabase);
			if (cardHeuristics.Count > 0)
			{
				foreach (CardHeuristic item in cardHeuristics)
				{
					if (action.ActionType == ActionType.Cast)
					{
						if (!item.HasPlayWindowDefined() || item.IsWithinPlayWindow(action, phaseHistory))
						{
							if (titleId == 351806)
							{
								Debug.LogWarning("SAILOR: " + !item.HasPlayWindowDefined() + " " + item.IsWithinPlayWindow(action, phaseHistory));
							}
							if (titleId == 351753)
							{
								Debug.LogWarning("CUTTHROAT: " + !item.HasPlayWindowDefined() + " " + item.IsWithinPlayWindow(action, phaseHistory));
							}
							if ((item.StackTiming == StackTiming.OnEnterTheBattlefield || item.StackTiming == StackTiming.OnLeavesTheBattlefield || item.StackTiming == StackTiming.None || item.HasAcceptableTargets(gameState, cardDatabase)) && item.IsAppropriateBoardstate(gameState, cardDatabase))
							{
								list.Add(new WeightedAction(action, item.Weight));
							}
						}
					}
					else if (action.ActionType == ActionType.Activate && (!item.HasPlayWindowDefined() || item.IsWithinPlayWindow(action, phaseHistory)) && item.HasAcceptableTargets(gameState, cardDatabase))
					{
						list.Add(new WeightedAction(action, item.Weight));
					}
				}
			}
			else if (action.ActionType != ActionType.Activate && action.ActionType != ActionType.ActivateMana)
			{
				list.Add(new WeightedAction(action, 1f));
			}
		}
		return list;
	}

	public static CardHeuristic GetHighestWeightHeuristic(List<CardHeuristic> heuristics)
	{
		float num = float.MinValue;
		CardHeuristic result = null;
		foreach (CardHeuristic heuristic in heuristics)
		{
			if (heuristic.Weight > num)
			{
				num = heuristic.Weight;
				result = heuristic;
			}
		}
		return result;
	}

	public List<Target> GetAcceptableTargets(uint titleId, TargetSelection targetSelection, MtgGameState gameState, ICardDatabaseAdapter cardDatabase, out CardHeuristic chosenHeuristic)
	{
		chosenHeuristic = null;
		List<Target> list = new List<Target>();
		List<CardHeuristic> cardHeuristics = GetCardHeuristics(titleId, cardDatabase);
		cardHeuristics.Sort(delegate(CardHeuristic c1, CardHeuristic c2)
		{
			if (c1.Weight > c2.Weight)
			{
				return -1;
			}
			return (c1.Weight < c2.Weight) ? 1 : 0;
		});
		foreach (CardHeuristic item in cardHeuristics)
		{
			list.Clear();
			foreach (Target target in targetSelection.Targets)
			{
				if (target.LegalAction == SelectAction.Select && gameState.TryGetEntity(target.TargetInstanceId, out var mtgEntity) && item.IsAcceptableTarget(titleId, mtgEntity, targetSelection, gameState, cardDatabase))
				{
					list.Add(target);
				}
			}
			if (list.Count >= targetSelection.MinTargets)
			{
				chosenHeuristic = item;
				break;
			}
		}
		if (targetSelection.MinTargets > list.Count)
		{
			list.Add(targetSelection.Targets[0]);
		}
		return list;
	}

	public Archetype GetCardHeuristicArchetype(uint titleId, ICardDatabaseAdapter cardDatabase)
	{
		List<CardHeuristic> cardHeuristics = GetCardHeuristics(titleId, cardDatabase);
		if (cardHeuristics.Count > 0)
		{
			return cardHeuristics[0].Archetype;
		}
		return Archetype.None;
	}

	public List<CardHeuristic> GetCardHeuristics(uint titleId, ICardDatabaseAdapter cardDatabase)
	{
		List<CardHeuristic> list = new List<CardHeuristic>();
		foreach (CardHeuristic heuristic in _heuristics)
		{
			if ((object)heuristic != null && heuristic.TitleId(cardDatabase) == titleId)
			{
				list.Add(heuristic);
			}
		}
		return list;
	}

	public bool CardHeuristicExists(uint titleId, ICardDatabaseAdapter cardDatabase)
	{
		return _heuristics.Find((CardHeuristic heuristic) => heuristic.TitleId(cardDatabase) == titleId);
	}

	public IReadOnlyList<AcceptableChoiceContainer.InnerListOfCriteria> GetCardHeuristicAcceptableChoices(uint titleId, ICardDatabaseAdapter cardDatabase)
	{
		List<CardHeuristic> cardHeuristics = GetCardHeuristics(titleId, cardDatabase);
		if (cardHeuristics.Count == 0)
		{
			return Array.Empty<AcceptableChoiceContainer.InnerListOfCriteria>();
		}
		return cardHeuristics[0].AcceptableChoiceContainer.acceptableChoices;
	}

	public float GetAbilityScoreScaledToPowerToughness(AbilityPrintingData ability, MtgCardInstance cardObj)
	{
		int num = Math.Max(0, cardObj.Power.Value);
		int num2 = Math.Max(0, cardObj.Toughness.Value);
		float value = 0f;
		if (WeightedAbilities.TryGetValue(ability.Id, out value))
		{
			switch (ability.Id)
			{
			case 3u:
			case 6u:
			case 8u:
			case 12u:
			case 14u:
				return value * (float)(num + num2) / 2f;
			default:
				return value;
			}
		}
		return value;
	}

	public float ScoreCard(MtgCardInstance cardObj, NPEDirector npeDirector = null)
	{
		int num = Math.Max(0, cardObj.Power.Value);
		int num2 = Math.Max(0, cardObj.Toughness.Value);
		float num3 = 0f;
		foreach (AbilityPrintingData ability in cardObj.Abilities)
		{
			num3 += GetAbilityScoreScaledToPowerToughness(ability, cardObj);
		}
		if (npeDirector != null && npeDirector.GameSpecificConfiguration._creaturesToChumpWith.Contains(cardObj.GrpId))
		{
			num3 -= 100f;
		}
		num3 += (float)num;
		return num3 + (float)num2;
	}
}
