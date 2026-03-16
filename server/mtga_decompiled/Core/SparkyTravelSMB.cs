using UnityEngine;

public class SparkyTravelSMB : SMBehaviour
{
	[SceneObjectReference(typeof(Transform))]
	[SerializeField]
	private string _anchorPath;

	[SerializeField]
	private Vector3 _sparkyLocation;

	[SerializeField]
	private bool _immediate;

	[Header("Debug")]
	public bool Pause;

	private bool _pause;

	private Transform _anchor;

	private SparkyController _sparky;

	private Vector3 _currentLocation;

	protected override void OnEnter()
	{
		_anchor = SceneObjectReference.GetSceneObject<Transform>(_anchorPath);
		_sparky = Animator.GetComponentInChildren<SparkyController>(includeInactive: true);
		Animator.ResetTrigger("SparkyArrived");
		_sparky.gameObject.SetActive(value: true);
		_currentLocation = _sparkyLocation;
		Vector3 currentLocation = _currentLocation;
		if (_anchor != null)
		{
			currentLocation.x += _anchor.transform.position.x;
			currentLocation.y += _anchor.transform.position.y;
		}
		_sparky.Travel(currentLocation, _immediate);
	}

	protected override void OnExit()
	{
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
