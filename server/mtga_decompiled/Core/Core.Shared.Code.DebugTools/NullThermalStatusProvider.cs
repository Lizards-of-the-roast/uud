namespace Core.Shared.Code.DebugTools;

public class NullThermalStatusProvider : IThermalStatusProvider
{
	public int GetThermalStatus()
	{
		return -1;
	}
}
