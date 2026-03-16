namespace Wizards.Mtga.Assets;

public static class AssetBundleProvisionProgressExtensions
{
	public static AssetBundleProvisionProgress ToProgress(this AssetBundleProvisionStage stage, long completed, long total = 0L)
	{
		return new AssetBundleProvisionProgress(stage, completed, total);
	}

	public static AssetBundleProvisionProgress ToProgress(this AssetBundleProvisionStage stage, double progress)
	{
		return new AssetBundleProvisionProgress(stage, (long)(progress * 100.0), 100L);
	}

	public static AssetBundleProvisionProgress ToCompletedProgress(this AssetBundleProvisionStage stage, long total = 1L)
	{
		return new AssetBundleProvisionProgress(stage, total, total);
	}
}
