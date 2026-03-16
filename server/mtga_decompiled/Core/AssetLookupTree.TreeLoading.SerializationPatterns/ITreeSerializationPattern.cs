using System.Collections.Generic;

namespace AssetLookupTree.TreeLoading.SerializationPatterns;

public interface ITreeSerializationPattern
{
	IEnumerable<(string suffix, string content)> SerializeTree<T>(AssetLookupTree<T> tree) where T : class, IPayload;
}
