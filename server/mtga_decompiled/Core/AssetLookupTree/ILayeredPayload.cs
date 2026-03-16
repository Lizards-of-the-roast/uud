using System.Collections.Generic;

namespace AssetLookupTree;

public interface ILayeredPayload : IPayload
{
	HashSet<string> Layers { get; }
}
