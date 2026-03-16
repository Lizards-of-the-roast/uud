using System.IO;

namespace AssetLookupTree.TreeLoading.PathPatterns;

public class TempTreePathPattern : ITreePathPattern
{
	private readonly string _tempRoot;

	public TempTreePathPattern(string tempRoot)
	{
		_tempRoot = tempRoot;
	}

	public string GetPath<T>() where T : class, IPayload
	{
		return Path.Combine(_tempRoot, "ALT_" + AssetLookupTreeUtils.GetPayloadTypeName<T>() + ".mtga").Replace("\\", "/");
	}
}
