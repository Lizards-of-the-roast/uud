using System;

namespace Wotc.Mtga.DuelScene;

public class BattlefieldCombatDamageMediator : IDisposable
{
	private readonly BattlefieldManager _battlefieldManager;

	private readonly CombatAnimationPlayer _combatAnimationPlayer;

	public BattlefieldCombatDamageMediator(BattlefieldManager battlefieldManager, CombatAnimationPlayer combatAnimationPlayer)
	{
		_battlefieldManager = battlefieldManager;
		_combatAnimationPlayer = combatAnimationPlayer;
		_combatAnimationPlayer.OpponentDamaged += _battlefieldManager.PlayBattlefieldWind;
		_combatAnimationPlayer.DamageDealt += _battlefieldManager.ShakeCamera;
	}

	public void Dispose()
	{
		_combatAnimationPlayer.OpponentDamaged -= _battlefieldManager.PlayBattlefieldWind;
		_combatAnimationPlayer.DamageDealt -= _battlefieldManager.ShakeCamera;
	}
}
