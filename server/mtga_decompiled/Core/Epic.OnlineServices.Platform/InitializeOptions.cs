namespace Epic.OnlineServices.Platform;

public class InitializeOptions
{
	public int ApiVersion => 2;

	public AllocateMemoryFunc AllocateMemoryFunction { get; set; }

	public ReallocateMemoryFunc ReallocateMemoryFunction { get; set; }

	public ReleaseMemoryFunc ReleaseMemoryFunction { get; set; }

	public string ProductName { get; set; }

	public string ProductVersion { get; set; }
}
