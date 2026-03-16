using System;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

[Serializable]
[CreateAssetMenu(fileName = "PlayerLifeTotal", menuName = "Heuristic/Boardstate/PlayerLifeTotal", order = 3)]
public class PlayerLifeTotal : BoardstateHeuristic
{
	[Serializable]
	public enum PlayerToCheck
	{
		Opponent,
		AI
	}

	[Serializable]
	public enum LifeThresholdComparison
	{
		LessThanOrEqualTo,
		GreaterThanOrEqualTo
	}

	[SerializeField]
	private int _lifeThreshold;

	[SerializeField]
	private PlayerToCheck _playerToCheck;

	[SerializeField]
	private LifeThresholdComparison _comparison;

	public int LifeThreshold => _lifeThreshold;

	public PlayerToCheck WhichPlayer => _playerToCheck;

	public LifeThresholdComparison Comparison => _comparison;

	private bool doComparison(int lifeTotal)
	{
		return Comparison switch
		{
			LifeThresholdComparison.GreaterThanOrEqualTo => lifeTotal >= _lifeThreshold, 
			LifeThresholdComparison.LessThanOrEqualTo => lifeTotal <= _lifeThreshold, 
			_ => false, 
		};
	}

	public override bool IsMet(MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
	{
		return WhichPlayer switch
		{
			PlayerToCheck.AI => doComparison(gameState.LocalPlayer.LifeTotal), 
			PlayerToCheck.Opponent => doComparison(gameState.Opponent.LifeTotal), 
			_ => false, 
		};
	}
}
