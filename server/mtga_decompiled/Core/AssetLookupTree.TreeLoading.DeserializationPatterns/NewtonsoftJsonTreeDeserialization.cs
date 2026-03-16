using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Wizards.Mtga.Logging;

namespace AssetLookupTree.TreeLoading.DeserializationPatterns;

public class NewtonsoftJsonTreeDeserialization
{
	private readonly JsonSerializer _jsonSerializer;

	public NewtonsoftJsonTreeDeserialization()
	{
		JsonSerializerSettings jsonSerializerSettings = AssetLookupTreeUtils.DefaultJsonSettings();
		MethodInfo method = typeof(AssetLookupTreeUtils).GetMethod("AddConverterIfNecessary");
		foreach (Type allPayloadType in AssetLookupTreeUtils.GetAllPayloadTypes())
		{
			MethodInfo methodInfo = method.MakeGenericMethod(allPayloadType);
			if ((object)methodInfo != null)
			{
				object[] parameters = new IList<JsonConverter>[1] { jsonSerializerSettings.Converters };
				methodInfo.Invoke(null, parameters);
			}
		}
		_jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);
	}

	public IEnumerable<IAssetLookupTree> DeserializeTrees(Stream treeContentStream, IReadOnlyDictionary<string, Type> altPayloadNameToType, UnityLogger logger)
	{
		using StreamReader streamReader = new StreamReader(treeContentStream);
		using JsonTextReader jsonReader = new JsonTextReader(streamReader);
		jsonReader.Read();
		while (jsonReader.Read())
		{
			if (jsonReader.TokenType != JsonToken.PropertyName)
			{
				jsonReader.Skip();
				continue;
			}
			string text = (string)jsonReader.Value;
			jsonReader.Read();
			if (text == null || !altPayloadNameToType.TryGetValue(text, out var value))
			{
				logger.LogError("Bundle contains an unknown ALT " + text);
				jsonReader.Skip();
				continue;
			}
			IAssetLookupTree assetLookupTree;
			try
			{
				assetLookupTree = (IAssetLookupTree)typeof(JsonSerializer).GetMethods().First((MethodInfo method) => method.Name == "Deserialize" && method.IsGenericMethod).MakeGenericMethod(typeof(AssetLookupTree<>).MakeGenericType(value))
					.Invoke(_jsonSerializer, new object[1] { jsonReader });
			}
			catch (Exception arg)
			{
				logger.LogError($"Exception deserializing payload {text} into tree: {arg}");
				throw;
			}
			yield return assetLookupTree;
		}
	}
}
