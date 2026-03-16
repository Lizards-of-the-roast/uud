using System;
using Wotc.Mtga.Login;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_AutoRegister : AutoPlayAction
{
	protected override void OnExecute()
	{
		RegistrationPanel registrationPanel = ComponentGetters.FindComponent<RegistrationPanel>();
		if (!registrationPanel)
		{
			Fail("Could not find a registration panel");
			return;
		}
		string text = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
		string text2 = "autoplay-" + text;
		registrationPanel.FillUser("autoplay", text2);
		Complete("Registered user: " + text2);
	}
}
