using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AssetLookupTree;
using AssetLookupTree.Evaluators;
using AssetLookupTree.Extractors;
using AssetLookupTree.Indirectors;
using AssetLookupTree.Nodes;
using Wizards.Mtga.Assets;

namespace Newtonsoft.Json.Converters;

public class AltConverter<T> : JsonConverter<AssetLookupTree<T>> where T : class, IPayload
{
	public override void WriteJson(JsonWriter writer, AssetLookupTree<T> value, JsonSerializer serializer)
	{
		Dictionary<Guid, INode<T>> dictionary = new Dictionary<Guid, INode<T>>(100);
		foreach (INode<T> item in value.EnumerateNodes())
		{
			dictionary[item.NodeId] = item;
		}
		IReadOnlyList<INode<T>> readOnlyList = dictionary.Values.OrderBy((INode<T> x) => x.NodeId).ToList();
		writer.WriteStartObject();
		writer.WritePropertyName("Root");
		writer.WriteValue(value.Root?.NodeId);
		writer.WritePropertyName("Priority");
		writer.WriteValue((int)value.Priority);
		writer.WritePropertyName("DefaultPayloadPriority");
		writer.WriteValue((int)value.DefaultPayloadPriority);
		writer.WritePropertyName("AssetsPerBundle");
		writer.WriteValue((int)value.AssetsPerBundle);
		writer.WritePropertyName("MustReturnPayload");
		writer.WriteValue(value.MustReturnPayload);
		writer.WritePropertyName("Nodes");
		writer.WriteStartArray();
		foreach (INode<T> item2 in readOnlyList)
		{
			writer.WriteStartObject();
			SerializedNodeType nodeType = NodeSerializationUtils.GetNodeType(item2);
			if (nodeType != SerializedNodeType.Value)
			{
				writer.WritePropertyName("NodeType");
				writer.WriteValue(nodeType);
			}
			writer.WritePropertyName("NodeId");
			writer.WriteValue(item2.NodeId);
			if (!string.IsNullOrEmpty(item2.Comment))
			{
				writer.WritePropertyName("Comment");
				writer.WriteValue(item2.Comment);
			}
			if (!string.IsNullOrEmpty(item2.Label))
			{
				writer.WritePropertyName("Label");
				writer.WriteValue(item2.Label);
			}
			if (!(item2 is ValueNode<T> valueNode))
			{
				if (!(item2 is ErrorNode<T> errorNode))
				{
					if (!(item2 is OrganizationNode<T> organizationNode))
					{
						if (!(item2 is PriorityNode<T>))
						{
							if (!(item2 is ConditionNode<T> conditionNode))
							{
								if (!(item2 is BucketNode<T, int> bucketNode))
								{
									if (!(item2 is BucketNode<T, string> bucketNode2))
									{
										if (item2 is IndirectionNode<T> indirectionNode)
										{
											writer.WritePropertyName("IndirectorType");
											writer.WriteValue(indirectionNode.Indirector?.GetType().FullName);
											writer.WritePropertyName("Indirector");
											serializer.Serialize(writer, indirectionNode.Indirector);
										}
									}
									else
									{
										writer.WritePropertyName("ExtractorType");
										writer.WriteValue(bucketNode2.Extractor?.GetType().FullName);
									}
								}
								else
								{
									writer.WritePropertyName("ExtractorType");
									writer.WriteValue(bucketNode.Extractor?.GetType().FullName);
								}
							}
							else
							{
								writer.WritePropertyName("EvaluatorType");
								writer.WriteValue(conditionNode.Evaluator?.GetType().FullName);
								writer.WritePropertyName("Evaluator");
								serializer.Serialize(writer, conditionNode.Evaluator);
							}
						}
					}
					else
					{
						writer.WritePropertyName("SerializeIndependently");
						writer.WriteValue(organizationNode.SerializeIndependently);
					}
				}
				else
				{
					writer.WritePropertyName("ErrorMessage");
					writer.WriteValue(errorNode.ErrorMessage);
				}
			}
			else
			{
				if (valueNode.Priority != AssetPriority.Automatic)
				{
					writer.WritePropertyName("Priority");
					writer.WriteValue((int)valueNode.Priority);
				}
				writer.WritePropertyName("Payload");
				serializer.Serialize(writer, valueNode.Payload);
			}
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WritePropertyName("Connections");
		writer.WriteStartArray();
		foreach (INode<T> item3 in readOnlyList)
		{
			if (item3 is ValueNode<T>)
			{
				continue;
			}
			if (!(item3 is INodeWithChild<T> nodeWithChild))
			{
				if (!(item3 is PriorityNode<T> priorityNode))
				{
					if (!(item3 is BucketNode<T, int> bucketNode3))
					{
						if (!(item3 is BucketNode<T, string> bucketNode4) || bucketNode4.Children.Count <= 0)
						{
							continue;
						}
						writer.WriteStartObject();
						writer.WritePropertyName("Parent");
						writer.WriteValue(bucketNode4.NodeId);
						writer.WritePropertyName("Child");
						writer.WriteStartObject();
						foreach (KeyValuePair<string, INode<T>> child in bucketNode4.Children)
						{
							if (child.Value != null)
							{
								writer.WritePropertyName(child.Key);
								writer.WriteValue(child.Value.NodeId);
							}
						}
						writer.WriteEndObject();
						writer.WriteEndObject();
					}
					else
					{
						if (bucketNode3.Children.Count <= 0)
						{
							continue;
						}
						writer.WriteStartObject();
						writer.WritePropertyName("Parent");
						writer.WriteValue(bucketNode3.NodeId);
						writer.WritePropertyName("Child");
						writer.WriteStartObject();
						foreach (KeyValuePair<int, INode<T>> child2 in bucketNode3.Children)
						{
							if (child2.Value != null)
							{
								writer.WritePropertyName(child2.Key.ToString());
								writer.WriteValue(child2.Value.NodeId);
							}
						}
						writer.WriteEndObject();
						writer.WriteEndObject();
					}
				}
				else
				{
					if (priorityNode.Children.Count <= 0)
					{
						continue;
					}
					writer.WriteStartObject();
					writer.WritePropertyName("Parent");
					writer.WriteValue(priorityNode.NodeId);
					writer.WritePropertyName("Child");
					writer.WriteStartArray();
					foreach (INode<T> child3 in priorityNode.Children)
					{
						if (child3 != null)
						{
							writer.WriteValue(child3.NodeId);
						}
					}
					writer.WriteEndArray();
					writer.WriteEndObject();
				}
			}
			else if (nodeWithChild.Child != null)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("Parent");
				writer.WriteValue(nodeWithChild.NodeId);
				writer.WritePropertyName("Child");
				writer.WriteValue(nodeWithChild.Child.NodeId);
				writer.WriteEndObject();
			}
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
	}

	public override AssetLookupTree<T> ReadJson(JsonReader reader, Type objectType, AssetLookupTree<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (objectType != typeof(AssetLookupTree<T>))
		{
			throw new ArgumentException("Invalid parameter.", "objectType");
		}
		Assembly assembly = Assembly.GetAssembly(typeof(AssetLookupTree<>));
		Dictionary<Guid, INode<T>> dictionary = new Dictionary<Guid, INode<T>>(100);
		AssetLookupTree<T> assetLookupTree = new AssetLookupTree<T>();
		reader.Read();
		if (!Guid.TryParse(reader.ReadAsString(), out var result))
		{
			result = Guid.Empty;
		}
		reader.Read();
		assetLookupTree.Priority = (AssetPriority)reader.ReadAsInt32().Value;
		reader.Read();
		assetLookupTree.DefaultPayloadPriority = (AssetPriority)reader.ReadAsInt32().Value;
		reader.Read();
		if (reader.Path.EndsWith("AssetsPerBundle"))
		{
			assetLookupTree.AssetsPerBundle = (uint)reader.ReadAsInt32().Value;
			reader.Read();
		}
		assetLookupTree.MustReturnPayload = reader.ReadAsBoolean().Value;
		reader.Read();
		reader.Read();
		while (reader.Read() && reader.TokenType != JsonToken.EndArray)
		{
			INode<T> node = null;
			SerializedNodeType serializedNodeType = SerializedNodeType.Value;
			Guid guid = default(Guid);
			string comment = string.Empty;
			string label = string.Empty;
			T payload = null;
			IExtractor<int> extractor = null;
			IExtractor<string> extractor2 = null;
			IEvaluator evaluator = null;
			IIndirector indirector = null;
			string text = null;
			string text2 = null;
			string errorMessage = null;
			bool serializeIndependently = false;
			AssetPriority priority = AssetPriority.Automatic;
			while (reader.Read() && reader.TokenType != JsonToken.EndObject)
			{
				switch (reader.Value as string)
				{
				case "SerializedNodeType":
				case "NodeType":
					serializedNodeType = (SerializedNodeType)reader.ReadAsInt32().Value;
					break;
				case "NodeId":
					guid = Guid.Parse(reader.ReadAsString());
					break;
				case "Comment":
					comment = reader.ReadAsString();
					break;
				case "Label":
					label = reader.ReadAsString();
					break;
				case "Payload":
					reader.Read();
					if (reader.TokenType != JsonToken.Null)
					{
						_ = reader.TokenType;
						payload = serializer.Deserialize<T>(reader);
					}
					break;
				case "Priority":
					priority = (AssetPriority)reader.ReadAsInt32().Value;
					break;
				case "ExtractorType":
				{
					string text3 = reader.ReadAsString();
					if (!string.IsNullOrWhiteSpace(text3))
					{
						switch (serializedNodeType)
						{
						case SerializedNodeType.IntBucket:
							extractor = (IExtractor<int>)assembly.GetType(text3).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
							break;
						case SerializedNodeType.StringBucket:
							extractor2 = (IExtractor<string>)assembly.GetType(text3).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
							break;
						default:
							throw new Exception("Reading extractor before reading extractor type");
						}
					}
					break;
				}
				case "EvaluatorType":
					text2 = reader.ReadAsString();
					break;
				case "Evaluator":
					if (text2 == null)
					{
						throw new Exception("Reading evaluator before reading evaluator type");
					}
					reader.Read();
					evaluator = (IEvaluator)serializer.Deserialize(reader, assembly.GetType(text2));
					break;
				case "IndirectorType":
					text = reader.ReadAsString();
					break;
				case "Indirector":
					if (text == null)
					{
						throw new Exception("Reading indirector before reading indirector type");
					}
					reader.Read();
					indirector = (IIndirector)serializer.Deserialize(reader, assembly.GetType(text));
					break;
				case "ErrorMessage":
					errorMessage = reader.ReadAsString();
					break;
				case "SerializeIndependently":
					serializeIndependently = reader.ReadAsBoolean().Value;
					break;
				default:
					throw new Exception($"Unexpected key reading JSON: {reader.Value} in node {guid}");
				}
			}
			dictionary[guid] = serializedNodeType switch
			{
				SerializedNodeType.Value => new ValueNode<T>
				{
					NodeId = guid,
					Label = label,
					Comment = comment,
					Priority = priority,
					Payload = payload
				}, 
				SerializedNodeType.Priority => new PriorityNode<T>
				{
					NodeId = guid,
					Label = label,
					Comment = comment
				}, 
				SerializedNodeType.Condition => new ConditionNode<T>
				{
					NodeId = guid,
					Label = label,
					Comment = comment,
					Evaluator = evaluator
				}, 
				SerializedNodeType.IntBucket => new BucketNode<T, int>
				{
					NodeId = guid,
					Label = label,
					Comment = comment,
					Extractor = extractor
				}, 
				SerializedNodeType.StringBucket => new BucketNode<T, string>
				{
					NodeId = guid,
					Label = label,
					Comment = comment,
					Extractor = extractor2
				}, 
				SerializedNodeType.Indirection => new IndirectionNode<T>
				{
					NodeId = guid,
					Label = label,
					Comment = comment,
					Indirector = indirector
				}, 
				SerializedNodeType.Error => new ErrorNode<T>
				{
					NodeId = guid,
					Label = label,
					Comment = comment,
					ErrorMessage = errorMessage
				}, 
				SerializedNodeType.Organization => new OrganizationNode<T>
				{
					NodeId = guid,
					Label = label,
					Comment = comment,
					SerializeIndependently = serializeIndependently
				}, 
				_ => throw new ArgumentException($"Unhandled NodeType {serializedNodeType} found in JSON with id {guid}!"), 
			};
		}
		reader.Read();
		reader.Read();
		while (reader.Read() && reader.TokenType != JsonToken.EndArray)
		{
			reader.Read();
			string text4 = reader.ReadAsString();
			if (!Guid.TryParse(text4, out var result2))
			{
				throw new Exception("Could not parse Guid " + text4);
			}
			reader.Read();
			if (!dictionary.TryGetValue(result2, out var value))
			{
				throw new ArgumentException($"Connection object found with invalid Parent: {result2}");
			}
			if (!(value is ValueNode<T>))
			{
				if (!(value is INodeWithChild<T> nodeWithChild))
				{
					if (!(value is PriorityNode<T> priorityNode))
					{
						if (!(value is BucketNode<T, int> bucketNode))
						{
							if (value is BucketNode<T, string> bucketNode2)
							{
								reader.Read();
								while (reader.Read() && reader.TokenType != JsonToken.EndObject)
								{
									string key = (string)reader.Value;
									if (!Guid.TryParse(reader.ReadAsString(), out var result3))
									{
										throw new Exception("Could not parse StringBucketNode child guid {}");
									}
									if (dictionary.TryGetValue(result3, out var value2))
									{
										if (!bucketNode2.Children.ContainsKey(key))
										{
											bucketNode2.Children.Add(key, value2);
										}
										continue;
									}
									throw new ArgumentException($"Connection object Child ({result3}) was not parsed from the Nodes table!");
								}
							}
						}
						else
						{
							reader.Read();
							while (reader.Read() && reader.TokenType != JsonToken.EndObject)
							{
								int key2 = int.Parse((string)reader.Value, NumberFormatInfo.InvariantInfo);
								Guid guid2 = Guid.Parse(reader.ReadAsString());
								if (dictionary.TryGetValue(guid2, out var value3))
								{
									if (!bucketNode.Children.ContainsKey(key2))
									{
										bucketNode.Children.Add(key2, value3);
									}
									continue;
								}
								throw new ArgumentException($"Connection object Child ({guid2}) was not parsed from the Nodes table!");
							}
						}
					}
					else
					{
						reader.Read();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							Guid guid3 = Guid.Parse((string)reader.Value);
							if (dictionary.TryGetValue(guid3, out var value4))
							{
								priorityNode.Children.Add(value4);
								continue;
							}
							throw new ArgumentException($"Connection object Child ({guid3}) was not parsed from the Nodes table!");
						}
					}
				}
				else
				{
					Guid guid4 = Guid.Parse(reader.ReadAsString());
					if (!dictionary.TryGetValue(guid4, out var value5))
					{
						throw new ArgumentException($"Connection object Child ({guid4}) was not parsed from the Nodes table!");
					}
					nodeWithChild.Child = value5;
				}
				reader.Read();
				continue;
			}
			throw new ArgumentException("Connection object found with a Parent of type ValueNode!");
		}
		reader.Read();
		if (result != Guid.Empty)
		{
			if (!dictionary.TryGetValue(result, out var value6))
			{
				throw new ArgumentException("Root Guid not found in the Nodes table!");
			}
			assetLookupTree.Root = value6;
		}
		return assetLookupTree;
	}
}
