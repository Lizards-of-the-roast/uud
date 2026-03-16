namespace Wotc.Mtga.DuelScene.Universal;

public interface IGroupValidator
{
	bool IsValidInGroup(ValidatorBlackboard blackboard);
}
