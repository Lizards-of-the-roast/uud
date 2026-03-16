using System;

namespace Wotc.Mtga.DuelScene.Universal;

[Serializable]
public abstract class GroupValidator : IGroupValidator
{
	public bool ExpectedValue = true;

	public bool IsValidInGroup(ValidatorBlackboard blackboard)
	{
		return Evaluate(blackboard) == ExpectedValue;
	}

	protected abstract bool Evaluate(ValidatorBlackboard blackboard);
}
