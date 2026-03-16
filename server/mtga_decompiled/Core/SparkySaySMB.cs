using UnityEngine;
using UnityEngine.Serialization;

public class SparkySaySMB : SMBehaviour
{
	[SerializeField]
	private string _text;

	[LocTerm]
	[SerializeField]
	private string _term;

	[SerializeField]
	private string _voiceoverId;

	[Tooltip("Time padding after voiceover is complete")]
	[SerializeField]
	private float _lapse = 1f;

	[Tooltip("Duration of the speech bubble when no voiceover is available")]
	[FormerlySerializedAs("_duration")]
	[SerializeField]
	private float _defaultDuration = 3f;

	[SerializeField]
	private Vector3 _offset;

	[Header("Debug")]
	public bool Pause;

	private bool _pause;

	private SparkyController _sparky;

	private string _sayHash;

	protected override void OnEnter()
	{
		if (!string.IsNullOrEmpty(_term))
		{
			_text = _term;
		}
		_sparky = Animator.GetComponentInChildren<SparkyController>(includeInactive: true);
		Animator.ResetTrigger("SparkySaid");
		_sparky.gameObject.SetActive(value: true);
		_sparky.OnSaying += OnSaying;
		_sparky.Say(_text, _offset, _defaultDuration);
		string text = _text;
		Vector3 offset = _offset;
		_sayHash = text + offset.ToString() + _defaultDuration;
	}

	private void OnSaying()
	{
		_sparky.OnSaying -= OnSaying;
		if (!string.IsNullOrEmpty(_voiceoverId) && AudioManager.PostEvent(_voiceoverId, _sparky.gameObject, 9u, VoiceoverCallback))
		{
			_sparky.Say(_text, _offset);
		}
	}

	private void VoiceoverCallback(object cookie, AkCallbackType type, object info)
	{
		if (base.Active)
		{
			switch (type)
			{
			case AkCallbackType.AK_EndOfEvent:
				_sparky.Say(_text, _offset, _sparky.TimeSaid + _lapse);
				break;
			case AkCallbackType.AK_Duration:
			{
				float fEstimatedDuration = ((AkDurationCallbackInfo)info).fEstimatedDuration;
				_sparky.Say(_text, _offset, fEstimatedDuration + _lapse);
				break;
			}
			}
		}
	}

	protected override void OnExit()
	{
		_sparky.OnSaying -= OnSaying;
		_sparky.Say();
		UpdatePause(pause: false);
	}

	private void UpdatePause(bool pause)
	{
		if (_pause != pause)
		{
			_pause = pause;
			_sparky.Pause = _pause;
		}
	}
}
