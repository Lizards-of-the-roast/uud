using System.IO;

namespace Wizards.Mtga.Assets;

public class InsufficientStorageException : AssetException
{
	public long BytesNeeded { get; }

	public long BytesAvailable { get; }

	public InsufficientStorageException(long bytesNeeded, long bytesAvailable, IOException? exception = null)
		: base("Insufficient storage available", exception)
	{
		BytesNeeded = bytesNeeded;
		BytesAvailable = bytesAvailable;
	}
}
