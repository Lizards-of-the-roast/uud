using System.Collections.Generic;
using System.IO;

namespace AssetLookupTree.TreeLoading.DeserializationPatterns;

public class NullTreeDeserializationPattern : ITreeDeserializationPattern
{
	public bool TryDeserializeTree<T>(IReadOnlyDictionary<string, Stream> treeContent, out AssetLookupTree<T> tree) where T : class, IPayload
	{
		tree = null;
		return false;
	}
}
