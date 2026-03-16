using System.Collections.Generic;
using System.IO;
using AssetLookupTree.TreeLoading.DeserializationPatterns;

namespace AssetLookupTree.TreeLoading.LoadPatterns;

public class AssetLoaderTreeLoadPattern : ITreeLoadPattern
{
	private readonly ITreeDeserializationPattern _deserializationPattern;

	public AssetLoaderTreeLoadPattern(ITreeDeserializationPattern deserializationPattern)
	{
		_deserializationPattern = deserializationPattern ?? new NullTreeDeserializationPattern();
	}

	public AssetLookupTree<T> LoadTree<T>() where T : class, IPayload
	{
		Dictionary<string, Stream> dictionary = new Dictionary<string, Stream>(1) { [string.Empty] = AssetLoader.GetTree<T>() };
		try
		{
			_deserializationPattern.TryDeserializeTree((IReadOnlyDictionary<string, Stream>)dictionary, out AssetLookupTree<T> tree);
			return tree;
		}
		finally
		{
			foreach (KeyValuePair<string, Stream> item in dictionary)
			{
				item.Value.Close();
				item.Value.Dispose();
			}
		}
	}
}
