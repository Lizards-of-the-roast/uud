using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.Universal;

[Serializable]
public class ListValidator : GroupValidator
{
	[SerializeField]
	[SerializeReference]
	private List<GroupValidator> _childValidators = new List<GroupValidator>();

	[SerializeField]
	private ValidatorBehavior _behavior;

	protected override bool Evaluate(ValidatorBlackboard blackboard)
	{
		ValidatorBehavior behavior = _behavior;
		if (behavior == ValidatorBehavior.AND || behavior != ValidatorBehavior.OR)
		{
			foreach (GroupValidator childValidator in _childValidators)
			{
				if (!((IGroupValidator)childValidator).IsValidInGroup(blackboard))
				{
					return false;
				}
			}
			return true;
		}
		foreach (GroupValidator childValidator2 in _childValidators)
		{
			if (((IGroupValidator)childValidator2).IsValidInGroup(blackboard))
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		return "(" + string.Join((_behavior == ValidatorBehavior.AND) ? " & " : " || ", _childValidators.Select((GroupValidator x) => x.ToString())) + ")";
	}
}
