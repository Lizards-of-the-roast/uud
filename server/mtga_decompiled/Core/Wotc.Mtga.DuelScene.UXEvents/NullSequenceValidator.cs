using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class NullSequenceValidator : ISequenceValidator
{
	public static readonly ISequenceValidator Default = new NullSequenceValidator();

	public bool ValidateSequence(in int startIdx, ref List<UXEvent> events, out uint length)
	{
		length = 0u;
		return false;
	}

	bool ISequenceValidator.ValidateSequence(in int startIdx, ref List<UXEvent> events, out uint length)
	{
		return ValidateSequence(in startIdx, ref events, out length);
	}
}
