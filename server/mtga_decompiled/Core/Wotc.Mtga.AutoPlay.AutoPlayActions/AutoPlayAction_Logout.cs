using Core.Shared.Code.Connection;
using Wizards.Mtga;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_Logout : AutoPlayAction
{
	protected override void OnExecute()
	{
		Pantry.Get<FrontDoorConnectionManager>().LogoutAndRestartGame("Logout");
		Complete("Successfully logged out");
	}
}
