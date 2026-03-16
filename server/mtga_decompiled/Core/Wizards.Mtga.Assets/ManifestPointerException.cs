using System;

namespace Wizards.Mtga.Assets;

public class ManifestPointerException : AssetException
{
	public Uri PointerUri { get; }

	public ManifestPointerException(string message, Uri pointerUri, Exception? exception = null)
		: base(message, exception)
	{
		PointerUri = pointerUri;
	}
}
