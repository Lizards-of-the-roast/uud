using System;

namespace Core.Code.ClientFeatureToggle;

public class ClientFeatureToggleBase : IClientFeatureToggle
{
	protected bool defaultClientToggleValue;

	protected ClientFeatureToggleState killSwitchToggleValue;

	protected ClientFeatureToggleState overrideConfigToggleValue;

	public ClientFeatureToggleBase(bool defaultToggleValue)
	{
		defaultClientToggleValue = defaultToggleValue;
	}

	public virtual bool GetToggleValue()
	{
		if (overrideConfigToggleValue > ClientFeatureToggleState.NotSet)
		{
			return GetBoolFromToggleState(overrideConfigToggleValue);
		}
		if (killSwitchToggleValue > ClientFeatureToggleState.NotSet)
		{
			return GetBoolFromToggleState(killSwitchToggleValue);
		}
		return defaultClientToggleValue;
	}

	public bool SetOverrideConfigValue(bool toggle)
	{
		overrideConfigToggleValue = GetToggleStateFromBool(toggle);
		return GetToggleValue();
	}

	public bool SetKillSwitchValue(bool toggle)
	{
		killSwitchToggleValue = GetToggleStateFromBool(toggle);
		return GetToggleValue();
	}

	public bool ClearKillSwitchValue()
	{
		killSwitchToggleValue = ClientFeatureToggleState.NotSet;
		return GetToggleValue();
	}

	protected ClientFeatureToggleState GetToggleStateFromBool(bool toggle)
	{
		if (toggle)
		{
			return ClientFeatureToggleState.True;
		}
		return ClientFeatureToggleState.False;
	}

	protected bool GetBoolFromToggleState(ClientFeatureToggleState state)
	{
		return state switch
		{
			ClientFeatureToggleState.False => false, 
			ClientFeatureToggleState.True => true, 
			_ => throw new Exception("Attempting to get bool from ClientFeatureToggle that is not set."), 
		};
	}
}
