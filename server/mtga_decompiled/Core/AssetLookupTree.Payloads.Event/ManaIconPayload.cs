using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Event;

public class ManaIconPayload : SpritePayload, ILayeredPayload, IPayload
{
	public HashSet<string> Layers { get; } = new HashSet<string>();
}
