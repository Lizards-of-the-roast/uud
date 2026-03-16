namespace Core.Shared.Code.DebugTools;

public class iOSThermalStatusProvider : IThermalStatusProvider
{
	private int _lastThermalStatus;

	private readonly InvokeAtInterval _thermalStatusUpdater;

	public iOSThermalStatusProvider()
	{
		_thermalStatusUpdater = new InvokeAtInterval(120, delegate
		{
		});
	}

	public int GetThermalStatus()
	{
		_thermalStatusUpdater.Increment();
		return _lastThermalStatus;
	}
}
