namespace Wizards.Mtga.Assets;

public enum AssetBundleProvisionStage
{
	None = 0,
	GetManifest = 1,
	CollectCompletedBundles = 2,
	CollectDownloadList = 3,
	SafeModeHash = 4,
	UnpackBuiltInBundles = 5,
	DownloadBundles = 6,
	Error = -1
}
