using System.Collections.Generic;
using System.IO;
using AssetLookupTree.TreeLoading.DeserializationPatterns;
using AssetLookupTree.TreeLoading.PathPatterns;

namespace AssetLookupTree.TreeLoading.LoadPatterns;

public class EditorTreeLoadPattern : ITreeLoadPattern
{
	private readonly ITreePathPattern _pathPattern;

	private readonly ITreeDeserializationPattern _deserializationPattern;

	public EditorTreeLoadPattern(ITreePathPattern pathPattern, ITreeDeserializationPattern deserializationPattern)
	{
		_pathPattern = pathPattern ?? new NullTreePathPattern();
		_deserializationPattern = deserializationPattern ?? new NullTreeDeserializationPattern();
	}

	public AssetLookupTree<T> LoadTree<T>() where T : class, IPayload
	{
		string path = _pathPattern.GetPath<T>();
		if (string.IsNullOrWhiteSpace(path))
		{
			return null;
		}
		if (!File.Exists(path))
		{
			return null;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
		{
			return null;
		}
		string directoryName = Path.GetDirectoryName(path);
		if (string.IsNullOrWhiteSpace(directoryName))
		{
			return null;
		}
		if (!Directory.Exists(directoryName))
		{
			return null;
		}
		Dictionary<string, Stream> dictionary = new Dictionary<string, Stream>(10);
		foreach (string item in (IEnumerable<string>)Directory.GetFiles(directoryName, fileNameWithoutExtension + "*.json", SearchOption.TopDirectoryOnly))
		{
			string key = Path.GetFileNameWithoutExtension(item).Remove(0, fileNameWithoutExtension.Length).TrimStart('_');
			dictionary[key] = FileSystemUtils.OpenRead(item);
		}
		try
		{
			_deserializationPattern.TryDeserializeTree((IReadOnlyDictionary<string, Stream>)dictionary, out AssetLookupTree<T> tree);
			return tree;
		}
		finally
		{
			foreach (KeyValuePair<string, Stream> item2 in dictionary)
			{
				item2.Value.Close();
				item2.Value.Dispose();
			}
		}
	}
}
