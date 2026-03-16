using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class RemoveDesignationUXEvent : DesignationUXEventBase
{
	public RemoveDesignationUXEvent(DesignationData designation, IDesignationController designationController, GameManager gameManager)
		: base(designation, designationController, gameManager)
	{
	}

	public override void Execute()
	{
		_designationController.RemoveDesignation(Designation);
		Complete();
	}
}
