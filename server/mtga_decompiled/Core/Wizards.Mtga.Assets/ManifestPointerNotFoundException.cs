using System;

namespace Wizards.Mtga.Assets;

public class ManifestPointerNotFoundException : ManifestPointerException
{
	public ManifestPointerNotFoundException(Uri pointerUri)
		: base($"Manfiest pointer not found: {pointerUri}", pointerUri)
	{
	}
}
