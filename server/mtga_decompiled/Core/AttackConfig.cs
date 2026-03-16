public readonly struct AttackConfig
{
	public readonly uint MaxAttackConfigurationsToExplore;

	public readonly float MaxAttackCalculationTime;

	public readonly float AICreatureDensityFactor;

	public readonly float PlayerCreatureDensityFactor;

	public readonly float IdleAttackTurnsDensityFactor;

	public AttackConfig(uint maxAttackConfigurationsToExplore, float maxAttackCalculationTime, float aiCreatureDensityFactor, float playerCreatureDensityFactor, float idleAttackTurnsDensityFactor)
	{
		MaxAttackConfigurationsToExplore = maxAttackConfigurationsToExplore;
		MaxAttackCalculationTime = maxAttackCalculationTime;
		AICreatureDensityFactor = aiCreatureDensityFactor;
		PlayerCreatureDensityFactor = playerCreatureDensityFactor;
		IdleAttackTurnsDensityFactor = idleAttackTurnsDensityFactor;
	}

	public AttackConfig(AttackConfig other, uint? maxAttackConfigurationsToExplore = null, float? maxAttackCalculationTime = null, float? aiCreatureDensityFactor = null, float? playerCreatureDensityFactor = null, float? idleAttackTurnsDensityFactor = null)
		: this(maxAttackConfigurationsToExplore ?? other.MaxAttackConfigurationsToExplore, maxAttackCalculationTime ?? other.MaxAttackCalculationTime, aiCreatureDensityFactor ?? other.AICreatureDensityFactor, playerCreatureDensityFactor ?? other.PlayerCreatureDensityFactor, idleAttackTurnsDensityFactor ?? other.IdleAttackTurnsDensityFactor)
	{
	}
}
