namespace Wotc.Mtga.DuelScene;

public readonly struct CombatStateData
{
	public readonly CombatAttackState CombatAttackState;

	public readonly CombatBlockState CombatBlockState;

	public static CombatStateData NotInCombat = new CombatStateData(CombatAttackState.None, CombatBlockState.None);

	private CombatStateData(CombatAttackState attackState, CombatBlockState blockState)
	{
		CombatAttackState = attackState;
		CombatBlockState = blockState;
	}

	public CombatStateData(CombatAttackState attackState)
		: this(attackState, CombatBlockState.None)
	{
	}

	public CombatStateData(CombatBlockState blockState)
		: this(CombatAttackState.None, blockState)
	{
	}
}
