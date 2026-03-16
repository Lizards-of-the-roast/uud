using System.Collections.Generic;
using System.IO;

namespace AssetLookupTree.TreeLoading.DeserializationPatterns;

public interface ITreeDeserializationPattern
{
	bool TryDeserializeTree<T>(IReadOnlyDictionary<string, Stream> treeContent, out AssetLookupTree<T> tree) where T : class, IPayload;
}
