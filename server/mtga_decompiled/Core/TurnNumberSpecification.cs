using System;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

[Serializable]
[CreateAssetMenu(fileName = "TurnNumberSpecification", menuName = "Heuristic/Boardstate/TurnNumberSpecification", order = 5)]
public class TurnNumberSpecification : BoardstateHeuristic
{
	[Serializable]
	public enum TurnThreshold
	{
		On,
		Before,
		After
	}

	[SerializeField]
	private uint _turnNumber;

	[SerializeField]
	private TurnThreshold _turnThresholdSpecification;

	public uint TurnNumber => _turnNumber;

	public TurnThreshold Specification => _turnThresholdSpecification;

	public override bool IsMet(MtgGameState gameState, ICardDatabaseAdapter gameManager)
	{
		uint gameWideTurn = gameState.GameWideTurn;
		return Specification switch
		{
			TurnThreshold.On => _turnNumber == gameWideTurn, 
			TurnThreshold.Before => _turnNumber < gameWideTurn, 
			TurnThreshold.After => _turnNumber > gameWideTurn, 
			_ => false, 
		};
	}
}
