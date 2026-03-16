using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public interface ISequenceValidator
{
	bool ValidateSequence(in int startIdx, ref List<UXEvent> events, out uint length);
}
