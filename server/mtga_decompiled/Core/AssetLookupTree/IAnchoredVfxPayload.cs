namespace AssetLookupTree;

public interface IAnchoredVfxPayload : IPayload
{
	AnchorPointType AnchorPointType { get; set; }
}
