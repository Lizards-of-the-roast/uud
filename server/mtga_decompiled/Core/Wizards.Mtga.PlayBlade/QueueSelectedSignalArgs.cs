using Wotc;

namespace Wizards.Mtga.PlayBlade;

public class QueueSelectedSignalArgs : SignalArgs
{
	public BladeEventInfo BladeEventInfo { get; private set; }

	public QueueSelectedSignalArgs(object dispatcher, BladeEventInfo bladeEventInfo)
		: base(dispatcher)
	{
		BladeEventInfo = bladeEventInfo;
	}
}
