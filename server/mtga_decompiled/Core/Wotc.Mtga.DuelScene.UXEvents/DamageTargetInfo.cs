using GreClient.Rules;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class DamageTargetInfo
{
	public Transform Transform;

	public readonly Transform DamageTextTransform;

	public readonly int DamageDealt;

	public readonly DamageType DamageType;

	public readonly MtgEntity TargetEntity;

	public readonly uint PlayerInstanceId;

	public readonly bool DamagedPlayerWasOpponent;

	public bool DamageDealtToPlayer => PlayerInstanceId != 0;

	public bool DamageDealtToCard => !DamageDealtToPlayer;

	public DamageTargetInfo(Transform transform, Transform damageTextTransform, int damageDealt, DamageType damageType, MtgEntity targetEntity)
	{
		Transform = transform;
		DamageTextTransform = damageTextTransform;
		DamageDealt = damageDealt;
		DamageType = damageType;
		TargetEntity = targetEntity;
		if (targetEntity is MtgPlayer mtgPlayer)
		{
			PlayerInstanceId = mtgPlayer.InstanceId;
			DamagedPlayerWasOpponent = mtgPlayer.ClientPlayerEnum == GREPlayerNum.Opponent;
		}
	}
}
