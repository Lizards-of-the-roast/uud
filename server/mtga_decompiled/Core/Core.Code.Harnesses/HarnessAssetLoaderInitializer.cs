using System.Collections;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wotc.Mtga;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class HarnessAssetLoaderInitializer : HarnessInitializer
{
	public override IEnumerator OnInitialize()
	{
		AssetLoader.Initialize(new BILogger(new ConsoleLogger()), Pantry.Get<ResourceErrorMessageManager>());
		yield break;
	}

	public override string Status()
	{
		return "Initializing Asset Loader.";
	}
}
