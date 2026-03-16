using System;
using AssetLookupTree.Nodes;

namespace Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

public abstract class IAltRequestLogEntry
{
	public INode Node;

	public string ResultString;

	public LogHighlightType HighlightType;

	public Guid NodeId => Node.NodeId;
}
