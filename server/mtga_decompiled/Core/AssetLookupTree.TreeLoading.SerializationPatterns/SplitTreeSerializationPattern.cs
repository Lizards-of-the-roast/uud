using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AssetLookupTree.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Wizards.Mtga.Assets;

namespace AssetLookupTree.TreeLoading.SerializationPatterns;

public class SplitTreeSerializationPattern : ITreeSerializationPattern
{
	public IEnumerable<(string suffix, string content)> SerializeTree<T>(AssetLookupTree<T> tree) where T : class, IPayload
	{
		JsonSerializer serializer = JsonSerializer.Create(AssetLookupTreeUtils.DefaultJsonSettings<T>());
		INode<T>[] array = (from x in tree.EnumerateNodes()
			orderby x.NodeId
			select x).ToArray();
		int num = array.Length;
		int num2 = array.Select((INode<T> x) => x.NodeId).Distinct().Count();
		if (num != num2)
		{
			throw new InvalidDataException("[" + typeof(T).FullName + "] Nodes list contained duplicate NodeId GUIDs, this will result in data loss so we are canceling the operation!");
		}
		INode<T>[] nestedTreeRoots = array.Where((INode<T> x) => (x as OrganizationNode<T>)?.SerializeIndependently ?? false).ToArray();
		Dictionary<INode<T>, INode<T>[]> nestedNodesByRoot = new Dictionary<INode<T>, INode<T>[]>(nestedTreeRoots.Length);
		INode<T>[] array2 = nestedTreeRoots;
		foreach (INode<T> node in array2)
		{
			if (!nestedNodesByRoot.ContainsKey(node))
			{
				ExtractNestedSubTree(node, nestedNodesByRoot);
			}
		}
		Dictionary<INode<T>, (string nodePart, string treePart)> nestedNodePartitionsByRoot = nestedTreeRoots.ToDictionary((INode<T> x) => x, (INode<T> x) => ("nodes_" + x.NodeId.ToString().Remove(8), "tree_" + x.NodeId.ToString().Remove(8)));
		INode<T>[] allNonNestedNodes = array.Except(nestedNodesByRoot.SelectMany((KeyValuePair<INode<T>, INode<T>[]> x) => x.Value)).ToArray();
		Dictionary<string, INode<T>[]> groupedNonNestedNodes = (from x in allNonNestedNodes
			group x by x.NodeId.ToString()[0]).ToDictionary((IGrouping<char, INode<T>> x) => $"nodes_{x.Key}", (IGrouping<char, INode<T>> x) => x.OrderBy((INode<T> y) => y.NodeId).ToArray());
		int num4 = groupedNonNestedNodes.Sum((KeyValuePair<string, INode<T>[]> x) => x.Value.Length) + nestedNodesByRoot.Sum((KeyValuePair<INode<T>, INode<T>[]> x) => x.Value.Length);
		if (num != num4)
		{
			throw new InvalidDataException($"[{typeof(T).FullName}] Grouped nodes did not contain all nodes (Found: {num4}, Expected: {num}), there may be a GUID duplication error in the source data! Canceling operation as continuing would result in data loss.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		IEnumerable<string> nodePartitionNames = from x in nestedNodePartitionsByRoot.Select((KeyValuePair<INode<T>, (string nodePart, string treePart)> x) => x.Value.nodePart).Concat(groupedNonNestedNodes.Keys)
			orderby x
			select x;
		IEnumerable<string> source = nestedNodePartitionsByRoot.Select((KeyValuePair<INode<T>, (string nodePart, string treePart)> x) => x.Value.treePart);
		if (allNonNestedNodes.Length != 0)
		{
			source = source.Append("tree");
		}
		source = source.OrderBy((string x) => x);
		yield return (suffix: string.Empty, content: WriteRootJson(tree, stringBuilder, num, nodePartitionNames, source));
		if (allNonNestedNodes.Length != 0)
		{
			yield return (suffix: "tree", content: WriteConnectionJson(stringBuilder, allNonNestedNodes));
		}
		INode<T>[] array3 = nestedTreeRoots;
		foreach (INode<T> key in array3)
		{
			yield return (suffix: nestedNodePartitionsByRoot[key].treePart, content: WriteConnectionJson(stringBuilder, nestedNodesByRoot[key]));
		}
		foreach (KeyValuePair<string, INode<T>[]> item in groupedNonNestedNodes)
		{
			yield return (suffix: item.Key, content: WriteNodesJson(stringBuilder, item.Value, serializer));
		}
		array3 = nestedTreeRoots;
		foreach (INode<T> key2 in array3)
		{
			yield return (suffix: nestedNodePartitionsByRoot[key2].nodePart, content: WriteNodesJson(stringBuilder, nestedNodesByRoot[key2], serializer));
		}
		static void ExtractNestedSubTree(INode<T> root, Dictionary<INode<T>, INode<T>[]> outputDict)
		{
			INode<T>[] array4 = (from x in root.EnumerateNodes().Skip(1)
				where (x as OrganizationNode<T>)?.SerializeIndependently ?? false
				select x).ToArray();
			if (array4.Length == 0)
			{
				outputDict[root] = (from x in root.EnumerateNodes().Skip(1)
					orderby x.NodeId
					select x).ToArray();
			}
			else
			{
				IEnumerable<INode<T>> enumerable = root.EnumerateNodes().Skip(1);
				INode<T>[] array5 = array4;
				foreach (INode<T> node2 in array5)
				{
					ExtractNestedSubTree(node2, outputDict);
					enumerable = enumerable.Except(outputDict[node2]);
				}
				outputDict[root] = enumerable.OrderBy((INode<T> x) => x.NodeId).ToArray();
			}
		}
	}

	private static string WriteRootJson<T>(AssetLookupTree<T> tree, StringBuilder stringBuilder, int totalNodeCount, IEnumerable<string> nodePartitionNames, IEnumerable<string> connectionPartitionNames) where T : class, IPayload
	{
		stringBuilder.Clear();
		using StringWriter textWriter = new StringWriter(stringBuilder);
		using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
		jsonWriter.Formatting = Formatting.Indented;
		jsonWriter.Culture = CultureInfo.InvariantCulture;
		jsonWriter.WriteStartObject();
		jsonWriter.WritePropertyName("Root");
		jsonWriter.WriteValue(tree.Root?.NodeId);
		jsonWriter.WritePropertyName("Priority");
		jsonWriter.WriteValue((int)tree.Priority);
		jsonWriter.WritePropertyName("DefaultPayloadPriority");
		jsonWriter.WriteValue((int)tree.DefaultPayloadPriority);
		jsonWriter.WritePropertyName("AssetsPerBundle");
		jsonWriter.WriteValue((int)tree.AssetsPerBundle);
		jsonWriter.WritePropertyName("MustReturnPayload");
		jsonWriter.WriteValue(tree.MustReturnPayload);
		jsonWriter.WritePropertyName("Partitions");
		jsonWriter.WriteStartArray();
		foreach (string nodePartitionName in nodePartitionNames)
		{
			jsonWriter.WriteValue(nodePartitionName);
		}
		jsonWriter.WriteEndArray();
		jsonWriter.WritePropertyName("Trees");
		jsonWriter.WriteStartArray();
		foreach (string connectionPartitionName in connectionPartitionNames)
		{
			jsonWriter.WriteValue(connectionPartitionName);
		}
		jsonWriter.WriteEndArray();
		jsonWriter.WriteEndObject();
		return stringBuilder.ToString();
	}

	private static string WriteConnectionJson<T>(StringBuilder stringBuilder, IReadOnlyList<INode<T>> allNodes) where T : class, IPayload
	{
		stringBuilder.Clear();
		using StringWriter textWriter = new StringWriter(stringBuilder);
		using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
		jsonWriter.Formatting = Formatting.Indented;
		jsonWriter.Culture = CultureInfo.InvariantCulture;
		jsonWriter.WriteStartObject();
		jsonWriter.WritePropertyName("Connections");
		jsonWriter.WriteStartObject();
		int num = 0;
		foreach (INode<T> allNode in allNodes)
		{
			if (WriteConnection(jsonWriter, allNode))
			{
				num++;
			}
		}
		jsonWriter.WriteEndObject();
		jsonWriter.WriteEndObject();
		return stringBuilder.ToString();
	}

	private static string WriteNodesJson<T>(StringBuilder stringBuilder, IReadOnlyList<INode<T>> nodesInPartition, JsonSerializer serializer) where T : class, IPayload
	{
		stringBuilder.Clear();
		using StringWriter textWriter = new StringWriter(stringBuilder);
		using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
		jsonWriter.Formatting = Formatting.Indented;
		jsonWriter.Culture = CultureInfo.InvariantCulture;
		jsonWriter.WriteStartObject();
		jsonWriter.WritePropertyName("Nodes");
		jsonWriter.WriteStartObject();
		foreach (INode<T> item in nodesInPartition)
		{
			WriteNode(jsonWriter, item, serializer);
		}
		jsonWriter.WriteEndObject();
		jsonWriter.WriteEndObject();
		return stringBuilder.ToString();
	}

	public static void WriteNode<T>(JsonWriter writer, INode<T> node, JsonSerializer serializer) where T : class, IPayload
	{
		writer.WritePropertyName(node.NodeId.ToString());
		writer.WriteStartObject();
		SerializedNodeType nodeType = NodeSerializationUtils.GetNodeType(node);
		if (nodeType != SerializedNodeType.Value)
		{
			writer.WritePropertyName("NodeType");
			writer.WriteValue(nodeType);
		}
		if (!string.IsNullOrEmpty(node.Comment))
		{
			writer.WritePropertyName("Comment");
			writer.WriteValue(node.Comment);
		}
		if (!string.IsNullOrEmpty(node.Label))
		{
			writer.WritePropertyName("Label");
			writer.WriteValue(node.Label);
		}
		if (!(node is ValueNode<T> valueNode))
		{
			if (!(node is OrganizationNode<T> organizationNode))
			{
				if (!(node is ErrorNode<T> errorNode))
				{
					if (!(node is ConditionNode<T> conditionNode))
					{
						if (!(node is BucketNode<T, int> bucketNode))
						{
							if (!(node is BucketNode<T, string> bucketNode2))
							{
								if (node is IndirectionNode<T> indirectionNode)
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
				else
				{
					writer.WritePropertyName("ErrorMessage");
					writer.WriteValue(errorNode.ErrorMessage);
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

	private static bool WriteConnection<T>(JsonWriter writer, INode<T> node) where T : class, IPayload
	{
		if (!(node is ErrorNode<T> errorNode))
		{
			if (!(node is OrganizationNode<T> organizationNode))
			{
				if (!(node is PriorityNode<T> priorityNode))
				{
					if (!(node is ConditionNode<T> conditionNode))
					{
						if (!(node is BucketNode<T, int> bucketNode))
						{
							if (!(node is BucketNode<T, string> bucketNode2))
							{
								if (node is IndirectionNode<T> { Child: not null } indirectionNode)
								{
									writer.WritePropertyName(indirectionNode.NodeId.ToString());
									writer.WriteValue(indirectionNode.Child.NodeId);
									return true;
								}
							}
							else if (bucketNode2.Children.Count > 0)
							{
								writer.WritePropertyName(bucketNode2.NodeId.ToString());
								writer.WriteStartObject();
								foreach (KeyValuePair<string, INode<T>> child in bucketNode2.Children)
								{
									if (child.Value != null)
									{
										writer.WritePropertyName(child.Key);
										writer.WriteValue(child.Value.NodeId);
									}
								}
								writer.WriteEndObject();
								return true;
							}
						}
						else if (bucketNode.Children.Count > 0)
						{
							writer.WritePropertyName(bucketNode.NodeId.ToString());
							writer.WriteStartObject();
							foreach (KeyValuePair<int, INode<T>> child2 in bucketNode.Children)
							{
								if (child2.Value != null)
								{
									writer.WritePropertyName(child2.Key.ToString());
									writer.WriteValue(child2.Value.NodeId);
								}
							}
							writer.WriteEndObject();
							return true;
						}
					}
					else if (conditionNode.Child != null)
					{
						writer.WritePropertyName(conditionNode.NodeId.ToString());
						writer.WriteValue(conditionNode.Child.NodeId);
						return true;
					}
				}
				else if (priorityNode.Children.Count > 0)
				{
					writer.WritePropertyName(priorityNode.NodeId.ToString());
					writer.WriteStartArray();
					foreach (INode<T> child3 in priorityNode.Children)
					{
						if (child3 != null)
						{
							writer.WriteValue(child3.NodeId);
						}
					}
					writer.WriteEndArray();
					return true;
				}
			}
			else if (organizationNode.Child != null)
			{
				writer.WritePropertyName(organizationNode.NodeId.ToString());
				writer.WriteValue(organizationNode.Child.NodeId);
				return true;
			}
		}
		else if (errorNode.Child != null)
		{
			writer.WritePropertyName(errorNode.NodeId.ToString());
			writer.WriteValue(errorNode.Child.NodeId);
			return true;
		}
		return false;
	}
}
