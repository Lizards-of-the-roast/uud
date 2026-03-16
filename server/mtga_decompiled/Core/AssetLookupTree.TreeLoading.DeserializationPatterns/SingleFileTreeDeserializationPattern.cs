using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace AssetLookupTree.TreeLoading.DeserializationPatterns;

public class SingleFileTreeDeserializationPattern : ITreeDeserializationPattern
{
	public bool TryDeserializeTree<T>(IReadOnlyDictionary<string, Stream> treeContent, out AssetLookupTree<T> tree) where T : class, IPayload
	{
		if (!treeContent.TryGetValue(string.Empty, out var value))
		{
			tree = null;
			return false;
		}
		JsonSerializer jsonSerializer = JsonSerializer.Create(AssetLookupTreeUtils.DefaultJsonSettings<T>());
		using (StreamReader reader = new StreamReader(value))
		{
			using JsonTextReader reader2 = new JsonTextReader(reader);
			tree = jsonSerializer.Deserialize<AssetLookupTree<T>>(reader2);
		}
		return tree != null;
	}
}
