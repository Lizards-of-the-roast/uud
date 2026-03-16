using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.Profiling;
using UnityEngine;

namespace AssetLookupTree.TreeLoading.DeserializationPatterns;

public class PackedTreeDeserializerPattern : ITreeDeserializationPattern
{
	private static readonly ProfilerMarker s_ProfilerMarker = new ProfilerMarker(TreeProfiling.Category, TreeProfiling.PrefixDeserialize + ".Packed");

	public bool TryDeserializeTree<T>(IReadOnlyDictionary<string, Stream> treeContent, out AssetLookupTree<T> tree) where T : class, IPayload
	{
		if (!treeContent.TryGetValue(string.Empty, out var value))
		{
			tree = null;
			return false;
		}
		using (s_ProfilerMarker.Auto())
		{
			tree = null;
			string text = "ALT_" + AssetLookupTreeUtils.GetPayloadTypeName(typeof(T));
			JsonSerializer jsonSerializer = JsonSerializer.Create(AssetLookupTreeUtils.DefaultJsonSettings<T>());
			Debug.Log(text);
			using (StreamReader reader = new StreamReader(value))
			{
				using JsonTextReader jsonTextReader = new JsonTextReader(reader);
				jsonTextReader.Read();
				while (jsonTextReader.Read())
				{
					if (jsonTextReader.TokenType == JsonToken.PropertyName && (string)jsonTextReader.Value == text)
					{
						jsonTextReader.Read();
						tree = jsonSerializer.Deserialize<AssetLookupTree<T>>(jsonTextReader);
						break;
					}
					jsonTextReader.Skip();
				}
			}
			return tree != null;
		}
	}
}
