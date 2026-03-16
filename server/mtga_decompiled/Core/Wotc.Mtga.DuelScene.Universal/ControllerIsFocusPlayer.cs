using System;

namespace Wotc.Mtga.DuelScene.Universal;

[Serializable]
public class ControllerIsFocusPlayer : GroupValidator
{
	protected override bool Evaluate(ValidatorBlackboard blackboard)
	{
		return blackboard.IsFocusPlayer;
	}

	public override string ToString()
	{
		return (ExpectedValue ? "Is" : "Not") + ": FocusPlayer";
	}
}
