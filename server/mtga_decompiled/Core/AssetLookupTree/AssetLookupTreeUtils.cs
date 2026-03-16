using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AssetLookupTree.Nodes;
using AssetLookupTree.TreeLoading.DeserializationPatterns;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AssetLookupTree;

public static class AssetLookupTreeUtils
{
	public const string ERROR_PREFIX = "[ERROR]";

	private static readonly ISerializationBinder _serializationBinder = new TreeSerializationBinder();

	private static readonly JsonSerializerSettings _altJsonSettings = new JsonSerializerSettings
	{
		SerializationBinder = _serializationBinder,
		TypeNameHandling = TypeNameHandling.Auto,
		Formatting = Formatting.Indented,
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
		NullValueHandling = NullValueHandling.Ignore,
		ContractResolver = new AltJsonContractResolver()
	};

	public static string GetPayloadTypeName<T>() where T : IPayload
	{
		return GetPayloadTypeName(typeof(T));
	}

	public static Type[] GetAllPayloadTypesOrdered()
	{
		return (from x in GetAllPayloadTypes()
			orderby x.FullName
			select x).ToArray();
	}

	public static IEnumerable<Type> GetAllPayloadTypes()
	{
		Type payloadInterface = typeof(IPayload);
		return from x in Assembly.GetAssembly(payloadInterface).GetTypes()
			where !x.IsAbstract && !x.IsInterface && payloadInterface.IsAssignableFrom(x)
			select x;
	}

	public static string GetPayloadTypeName(Type type)
	{
		if ((object)type == null)
		{
			return "INVALID";
		}
		if (type.IsAbstract || type.IsInterface)
		{
			return "INVALID";
		}
		if (!typeof(IPayload).IsAssignableFrom(type))
		{
			return "INVALID";
		}
		return type.FullName.Replace("AssetLookupTree.Payloads.", string.Empty);
	}

	public static JsonSerializerSettings DefaultJsonSettings<T>() where T : class, IPayload
	{
		AddConverterIfNecessary<T>(_altJsonSettings.Converters);
		return _altJsonSettings;
	}

	public static JsonSerializerSettings DefaultJsonSettings()
	{
		return _altJsonSettings;
	}

	public static void AddConverterIfNecessary<T>(IList<JsonConverter> converters) where T : class, IPayload
	{
		if (!AltConverterExists(converters))
		{
			lock (converters)
			{
				converters.Add(new AltConverter<T>());
			}
		}
		static bool AltConverterExists(IList<JsonConverter> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is AltConverter<T>)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool IsParentNode<T>(this INode<T> node, out IEnumerable<INode<T>> children) where T : class, IPayload
	{
		if (!(node is PriorityNode<T> priorityNode))
		{
			if (!(node is ConditionNode<T> conditionNode))
			{
				if (!(node is BucketNode<T, int> bucketNode))
				{
					if (!(node is BucketNode<T, string> bucketNode2))
					{
						if (!(node is IndirectionNode<T> indirectionNode))
						{
							if (node is OrganizationNode<T> organizationNode)
							{
								children = new INode<T>[1] { organizationNode.Child };
								return true;
							}
							children = null;
							return false;
						}
						children = new INode<T>[1] { indirectionNode.Child };
						return true;
					}
					children = bucketNode2.Children.Values;
					return true;
				}
				children = bucketNode.Children.Values;
				return true;
			}
			children = new INode<T>[1] { conditionNode.Child };
			return true;
		}
		children = priorityNode.Children;
		return true;
	}
}
