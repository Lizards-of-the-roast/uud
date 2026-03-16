using System.Collections.Generic;
using UnityEngine;

namespace MovementSystem;

public class SplineEventAudio : SplineEventTrigger
{
	private readonly GameObject _target;

	private readonly IEnumerable<AudioEvent> _audio;

	public SplineEventAudio(float time, IEnumerable<AudioEvent> audio, GameObject target = null)
		: base(time)
	{
		_audio = audio;
		if (!target)
		{
			target = AudioManager.Default;
		}
		_target = target;
	}

	protected override bool CanUpdate()
	{
		return _target;
	}

	protected override void Trigger(float progress)
	{
		foreach (AudioEvent item in _audio)
		{
			AudioManager.PlayAudio(item.WwiseEventName, item.PlayOnGlobal ? AudioManager.Default : _target, item.Delay);
		}
	}
}
