using System;

namespace Wotc.Mtga.DuelScene;

[Flags]
public enum CombatAttackState
{
	None = 0,
	CanAttack = 1,
	CanExert = 2,
	IsAttacking = 4,
	IsExerting = 8,
	IsBlocked = 0x10,
	CanEnlist = 0x20,
	IsEnlisting = 0x40
}
