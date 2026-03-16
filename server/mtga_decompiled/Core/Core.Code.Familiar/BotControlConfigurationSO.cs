using System.Collections.Generic;
using UnityEngine;

namespace Core.Code.Familiar;

[CreateAssetMenu(fileName = "BotControlConfigurationSO", menuName = "BotControlConfig", order = 1)]
public class BotControlConfigurationSO : ScriptableObject
{
	[SerializeField]
	public List<DeckHeuristic> _deckHeuristics = new List<DeckHeuristic>();

	[SerializeField]
	public float _minWaitTime = 0.5f;

	[SerializeField]
	public float _maxWaitTime = 2.5f;

	[SerializeField]
	public float _maxIdleTime = 20f;

	[Header("Combat Configuration Factors")]
	[SerializeField]
	public uint _maxAttackConfigurationsToExplore = 5000u;

	[SerializeField]
	public uint _maxBlockConfigurationsToExplore = 5000u;

	[SerializeField]
	public float _maxAttackCalculationTime = 5f;

	[SerializeField]
	public float _maxBlockCalculationTime = 5f;

	[SerializeField]
	public float _aiCreatureDensityFactor = 0.2f;

	[SerializeField]
	public float _playerCreatureDensityFactor = 0.2f;

	[SerializeField]
	public float _idleAttackTurnsDensityFactor = 0.33f;

	[Header("Chatter Options")]
	[SerializeField]
	public List<GameStartChatterBucket> gameStartChatterOptions;

	[SerializeField]
	public List<EmoteReplyChatterBucket> emoteReplyChatterOptions;

	[SerializeField]
	public List<ChatterPair> playerLoseChatterOptions;

	[SerializeField]
	public List<ChatterPair> sparkyLoseChatterOptions;

	[SerializeField]
	public float sparkyIdleTimer = 30f;

	[SerializeField]
	public List<ChatterPair> sparkyIdleChatterOptions;

	[SerializeField]
	public float sparkyThinkingTimer = 3f;

	[SerializeField]
	public List<ChatterPair> sparkyThinkingChatterOptions;

	[SerializeField]
	public List<MinimumNumberToVOPercentageBuckets> damageToVOPercentage;

	[SerializeField]
	public List<ChatterPair> damageTakenChatterOptions;

	[SerializeField]
	public List<MinimumNumberToVOPercentageBuckets> CMCToVOPercentage;

	[SerializeField]
	public List<ChatterPair> CMCCastedChatterOptions;

	[SerializeField]
	public List<ChatterPair> sparkyCreatureDeathChatterOptions;
}
