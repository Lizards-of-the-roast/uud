using System;

namespace WAS;

[Flags]
public enum WotcFlags
{
	None = 0,
	EmailVerified = 1,
	Anonymous = 4
}
