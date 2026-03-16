namespace AssetLookupTree.Nodes;

public interface INodeWithChild<T> : INode<T>, INode where T : class, IPayload
{
	INode<T> Child { get; set; }
}
