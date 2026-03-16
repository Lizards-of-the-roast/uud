using System.Collections.Generic;
using Newtonsoft.Json;

namespace AssetLookupTree.TreeLoading.SerializationPatterns;

public class SingleFileTreeSerializationPattern : ITreeSerializationPattern
{
	public IEnumerable<(string suffix, string content)> SerializeTree<T>(AssetLookupTree<T> tree) where T : class, IPayload
	{
		yield return (suffix: string.Empty, content: JsonConvert.SerializeObject(tree, AssetLookupTreeUtils.DefaultJsonSettings<T>()));
	}
}
