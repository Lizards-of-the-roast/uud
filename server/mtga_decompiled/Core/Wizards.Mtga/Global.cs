using UnityEngine;
using Wizards.Mtga.Platforms;

namespace Wizards.Mtga;

public static class Global
{
	private static IClientVersionInfo _version;

	public static IClientVersionInfo VersionInfo
	{
		get
		{
			if (_version == null)
			{
				_version = PlatformContext.CreateVersionInfo(Application.platform, Metadata.ContentVersionBuildPart, Metadata.SourceVersion, Metadata.BuildInfo);
			}
			return _version;
		}
		set
		{
			_version = value;
		}
	}
}
