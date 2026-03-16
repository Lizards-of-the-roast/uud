using System.Collections.Generic;

namespace AssetLookupTree.TreeLoading.SerializationPatterns;

public class NullTreeSerializationPattern : ITreeSerializationPattern
{
	public IEnumerable<(string, string)> SerializeTree<T>(AssetLookupTree<T> tree) where T : class, IPayload
	{
		yield return (string.Empty, string.Empty);
	}
}
