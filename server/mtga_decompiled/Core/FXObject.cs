using System;
using UnityEngine;

[Serializable]
public class FXObject
{
	public Transform Prefab;

	public string AudioEventName;

	public bool AttachToParent = true;

	public float TimeDelay;

	public float TimeDuration = 1f;

	public bool StopOnLoop;

	public bool UsePrefabTransform;

	public Vector3 PositionOffset = Vector3.zero;

	private Transform _spawnedPrefab;

	private string _audioEventName;

	private float _activeDelay;

	private float _activeDuration = 1f;

	private bool _delayFinished;

	private bool _durationFinished;

	private bool _hasPlayed;

	[NonSerialized]
	private FXState _parent;

	public FXState Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	public float Duration => TimeDelay + TimeDuration;

	public void Init(FXState state)
	{
		Parent = state;
		_activeDelay = TimeDelay;
		_activeDuration = TimeDuration;
		_delayFinished = false;
		_durationFinished = false;
		_hasPlayed = false;
		_audioEventName = AudioEventName;
	}

	public void Play()
	{
		if (!_delayFinished || _durationFinished || _hasPlayed)
		{
			return;
		}
		Transform transform = Prefab.transform;
		if (AttachToParent)
		{
			_activeDelay = TimeDelay;
			_activeDuration = TimeDuration;
			_spawnedPrefab = UnityEngine.Object.Instantiate(Prefab, Parent.Parent.transform, worldPositionStays: true);
			_spawnedPrefab.transform.localPosition = Vector3.zero + PositionOffset;
			if (UsePrefabTransform)
			{
				_spawnedPrefab.localPosition += transform.position;
				_spawnedPrefab.localRotation *= transform.rotation;
			}
			_hasPlayed = true;
		}
		else
		{
			_activeDelay = TimeDelay;
			_activeDuration = TimeDuration;
			_spawnedPrefab = UnityEngine.Object.Instantiate(Prefab, Parent.Parent.transform.position + PositionOffset, Quaternion.identity);
			if (UsePrefabTransform)
			{
				_spawnedPrefab.position += transform.position;
				_spawnedPrefab.rotation = transform.rotation;
			}
			_hasPlayed = true;
		}
		if (!string.IsNullOrEmpty(_audioEventName))
		{
			AudioManager.PlayAudio(_audioEventName, _spawnedPrefab.gameObject);
		}
	}

	public void StopHard()
	{
		if ((bool)_spawnedPrefab)
		{
			_spawnedPrefab.GetComponent<ParticleSystem>().Stop();
		}
	}

	public void Stop()
	{
		if ((bool)_spawnedPrefab && StopOnLoop)
		{
			_spawnedPrefab.GetComponent<ParticleSystem>().Stop();
		}
	}

	public void Tick(float deltaTime)
	{
		if (!_delayFinished)
		{
			_activeDelay -= deltaTime;
			if (_activeDelay <= 0f)
			{
				_delayFinished = true;
				Play();
			}
		}
		if (_delayFinished)
		{
			_activeDuration -= deltaTime;
			if (_activeDuration <= 0f)
			{
				_durationFinished = true;
				Stop();
			}
		}
	}
}
