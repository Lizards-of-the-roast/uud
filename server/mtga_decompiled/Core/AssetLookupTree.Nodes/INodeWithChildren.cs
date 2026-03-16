using System.Collections.Generic;

namespace AssetLookupTree.Nodes;

public interface INodeWithChildren<T> : INode<T>, INode where T : class, IPayload
{
	int ChildCount { get; }

	IReadOnlyCollection<INode<T>> GetChildren();
}
