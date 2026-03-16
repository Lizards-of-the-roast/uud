using Wizards.Mtga.UI;

namespace Wotc.Mtga.Login;

public class LoginQueuePanel : Panel
{
	private void OnEnable()
	{
		ScreenKeepAlive.KeepScreenAwake();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ScreenKeepAlive.AllowScreenTimeout();
	}
}
