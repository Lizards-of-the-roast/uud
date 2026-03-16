namespace AssetLookupTree.TreeLoading.LoadPatterns;

public class CompositeEditorLoadTreePattern : ITreeLoadPattern
{
	private readonly AssetLoaderTreeLoadPattern _assetBundleLoaderPattern;

	private readonly EditorTreeLoadPattern _editorLoaderPattern;

	public CompositeEditorLoadTreePattern(AssetLoaderTreeLoadPattern assetBundleLoaderPattern, EditorTreeLoadPattern editorLoaderPattern)
	{
		_assetBundleLoaderPattern = assetBundleLoaderPattern;
		_editorLoaderPattern = editorLoaderPattern;
	}

	public AssetLookupTree<T> LoadTree<T>() where T : class, IPayload
	{
		if (AssetBundleManager.Instance.GetAltPath(typeof(T)) != null)
		{
			return _assetBundleLoaderPattern.LoadTree<T>();
		}
		return _editorLoaderPattern.LoadTree<T>();
	}
}
