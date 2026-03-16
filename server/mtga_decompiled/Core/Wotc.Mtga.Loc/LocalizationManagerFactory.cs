using System;
using Core.Code.Localization;
using Wizards.Mtga;

namespace Wotc.Mtga.Loc;

public static class LocalizationManagerFactory
{
	public static IClientLocProvider Create()
	{
		Languages.CurrentLanguage = MDNPlayerPrefs.PLAYERPREFS_ClientLanguage;
		EnvironmentDescription currentEnvironment = Pantry.CurrentEnvironment;
		HostPlatform hostPlatform = currentEnvironment.HostPlatform;
		if ((uint)(hostPlatform - 1) <= 1u)
		{
			return Languages.ActiveLocProvider = new CompositeLocProvider(new UnityCrossThreadLogger(), LocLibrary.Instance);
		}
		throw new ArgumentOutOfRangeException($"Unrecognized Environment {currentEnvironment.HostPlatform}");
	}
}
