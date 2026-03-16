using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class SequenceValidatorAggregate : ISequenceValidator
{
	private readonly ISequenceValidator[] _elements;

	public SequenceValidatorAggregate(params ISequenceValidator[] elements)
	{
		_elements = elements ?? Array.Empty<ISequenceValidator>();
	}

	public bool ValidateSequence(in int idx, ref List<UXEvent> events, out uint length)
	{
		length = 0u;
		if (idx + (_elements.Length - 1) >= events.Count)
		{
			return false;
		}
		for (int i = 0; i < _elements.Length; i++)
		{
			if (_elements[i].ValidateSequence(idx + i, ref events, out var length2))
			{
				length += length2;
				continue;
			}
			length = 0u;
			return false;
		}
		return true;
	}

	bool ISequenceValidator.ValidateSequence(in int startIdx, ref List<UXEvent> events, out uint length)
	{
		return ValidateSequence(in startIdx, ref events, out length);
	}
}
