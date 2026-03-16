using System;
using Wizards.Mtga.Assets;

namespace AssetLookupTree.Nodes;

public interface IValueNode
{
	AssetPriority Priority { get; }

	IPayload Payload { get; }

	Guid NodeId { get; }
}
