using System.IO;
using Wizards.Mtga.Storage;

namespace Wizards.Mtga;

public static class OverridesConfigurationExtensions
{
	public static string GetOverrideConfigurationPath(this IStorageContext storage)
	{
		return Path.Combine(storage.LocalPersistedStoragePath, "overrides.conf");
	}
}
