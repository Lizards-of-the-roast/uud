using System;

namespace Wizards.Mtga.Assets;

public class ManifestLoadException : AssetException
{
	public ManifestLoadException(string message)
		: base("Could not load manifest file: " + message)
	{
	}

	public ManifestLoadException(Exception exception)
		: base("Could not load manifest file: " + exception.Message)
	{
	}
}
