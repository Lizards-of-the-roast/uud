using System;

namespace AssetLookupTree.Payloads.Ability.Metadata;

[Flags]
public enum DisplayValidity
{
	None = 0,
	Hanger = 1,
	TTP = 2,
	Battlefield = 4
}
