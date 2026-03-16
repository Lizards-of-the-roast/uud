namespace Wizards.Mtga.Assets;

public interface IAssetBundleDownloadQueue
{
	long RemainingBytes { get; }

	int Count { get; }

	bool TryDequeuePendingDownload(out AssetFileInfo? bundleInfo);

	void RequeuePendingDownload(AssetFileInfo bundleInfo);
}
