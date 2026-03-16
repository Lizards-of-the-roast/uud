using UnityEngine;

namespace Core.Shared.Code.DebugTools;

public static class CPUUsageProviderFactory
{
	public static ICPUUsageProvider Create()
	{
		RuntimePlatform platform = Application.platform;
		if (platform != RuntimePlatform.IPhonePlayer)
		{
			_ = 11;
			return new NullCPUUsageProvider();
		}
		return new iOSCPUUsageProvider();
	}
}
