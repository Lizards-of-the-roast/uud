using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

[CreateAssetMenu(fileName = "CardHeuristic", menuName = "Heuristic/Card", order = 1)]
public class CardHeuristic : ScriptableObject
{
	[SerializeField]
	public string _cardTitle;

	[SerializeField]
	private float _weight = 1f;

	[SerializeField]
	private bool _attemptToHoldMana;

	private uint _titleId;

	private string _workingTitle;

	[Header("Card Effect")]
	[SerializeField]
	private Archetype _archetype;

	[SerializeField]
	private bool _canTargetIndestructible;

	[SerializeField]
	private StackTiming _stackTiming;

	[SerializeField]
	private PlayWindows _playWindows;

	[SerializeField]
	private List<BoardstateHeuristic> _boardstateHeuristics;

	[SerializeField]
	private AcceptableChoiceContainer _acceptableTargets;

	public string CardTitle => _cardTitle;

	public float Weight => _weight;

	public bool AttemptToHoldMana => _attemptToHoldMana;

	public Archetype Archetype => _archetype;

	public bool CanTargetIndestructible => _canTargetIndestructible;

	public StackTiming StackTiming => _stackTiming;

	public PlayWindows PlayWindows => _playWindows;

	public List<BoardstateHeuristic> BoardstateHeuristics => _boardstateHeuristics;

	public AcceptableChoiceContainer AcceptableChoiceContainer => _acceptableTargets;

	public uint TitleId(ICardDatabaseAdapter cardDatabase)
	{
		bool flag = _workingTitle != null && _workingTitle.Equals(_cardTitle);
		if (_titleId == 0 || !flag)
		{
			if (cardDatabase != null && !string.IsNullOrEmpty(_cardTitle))
			{
				IReadOnlyList<CardPrintingData> printingsByEnglishTitle = cardDatabase.DatabaseUtilities.GetPrintingsByEnglishTitle(_cardTitle);
				if (printingsByEnglishTitle.Count > 0)
				{
					CardPrintingData cardPrintingData = printingsByEnglishTitle[0];
					_titleId = cardPrintingData.TitleId;
					_workingTitle = _cardTitle;
				}
			}
			return _titleId;
		}
		return _titleId;
	}

	private bool IsWithinPlayWindow(TurnInformation turnInfo)
	{
		foreach (PlayWindows.PlayWindow playWindow in _playWindows.playWindowList)
		{
			if ((playWindow.activePlayer == turnInfo.activePlayer || playWindow.activePlayer == TurnInformation.ActivePlayer.Any) && (playWindow.phase == Phase.None || (playWindow.phase == turnInfo.phase && (playWindow.step == Step.None || playWindow.step == turnInfo.step))))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPlayWindowDefined()
	{
		return _playWindows.playWindowList.Count > 0;
	}

	public bool IsWithinPlayWindow(Action action, TurnInformation turnPhase)
	{
		if (_stackTiming == StackTiming.OnCast || _stackTiming == StackTiming.OnEnterTheBattlefield)
		{
			if (action.ActionType == ActionType.Cast)
			{
				return IsWithinPlayWindow(turnPhase);
			}
			return false;
		}
		if (action.ActionType == ActionType.Cast)
		{
			return true;
		}
		return IsWithinPlayWindow(turnPhase);
	}

	public bool HasAcceptableTargets(MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		foreach (AcceptableChoiceContainer.InnerListOfCriteria acceptableChoice in _acceptableTargets.acceptableChoices)
		{
			int currentTargetID = 1;
			List<AcceptableChoiceContainer.AcceptableChoice> list = acceptableChoice.criteria.FindAll((AcceptableChoiceContainer.AcceptableChoice target) => target.targetIndex == currentTargetID);
			bool flag = true;
			while (list.Count > 0)
			{
				foreach (AcceptableChoiceContainer.AcceptableChoice item in list)
				{
					if (!item.AcceptableTargetExists(_cardTitle, gameState, _archetype, cardDatabase, _canTargetIndestructible))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
				currentTargetID++;
				list = acceptableChoice.criteria.FindAll((AcceptableChoiceContainer.AcceptableChoice target) => target.targetIndex == currentTargetID);
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAcceptableTarget(uint sourceGrpID, MtgEntity targetEntity, TargetSelection targetSelection, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		if (targetEntity is MtgCardInstance)
		{
			MtgCardInstance mtgCardInstance = (MtgCardInstance)targetEntity;
			if (_archetype == Archetype.Removal && !_canTargetIndestructible && mtgCardInstance.Abilities.Exists((AbilityPrintingData ability) => ability.Id == 104))
			{
				return false;
			}
			string localizedText = cardDatabase.GreLocProvider.GetLocalizedText(cardDatabase.DatabaseUtilities.GetPrintingsByTitleId(sourceGrpID)[0].TitleId);
			foreach (MtgEntity item in mtgCardInstance.TargetedBy)
			{
				if (item is MtgCardInstance mtgCardInstance2 && string.Equals(localizedText, cardDatabase.GreLocProvider.GetLocalizedText(mtgCardInstance2.TitleId)))
				{
					return false;
				}
			}
		}
		foreach (AcceptableChoiceContainer.InnerListOfCriteria acceptableChoice in _acceptableTargets.acceptableChoices)
		{
			List<AcceptableChoiceContainer.AcceptableChoice> list = acceptableChoice.criteria.FindAll((AcceptableChoiceContainer.AcceptableChoice targetFilter) => targetFilter.targetIndex == targetSelection.TargetIdx);
			bool flag = true;
			foreach (AcceptableChoiceContainer.AcceptableChoice item2 in list)
			{
				if (!item2.IsAcceptableTarget(this, gameState, targetEntity, targetSelection.Targets.ToList(), cardDatabase))
				{
					flag = false;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private int GetAvailableManaCount(MtgGameState gameState)
	{
		int num = 0;
		foreach (MtgCardInstance localPlayerBattlefieldCard in gameState.LocalPlayerBattlefieldCards)
		{
			if (localPlayerBattlefieldCard.IsTapped)
			{
				continue;
			}
			foreach (AbilityPrintingData ability in localPlayerBattlefieldCard.Abilities)
			{
				if (ability.SubCategory == AbilitySubCategory.Mana)
				{
					num++;
				}
			}
		}
		return num;
	}

	public Dictionary<string, object> CanCombatTrickBeApplied(MtgCardInstance trickTarget, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		if (_archetype == Archetype.CombatTrick)
		{
			foreach (AcceptableChoiceContainer.InnerListOfCriteria acceptableChoice in _acceptableTargets.acceptableChoices)
			{
				using List<AcceptableChoiceContainer.AcceptableChoice>.Enumerator enumerator2 = acceptableChoice.criteria.GetEnumerator();
				if (enumerator2.MoveNext())
				{
					AcceptableChoiceContainer.AcceptableChoice current = enumerator2.Current;
					if (!current.TargetMatchesAcceptableTypes(trickTarget))
					{
						return null;
					}
					if (current.targetController == TurnInformation.ActivePlayer.AI && !trickTarget.Controller.IsLocalPlayer)
					{
						return null;
					}
					if (current.targetController == TurnInformation.ActivePlayer.Player && trickTarget.Controller.IsLocalPlayer)
					{
						return null;
					}
					IReadOnlyList<CardPrintingData> printingsByEnglishTitle = cardDatabase.DatabaseUtilities.GetPrintingsByEnglishTitle(_cardTitle.ToLower());
					if (printingsByEnglishTitle != null && printingsByEnglishTitle.Count > 0 && GetAvailableManaCount(gameState) < printingsByEnglishTitle[0].ConvertedManaCost)
					{
						return null;
					}
					return current.ParseCombatTrick();
				}
			}
		}
		return null;
	}

	public bool IsAppropriateBoardstate(MtgGameState gamestate, ICardDatabaseAdapter cardDatabase)
	{
		foreach (BoardstateHeuristic boardstateHeuristic in _boardstateHeuristics)
		{
			if (!boardstateHeuristic.IsMet(gamestate, cardDatabase))
			{
				return false;
			}
		}
		return true;
	}
}
