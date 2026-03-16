using Wotc;

namespace Wizards.Mtga.PlayBlade;

public class FilterSelectedSignalArgs : SignalArgs
{
	public BladeEventFilter BladeEventFilter { get; private set; }

	public FilterSelectedSignalArgs(object dispatcher, BladeEventFilter bladeEventFilter)
		: base(dispatcher)
	{
		BladeEventFilter = bladeEventFilter;
	}
}
