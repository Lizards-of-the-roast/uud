namespace Core.Shared.Code.DebugTools;

public class NullCPUUsageProvider : ICPUUsageProvider
{
	public float GetTotalCPUUsage()
	{
		return -1f;
	}
}
