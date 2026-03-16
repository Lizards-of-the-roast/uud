using System.Collections;
using System.Threading.Tasks;
using Wizards.Mtga.Configuration;
using Wizards.Mtga.Platforms;
using com.wizards.harness;

namespace Core.Code.Harnesses;

public class HarnessAssetBundleManagerLoader : HarnessInitializer
{
	public override IEnumerator OnInitialize()
	{
		PlatformContext.GetStorageContext();
		Task<AssetsConfiguration> task = PlatformContext.GetConfigurationLoader().LoadAssetsConfiguration();
		Task.WaitAll(task);
		AssetBundleManager.Create(task.Result);
		yield break;
	}

	public override string Status()
	{
		return "Loading Asset Bundle Manager.";
	}
}
