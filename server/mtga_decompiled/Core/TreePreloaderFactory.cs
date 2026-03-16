using AssetLookupTree.TreeLoading.PreloadPatterns;

public static class TreePreloaderFactory
{
	public static ITreePreloader Create()
	{
		return new BundlePreloader();
	}
}
