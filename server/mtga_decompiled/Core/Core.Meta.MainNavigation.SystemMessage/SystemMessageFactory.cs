using Core.Code.Input;
using MTGA.KeyboardManager;
using Wizards.Mtga;

namespace Core.Meta.MainNavigation.SystemMessage;

public class SystemMessageFactory
{
	public static ISystemMessageManager Create()
	{
		return SystemMessageManager.Initialize(Pantry.Get<KeyboardManager>(), Pantry.Get<IActionSystem>(), Pantry.Get<IBILogger>());
	}
}
