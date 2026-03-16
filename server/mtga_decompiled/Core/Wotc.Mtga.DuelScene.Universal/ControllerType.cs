using System;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.Universal;

[Serializable]
public class ControllerType : GroupValidator
{
	[SerializeField]
	private GREPlayerNum _controllerType;

	protected override bool Evaluate(ValidatorBlackboard blackboard)
	{
		return blackboard.CardData.ControllerNum == _controllerType;
	}

	public override string ToString()
	{
		return _controllerType.ToString();
	}
}
