using System.Collections;
using Wizards.Mtga;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class HarnessEnvironmentInitializer : HarnessInitializer
{
	public override IEnumerator OnInitialize()
	{
		Pantry.CurrentEnvironment = new EnvironmentDescription
		{
			HostPlatform = HostPlatform.Harness
		};
		yield break;
	}

	public override string Status()
	{
		return "Initializing environment.";
	}
}
