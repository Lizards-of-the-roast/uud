using Wotc;

namespace Wizards.Mtga.PlayBlade;

public class SelectTabSignalArgs : SignalArgs
{
	public BladeType BladeType { get; private set; }

	public SelectTabSignalArgs(object dispatcher, BladeType bladeType)
		: base(dispatcher)
	{
		BladeType = bladeType;
	}
}
