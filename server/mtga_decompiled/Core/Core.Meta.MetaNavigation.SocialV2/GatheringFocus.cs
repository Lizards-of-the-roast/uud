using System;

namespace Core.Meta.MetaNavigation.SocialV2;

[Flags]
public enum GatheringFocus : ulong
{
	None = 0uL,
	Standard = 1uL,
	Limited = 2uL,
	HangingOut = 4uL,
	SocialCause = 8uL,
	Brawl = 0x10uL,
	Historic = 0x20uL,
	Timeless = 0x40uL,
	Pioneer = 0x80uL,
	Alchemy = 0x100uL,
	All = 0x1FFuL
}
