using Wizards.Mtga.Assets;
using Wizards.Mtga.IO;
using Wizards.Mtga.Platforms;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_DeleteBundles : AutoPlayAction
{
	protected override void OnInitialize(in string[] parameters, int index)
	{
		string assetBundleStoragePath = PlatformContext.GetStorageContext().GetAssetBundleStoragePath();
		if (WindowsSafePath.DirectoryExists(assetBundleStoragePath))
		{
			WindowsSafePath.DeleteDirectory(assetBundleStoragePath, recursive: true);
		}
	}

	protected override void OnExecute()
	{
		Complete("Deleted bundles");
	}
}
