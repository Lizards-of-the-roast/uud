using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wizards.Mtga.Logging;

namespace Core.Code.Input;

public class ActionSystemFactory
{
	public static bool CaptureEscapeFeatureToggle => OverridesConfiguration.Local.GetFeatureToggleValue("use_action_system_escape");

	public static bool UseNewInput => OverridesConfiguration.Local.GetFeatureToggleValue("use_new_unity_input");

	public static ActionSystem Create()
	{
		IInputHandler inputHandler = ((!UseNewInput) ? ((IInputHandler)new OldInputHandler()) : ((IInputHandler)new NewInputHandler()));
		return new ActionSystem(inputHandler, new UnityLogger("ActionSystem", LoggerLevel.Debug), CaptureEscapeFeatureToggle);
	}
}
