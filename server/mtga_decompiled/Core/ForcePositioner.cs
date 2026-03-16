using UnityEngine;

public class ForcePositioner : MonoBehaviour
{
	[SerializeField]
	private bool _forceX;

	[SerializeField]
	private bool _forceY;

	[SerializeField]
	private bool _forceZ;

	[SerializeField]
	private Vector3 _vector = Vector3.zero;

	private void Awake()
	{
		if (!_forceX && !_forceY && !_forceZ)
		{
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		if (_forceX)
		{
			position.x = _vector.x;
		}
		if (_forceY)
		{
			position.y = _vector.y;
		}
		if (_forceZ)
		{
			position.z = _vector.z;
		}
		base.transform.position = position;
	}
}
