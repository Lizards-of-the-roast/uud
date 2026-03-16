using System;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Universal;

[Serializable]
public class ControllerOffset : GroupValidator
{
	[SerializeField]
	[Tooltip("0=Local Player, 1=player after local, 2=player after that, etc.")]
	private uint _localPlayerOffset;

	protected override bool Evaluate(ValidatorBlackboard blackboard)
	{
		if (blackboard.Players != null)
		{
			int num = blackboard.Players.FindIndex((MtgPlayer player) => player.IsLocalPlayer);
			uint instanceId = blackboard.Players[(num + (int)_localPlayerOffset) % blackboard.Players.Count].InstanceId;
			return blackboard.CardData.Controller.InstanceId == instanceId;
		}
		return false;
	}

	public override string ToString()
	{
		return $"Offset {_localPlayerOffset}";
	}
}
