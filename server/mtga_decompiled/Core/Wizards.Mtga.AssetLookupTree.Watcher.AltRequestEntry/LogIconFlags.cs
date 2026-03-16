using System;

namespace Wizards.Mtga.AssetLookupTree.Watcher.AltRequestEntry;

[Flags]
public enum LogIconFlags
{
	None = 0,
	Removed = 1,
	Added = 2,
	Changed = 4,
	ChangedParent = 8,
	MovedIndex = 0x10,
	NestedChange = 0x20,
	SearchResult = 0x40,
	Error = 0x80,
	Success = 0x100,
	NotResult = 0x200
}
