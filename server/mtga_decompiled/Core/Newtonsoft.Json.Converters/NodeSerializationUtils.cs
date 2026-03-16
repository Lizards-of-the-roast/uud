using System;
using AssetLookupTree;
using AssetLookupTree.Nodes;

namespace Newtonsoft.Json.Converters;

public static class NodeSerializationUtils
{
	public static SerializedNodeType GetNodeType<T>(INode<T> node) where T : class, IPayload
	{
		if (!(node is ValueNode<T>))
		{
			if (!(node is PriorityNode<T>))
			{
				if (!(node is ConditionNode<T>))
				{
					if (!(node is BucketNode<T, int>))
					{
						if (!(node is BucketNode<T, string>))
						{
							if (!(node is IndirectionNode<T>))
							{
								if (!(node is ErrorNode<T>))
								{
									if (node is OrganizationNode<T>)
									{
										return SerializedNodeType.Organization;
									}
									throw new ArgumentOutOfRangeException("node", node, null);
								}
								return SerializedNodeType.Error;
							}
							return SerializedNodeType.Indirection;
						}
						return SerializedNodeType.StringBucket;
					}
					return SerializedNodeType.IntBucket;
				}
				return SerializedNodeType.Condition;
			}
			return SerializedNodeType.Priority;
		}
		return SerializedNodeType.Value;
	}
}
