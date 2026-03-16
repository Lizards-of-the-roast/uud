using System;

namespace Core.Code.ClientFeatureToggle;

public class RoleOverrideFeatureToggle : ClientFeatureToggleBase
{
	private Func<bool> _hasDebugging;

	private Func<bool> _hasWotcAccess;

	public RoleOverrideFeatureToggle(bool defaultToggleValue, Func<bool> hasDebugging, Func<bool> hasWotcAccess)
		: base(defaultToggleValue)
	{
		_hasDebugging = hasDebugging;
		_hasWotcAccess = hasWotcAccess;
	}

	public override bool GetToggleValue()
	{
		if (overrideConfigToggleValue > ClientFeatureToggleState.NotSet)
		{
			return GetBoolFromToggleState(overrideConfigToggleValue);
		}
		if (killSwitchToggleValue == ClientFeatureToggleState.False && (_hasDebugging() || _hasWotcAccess()))
		{
			return true;
		}
		if (killSwitchToggleValue > ClientFeatureToggleState.NotSet)
		{
			return GetBoolFromToggleState(killSwitchToggleValue);
		}
		return defaultClientToggleValue;
	}
}
