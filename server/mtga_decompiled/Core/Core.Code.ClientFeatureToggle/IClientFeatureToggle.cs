namespace Core.Code.ClientFeatureToggle;

public interface IClientFeatureToggle
{
	bool GetToggleValue();

	bool SetOverrideConfigValue(bool toggle);

	bool SetKillSwitchValue(bool toggle);

	bool ClearKillSwitchValue();
}
