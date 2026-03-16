public class BotTool
{
	public float MinWaitTime = 0.5f;

	public float MaxWaitTime = 2.5f;

	public float MaxIdleTime = 20f;

	public DeckHeuristic DeckHeuristic { get; private set; }

	public AttackConfig AttackConfig { get; private set; } = AttackingAI.DefaultConfig;

	public BlockConfig BlockConfig { get; private set; } = BlockingAI.DefaultConfig;

	public static BotTool Create()
	{
		return new BotTool();
	}

	public void SetDeckHeuristic(DeckHeuristic deckHeuristic)
	{
		DeckHeuristic = deckHeuristic;
	}

	public void SetAttackConfig(AttackConfig attackConfig)
	{
		AttackConfig config = (AttackConfig = attackConfig);
		AttackingAI.Config = config;
	}

	public void SetBlockConfig(BlockConfig blockConfig)
	{
		BlockConfig config = (BlockConfig = blockConfig);
		BlockingAI.Config = config;
	}
}
