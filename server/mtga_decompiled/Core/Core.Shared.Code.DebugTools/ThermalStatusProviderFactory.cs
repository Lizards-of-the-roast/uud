using UnityEngine;

namespace Core.Shared.Code.DebugTools;

public static class ThermalStatusProviderFactory
{
	public static IThermalStatusProvider Create()
	{
		return Application.platform switch
		{
			RuntimePlatform.Android => new AndroidThermalStatusProvider(), 
			RuntimePlatform.IPhonePlayer => new iOSThermalStatusProvider(), 
			_ => new NullThermalStatusProvider(), 
		};
	}
}
