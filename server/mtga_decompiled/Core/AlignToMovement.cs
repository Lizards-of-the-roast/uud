using UnityEngine;

public class AlignToMovement : MonoBehaviour
{
	private Vector3? _prevPosition;

	private void OnEnable()
	{
		_prevPosition = null;
	}

	private void OnDisable()
	{
		_prevPosition = null;
	}

	private void LateUpdate()
	{
		if (_prevPosition.HasValue && base.transform.hasChanged)
		{
			Vector3 forward = Vector3.Normalize(base.transform.position - _prevPosition.Value);
			base.transform.forward = forward;
		}
		_prevPosition = base.transform.position;
		base.transform.hasChanged = false;
	}
}
