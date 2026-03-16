namespace MovementSystem;

public class SplineEventCardAudioETB : SplineEventTrigger
{
	private readonly BASE_CDC _cdc;

	public SplineEventCardAudioETB(float time, BASE_CDC cdc)
		: base(time)
	{
		_cdc = cdc;
	}

	protected override bool CanUpdate()
	{
		return _cdc;
	}

	protected override void Trigger(float progress)
	{
		AudioManager.Instance.PlayAudio_ETB(_cdc);
	}
}
