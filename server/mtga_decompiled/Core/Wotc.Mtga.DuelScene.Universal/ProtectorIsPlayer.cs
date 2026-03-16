using System;
using System.Linq;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Universal;

[Serializable]
public class ProtectorIsPlayer : GroupValidator
{
	[SerializeField]
	[Tooltip("0=Local Player, 1=player after local, 2=player after that, etc.")]
	private uint _localPlayerOffset;

	protected override bool Evaluate(ValidatorBlackboard blackboard)
	{
		if (blackboard.Players == null)
		{
			return false;
		}
		int num = blackboard.Players.FindIndex((MtgPlayer player) => player.IsLocalPlayer);
		uint instanceId = blackboard.Players[(num + (int)_localPlayerOffset) % blackboard.Players.Count].InstanceId;
		foreach (MtgPlayer player in blackboard.Players)
		{
			if (player.InstanceId != instanceId)
			{
				continue;
			}
			foreach (DesignationData item in player.Designations.Where((DesignationData x) => x.Type == Designation.Protector))
			{
				if (item.AffectorId == blackboard.CardData.InstanceId && item.AffectedId == instanceId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string ToString()
	{
		string arg = (ExpectedValue ? "Is" : "Not");
		return $"{arg}: Protector {_localPlayerOffset}";
	}
}
