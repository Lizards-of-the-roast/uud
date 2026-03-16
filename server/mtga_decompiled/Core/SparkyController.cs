using System;
using System.Collections;
using UnityEngine;

public class SparkyController : MonoBehaviour
{
	[Header("Assets")]
	[SerializeField]
	private Canvas _dialogCanvas;

	[Tooltip("Indices 0:Top, 1:Right, 2:Bottom, 3:Left")]
	[SerializeField]
	private ChatBubble[] _chatBubblePrefabs;

	private ChatBubble[] _chatBubbles = new ChatBubble[4];

	[Header("Steering")]
	[SerializeField]
	private float _maxVelocity = 0.5f;

	[SerializeField]
	private float _maxForce = 0.1f;

	[SerializeField]
	private float _mass = 1f;

	[SerializeField]
	private float _damping = 5f;

	[SerializeField]
	private float _wanderFrequency = 0.2f;

	[SerializeField]
	private float _wanderMagnitude = 1.2f;

	public bool Pause;

	private bool _moving;

	private bool _saying;

	private ChatBubble _activeBubble;

	private bool _hasActiveBubble;

	private Animator _animator;

	private Vector3 _targetLocation;

	private Vector3 _position;

	private Vector3 _velocity;

	private string _dialogText;

	private Vector3 _dialogOffset;

	private float _dialogDuration;

	public Vector3 TargetVector { get; private set; }

	public float TimeSaid { get; private set; }

	public event Action OnArrived;

	public event Action OnSaying;

	public event Action OnSaid;

	private void Awake()
	{
		_animator = GetComponentInChildren<Animator>();
	}

	private IEnumerator Start()
	{
		yield return null;
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		if (_hasActiveBubble)
		{
			_activeBubble.gameObject.SetActive(value: false);
		}
		Say();
		_moving = false;
	}

	private void OnDestroy()
	{
		this.OnArrived = null;
		this.OnSaid = null;
		this.OnSaying = null;
	}

	public void Travel(Vector3 location, bool immediate = false)
	{
		base.gameObject.SetActive(value: true);
		if (_targetLocation != location)
		{
			_targetLocation = location;
			if (immediate)
			{
				base.transform.position = _targetLocation;
			}
			_position = base.transform.position;
			TargetVector = _targetLocation - _position;
			_moving = true;
			if (_saying)
			{
				_saying = false;
				TimeSaid = 0f;
				_activeBubble.gameObject.SetActive(value: false);
				if (_animator != null)
				{
					_animator.ResetTrigger("chatty");
					_animator.SetTrigger("idle");
				}
			}
		}
		else if (!_moving)
		{
			this.OnArrived?.Invoke();
		}
	}

	public void Say(string text = "", Vector3 offset = default(Vector3), float duration = 0f)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		ChatBubble chatBubble = (string.IsNullOrEmpty(text) ? null : GetChatBubble(offset));
		if (_hasActiveBubble && _activeBubble != chatBubble)
		{
			_saying = false;
			TimeSaid = 0f;
			_activeBubble.gameObject.SetActive(value: false);
			_activeBubble.Clicked -= DoneSaying;
			_activeBubble = null;
			_hasActiveBubble = false;
		}
		if (_dialogText != text || _dialogOffset != offset)
		{
			if (chatBubble != null)
			{
				_activeBubble = chatBubble;
				_hasActiveBubble = true;
			}
			_saying = false;
			TimeSaid = 0f;
			_dialogText = text;
			_dialogOffset = offset;
		}
		_dialogDuration = ((duration > 0f) ? duration : float.MaxValue);
	}

	private void Update()
	{
		if (!_hasActiveBubble || _moving)
		{
			return;
		}
		if (!_saying)
		{
			_saying = true;
			this.OnSaying?.Invoke();
			if (CurrentCamera.Value != null)
			{
				_dialogCanvas.worldCamera = CurrentCamera.Value;
			}
			_activeBubble.transform.localPosition = _dialogOffset;
			_activeBubble.Show(_dialogText);
			_activeBubble.Clicked -= DoneSaying;
			_activeBubble.Clicked += DoneSaying;
			if (_animator != null)
			{
				_animator.ResetTrigger("idle");
				_animator.SetTrigger("chatty");
			}
		}
		if (!Pause)
		{
			TimeSaid += Time.deltaTime;
		}
		if (TimeSaid >= _dialogDuration)
		{
			DoneSaying();
		}
	}

	private void DoneSaying()
	{
		_saying = false;
		TimeSaid = 0f;
		_activeBubble.Hide();
		_activeBubble.Clicked -= DoneSaying;
		_activeBubble = null;
		_hasActiveBubble = false;
		if (_animator != null)
		{
			_animator.ResetTrigger("chatty");
			_animator.SetTrigger("idle");
		}
		this.OnSaid?.Invoke();
	}

	private ChatBubble GetChatBubble(Vector3 offset)
	{
		int num = ((!(Mathf.Abs(offset.x) >= Mathf.Abs(offset.y))) ? ((!(offset.y >= 0f)) ? 2 : 0) : ((offset.x >= 0f) ? 1 : 3));
		if (_chatBubbles[num] == null)
		{
			_chatBubbles[num] = UnityEngine.Object.Instantiate(_chatBubblePrefabs[num], _dialogCanvas.transform, worldPositionStays: false);
		}
		return _chatBubbles[num];
	}

	private void FixedUpdate()
	{
		if (_moving)
		{
			float magnitude = TargetVector.magnitude;
			Vector3 vector = ((Mathf.Abs(TargetVector.y) > Mathf.Abs(TargetVector.z)) ? new Vector3(0f - TargetVector.y, TargetVector.x, 0f) : new Vector3(0f - TargetVector.z, 0f, TargetVector.x)) * Mathf.Sin(magnitude * _wanderFrequency);
			if (TargetVector.x < 0f)
			{
				vector *= -1f;
			}
			Vector3 vector2 = Vector3.ClampMagnitude((Vector3.Normalize(TargetVector) * _maxVelocity + vector * _wanderMagnitude * 0.01f) * Mathf.Clamp01(magnitude / _damping) - _velocity, _maxForce);
			if (_mass > 0f)
			{
				vector2 /= _mass;
			}
			_velocity += vector2;
			_position += _velocity;
			TargetVector = _targetLocation - _position;
			base.transform.position = _position;
			if (TargetVector.sqrMagnitude < 0.01f && !Pause)
			{
				_moving = false;
				this.OnArrived?.Invoke();
			}
		}
	}
}
