namespace Core.Shared.Code.DebugTools;

public class iOSCPUUsageProvider : ICPUUsageProvider
{
	private float _lastCPUUsage;

	private readonly InvokeAtInterval _cpuUsageUpdater;

	public iOSCPUUsageProvider()
	{
		_cpuUsageUpdater = new InvokeAtInterval(120, delegate
		{
		});
	}

	public float GetTotalCPUUsage()
	{
		_cpuUsageUpdater.Increment();
		return _lastCPUUsage;
	}
}
