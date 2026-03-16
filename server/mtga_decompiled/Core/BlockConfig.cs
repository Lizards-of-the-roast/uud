public readonly struct BlockConfig
{
	public readonly uint MaxBlockConfigurationsToExplore;

	public readonly float MaxBlockCalculationTime;

	public BlockConfig(uint maxBlockConfigurationsToExplore, float maxBlockCalculationTime)
	{
		MaxBlockConfigurationsToExplore = maxBlockConfigurationsToExplore;
		MaxBlockCalculationTime = maxBlockCalculationTime;
	}

	public BlockConfig(BlockConfig other, uint? maxBlockConfigurationsToExplore = null, float? maxBlockCalculationTime = null)
		: this(maxBlockConfigurationsToExplore ?? other.MaxBlockConfigurationsToExplore, maxBlockCalculationTime ?? other.MaxBlockCalculationTime)
	{
	}
}
