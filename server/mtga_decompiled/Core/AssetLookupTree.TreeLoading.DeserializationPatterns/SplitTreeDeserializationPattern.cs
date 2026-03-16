using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AssetLookupTree.Evaluators;
using AssetLookupTree.Extractors;
using AssetLookupTree.Indirectors;
using AssetLookupTree.Nodes;
using Core.Code.AssetLookupTree.AssetLookup.TreeLoading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using Wizards.Mtga.Assets;

namespace AssetLookupTree.TreeLoading.DeserializationPatterns;

public class SplitTreeDeserializationPattern : ITreeDeserializationPattern
{
	private readonly bool _createOrphanNode;

	public SplitTreeDeserializationPattern(bool createOrphanNode = true)
	{
		_createOrphanNode = createOrphanNode;
	}

	public bool TryDeserializeTree<T>(IReadOnlyDictionary<string, Stream> treeContent, out AssetLookupTree<T> tree) where T : class, IPayload
	{
		if (!treeContent.TryGetValue(string.Empty, out var value))
		{
			throw new ArgumentException("[" + typeof(T).FullName + "] Missing necessary tree definition file stream!", "treeContent");
		}
		JsonSerializer serializer = JsonSerializer.Create(AssetLookupTreeUtils.DefaultJsonSettings<T>());
		List<string> list = new List<string>();
		Dictionary<Guid, INode<T>> dictionary = new Dictionary<Guid, INode<T>>(1000);
		Guid? guid = null;
		AssetPriority priority = AssetPriority.Automatic;
		AssetPriority defaultPayloadPriority = AssetPriority.Automatic;
		uint assetsPerBundle = 0u;
		bool mustReturnPayload = false;
		List<string> list2 = new List<string>(8);
		List<string> list3 = new List<string>(1);
		try
		{
			using (StreamReader streamReader = new StreamReader(value))
			{
				TreeDescription treeDescription = JsonConvert.DeserializeObject<TreeDescription>(streamReader.ReadToEnd());
				guid = ((treeDescription.Root == null) ? ((Guid?)null) : new Guid?(Guid.Parse(treeDescription.Root)));
				priority = treeDescription.Priority;
				defaultPayloadPriority = treeDescription.DefaultPayloadPriority;
				assetsPerBundle = treeDescription.AssetsPerBundle;
				mustReturnPayload = treeDescription.MustReturnPayload;
				list2 = treeDescription.Partitions;
				list3 = treeDescription.Trees;
			}
			foreach (string item in list2)
			{
				int num = 0;
				using StreamReader reader = new StreamReader(treeContent[item]);
				using JsonTextReader jsonTextReader = new JsonTextReader(reader);
				jsonTextReader.Read();
				jsonTextReader.Read();
				if (object.Equals(jsonTextReader.Value, "NodeCountInPartition"))
				{
					_ = jsonTextReader.ReadAsInt32().Value;
					jsonTextReader.Read();
					Guid.Parse(jsonTextReader.ReadAsString());
					jsonTextReader.Read();
					Guid.Parse(jsonTextReader.ReadAsString());
					jsonTextReader.Read();
				}
				jsonTextReader.Read();
				while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.PropertyName)
				{
					INode<T> node = ReadNode<T>(jsonTextReader, serializer);
					num++;
					dictionary[node.NodeId] = node;
				}
				jsonTextReader.Read();
				jsonTextReader.Read();
			}
			if (guid.HasValue)
			{
				if (!dictionary.ContainsKey(guid.Value))
				{
					list.Add($"[{typeof(T).FullName}] Missing root node {guid}!");
				}
			}
			else if (dictionary.Count != 0)
			{
				list.Add("[" + typeof(T).FullName + "] Null root, but non-zero nodes. This likely means there was some data loss!");
			}
			foreach (string item2 in list3)
			{
				if (treeContent.TryGetValue(item2, out var value2))
				{
					using StreamReader reader2 = new StreamReader(value2);
					using JsonTextReader jsonTextReader2 = new JsonTextReader(reader2);
					int num2 = 0;
					jsonTextReader2.Read();
					jsonTextReader2.Read();
					jsonTextReader2.Read();
					while (jsonTextReader2.Read() && jsonTextReader2.TokenType == JsonToken.PropertyName)
					{
						ReadConnection(jsonTextReader2, dictionary, list);
						num2++;
					}
					jsonTextReader2.Read();
				}
				else
				{
					list.Add("[" + typeof(T).FullName + " tree " + item2 + " not found.");
				}
			}
			List<Guid> list4 = new List<Guid>();
			HashSet<Guid> hashSet = new HashSet<Guid>();
			if (dictionary.Keys.Count > 1)
			{
				HashSet<Guid> hashSet2 = new HashSet<Guid>();
				if (guid.HasValue)
				{
					foreach (INode<T> item3 in dictionary[guid.Value].EnumerateNodes())
					{
						hashSet2.Add(item3.NodeId);
					}
				}
				foreach (Guid key in dictionary.Keys)
				{
					if (hashSet2.Contains(key) || hashSet.Contains(key))
					{
						continue;
					}
					list4.Add(key);
					if (!dictionary[key].IsParentNode(out var children))
					{
						continue;
					}
					foreach (INode<T> item4 in children)
					{
						RecursivelyIgnore(item4, hashSet);
					}
				}
			}
			if (list4.Count > 0)
			{
				string text = "Nodes discovered with no parents in tree: [" + string.Join(", ", list4) + "]";
				if (_createOrphanNode)
				{
					Debug.LogWarning(text);
					PriorityNode<T> priorityNode = new PriorityNode<T>
					{
						NodeId = Guid.NewGuid(),
						Comment = "Orphaned nodes detected during deserialization",
						Label = "[ERROR] Orphans"
					};
					foreach (Guid item5 in list4)
					{
						priorityNode.Children.Add(dictionary[item5]);
					}
					if (!(dictionary[guid.Value] is PriorityNode<T>))
					{
						PriorityNode<T> priorityNode2 = new PriorityNode<T>
						{
							NodeId = Guid.NewGuid(),
							Label = "Priority Root"
						};
						priorityNode2.Children.Add(dictionary[guid.Value]);
						guid = priorityNode2.NodeId;
						dictionary.Add(guid.Value, priorityNode2);
					}
					(dictionary[guid.Value] as PriorityNode<T>).Children.Add(priorityNode);
					dictionary.Add(priorityNode.NodeId, priorityNode);
				}
				else
				{
					list.Add(text);
				}
			}
		}
		catch (Exception arg)
		{
			list.Add($"[{typeof(T).FullName}] Unhandled exception during deserialization!\n{arg}");
		}
		if (list.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Empty);
			foreach (string item6 in list)
			{
				stringBuilder.AppendLine(item6);
			}
			throw new FileLoadException(stringBuilder.ToString());
		}
		tree = new AssetLookupTree<T>
		{
			Root = (guid.HasValue ? dictionary[guid.Value] : null),
			DefaultPayloadPriority = defaultPayloadPriority,
			AssetsPerBundle = assetsPerBundle,
			MustReturnPayload = mustReturnPayload,
			Priority = priority
		};
		return true;
		static void RecursivelyIgnore(INode<T> child, HashSet<Guid> ignoredChildren)
		{
			if (child.IsParentNode(out var children2))
			{
				foreach (INode<T> item7 in children2)
				{
					if (item7 != null)
					{
						ignoredChildren.Add(item7.NodeId);
						RecursivelyIgnore(item7, ignoredChildren);
					}
				}
				ignoredChildren.Add(child.NodeId);
			}
		}
	}

	private static INode<T> ReadNode<T>(JsonReader reader, JsonSerializer serializer) where T : class, IPayload
	{
		Assembly assembly = Assembly.GetAssembly(typeof(AssetLookupTree<>));
		Guid guid = Guid.Parse((string)reader.Value);
		reader.Read();
		reader.Read();
		SerializedNodeType serializedNodeType = SerializedNodeType.Value;
		if ((string)reader.Value == "NodeType")
		{
			serializedNodeType = (SerializedNodeType)reader.ReadAsInt32().Value;
			reader.Read();
		}
		string comment = string.Empty;
		if ((string)reader.Value == "Comment")
		{
			comment = reader.ReadAsString();
			reader.Read();
		}
		string label = string.Empty;
		if ((string)reader.Value == "Label")
		{
			label = reader.ReadAsString();
			reader.Read();
		}
		List<string> list = null;
		if ((string)reader.Value == "Tests")
		{
			reader.Read();
			reader.Read();
			list = new List<string>();
			while (reader.TokenType != JsonToken.EndArray)
			{
				list.Add((string)reader.Value);
				reader.Read();
			}
			reader.Read();
		}
		switch (serializedNodeType)
		{
		case SerializedNodeType.Value:
		{
			AssetPriority priority = AssetPriority.Automatic;
			if ((string)reader.Value == "Priority")
			{
				priority = (AssetPriority)reader.ReadAsInt32().Value;
				reader.Read();
			}
			reader.Read();
			T payload;
			try
			{
				payload = serializer.Deserialize<T>(reader);
			}
			catch (Exception innerException)
			{
				throw new Exception($"Couldn't deserialize node {guid} value {reader.Value}", innerException);
			}
			reader.Read();
			return new ValueNode<T>
			{
				NodeId = guid,
				Comment = comment,
				Label = label,
				Priority = priority,
				Payload = payload
			};
		}
		case SerializedNodeType.Priority:
			return new PriorityNode<T>
			{
				NodeId = guid,
				Comment = comment,
				Label = label
			};
		case SerializedNodeType.Condition:
		{
			string text3 = reader.ReadAsString();
			reader.Read();
			reader.Read();
			IEvaluator evaluator = null;
			if (!string.IsNullOrWhiteSpace(text3))
			{
				Type type3 = assembly.GetType(text3);
				if (type3 == null)
				{
					throw new TypeLoadException("Could not find Evaluator type of \"" + text3 + "\"");
				}
				evaluator = (IEvaluator)serializer.Deserialize(reader, type3);
			}
			reader.Read();
			return new ConditionNode<T>
			{
				NodeId = guid,
				Comment = comment,
				Label = label,
				Evaluator = evaluator
			};
		}
		case SerializedNodeType.IntBucket:
		{
			string text2 = reader.ReadAsString();
			reader.Read();
			IExtractor<int> extractor = null;
			if (!string.IsNullOrWhiteSpace(text2))
			{
				Type type2 = assembly.GetType(text2);
				if (type2 == null)
				{
					throw new TypeLoadException("Could not find Extractor type of \"" + text2 + "\"");
				}
				extractor = (IExtractor<int>)type2.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
			}
			return new BucketNode<T, int>
			{
				NodeId = guid,
				Comment = comment,
				Label = label,
				Extractor = extractor
			};
		}
		case SerializedNodeType.StringBucket:
		{
			string text4 = reader.ReadAsString();
			reader.Read();
			IExtractor<string> extractor2 = null;
			if (!string.IsNullOrWhiteSpace(text4))
			{
				Type type4 = assembly.GetType(text4);
				if (type4 == null)
				{
					throw new TypeLoadException("Could not find Extractor type of \"" + text4 + "\"");
				}
				extractor2 = (IExtractor<string>)type4.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
			}
			return new BucketNode<T, string>
			{
				NodeId = guid,
				Comment = comment,
				Label = label,
				Extractor = extractor2
			};
		}
		case SerializedNodeType.Indirection:
		{
			string text = reader.ReadAsString();
			reader.Read();
			reader.Read();
			IIndirector indirector = null;
			if (!string.IsNullOrWhiteSpace(text))
			{
				Type type = assembly.GetType(text);
				if (type == null)
				{
					throw new TypeLoadException("Could not find Indirector type of \"" + text + "\"");
				}
				indirector = (IIndirector)serializer.Deserialize(reader, type);
			}
			reader.Read();
			return new IndirectionNode<T>
			{
				NodeId = guid,
				Comment = comment,
				Label = label,
				Indirector = indirector
			};
		}
		case SerializedNodeType.Error:
		{
			string errorMessage = reader.ReadAsString();
			reader.Read();
			return new ErrorNode<T>
			{
				NodeId = guid,
				Comment = comment,
				Label = label,
				ErrorMessage = errorMessage
			};
		}
		case SerializedNodeType.Organization:
		{
			bool valueOrDefault = reader.ReadAsBoolean() == true;
			reader.Read();
			return new OrganizationNode<T>
			{
				NodeId = guid,
				Comment = comment,
				Label = label,
				SerializeIndependently = valueOrDefault
			};
		}
		default:
			throw new ArgumentOutOfRangeException($"Unknown serialized NodeType {serializedNodeType} with id {guid}");
		}
	}

	private static void MapChildNode<T>(INodeWithChild<T> cNode, IReadOnlyDictionary<Guid, INode<T>> nodeMapping, Guid childId) where T : class, IPayload
	{
		if (nodeMapping.ContainsKey(childId))
		{
			cNode.Child = nodeMapping[childId];
			return;
		}
		throw new Exception($"Child could not be found for {childId}");
	}

	private static void MapChildBucketNode<T, K>(BucketNode<T, K> bucketNode, IReadOnlyDictionary<Guid, INode<T>> nodeMapping, Guid childId, K childKey, Guid parentId) where T : class, IPayload
	{
		if (nodeMapping.ContainsKey(childId))
		{
			bucketNode.Children[childKey] = nodeMapping[childId];
			return;
		}
		throw new Exception($"Child could not be found for {childId} parent {parentId}");
	}

	private static void ReadConnection<T>(JsonReader reader, IReadOnlyDictionary<Guid, INode<T>> nodeMapping, List<string> errors) where T : class, IPayload
	{
		Guid guid = Guid.Parse((string)reader.Value);
		reader.Read();
		switch (reader.TokenType)
		{
		case JsonToken.String:
		{
			Guid guid2 = Guid.Parse((string)reader.Value);
			if (nodeMapping.ContainsKey(guid))
			{
				if (nodeMapping[guid] is INodeWithChild<T> cNode)
				{
					MapChildNode(cNode, nodeMapping, guid2);
				}
				else
				{
					errors.Add($"Node {guid2} should have parent {guid} but it was not parsed as a NodeWithChild");
				}
			}
			else
			{
				errors.Add($"Cannot find key for parent {guid} for child {guid2}");
			}
			break;
		}
		case JsonToken.StartArray:
		{
			if (nodeMapping.TryGetValue(guid, out var value2) && value2 is PriorityNode<T> priorityNode)
			{
				while (reader.Read() && reader.TokenType == JsonToken.String)
				{
					if (reader.Value is string input)
					{
						Guid guid3 = Guid.Parse(input);
						if (nodeMapping.TryGetValue(guid3, out var value3))
						{
							priorityNode.Children.Add(value3);
						}
						else
						{
							errors.Add($"{guid}'s child ID <{guid3}> is not in the known nodes for this tree.");
						}
						continue;
					}
					throw new Exception($"Read Value is not a string value: {reader.Value} parent {guid}");
				}
			}
			else
			{
				errors.Add($"Priority parent <{guid}> not in the known nodes for this tree");
			}
			break;
		}
		case JsonToken.StartObject:
		{
			if (nodeMapping.TryGetValue(guid, out var value))
			{
				if (!(value is BucketNode<T, int> bucketNode))
				{
					if (value is BucketNode<T, string> bucketNode2)
					{
						while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
						{
							string childKey = (string)reader.Value;
							reader.Read();
							Guid childId = Guid.Parse((string)reader.Value);
							MapChildBucketNode(bucketNode2, nodeMapping, childId, childKey, guid);
						}
					}
				}
				else
				{
					while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
					{
						int childKey2 = int.Parse((string)reader.Value);
						reader.Read();
						Guid childId2 = Guid.Parse((string)reader.Value);
						MapChildBucketNode(bucketNode, nodeMapping, childId2, childKey2, guid);
					}
				}
			}
			else
			{
				errors.Add($"Bucket parent <{guid}> not in the known nodes for this tree.");
			}
			break;
		}
		}
	}
}
