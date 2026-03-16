using AssetLookupTree;
using AssetLookupTree.Nodes;

public interface IAssetLookupTreeLoader
{
	INode<T> GetRootNodeOfTree<T>() where T : class, IPayload;
}
