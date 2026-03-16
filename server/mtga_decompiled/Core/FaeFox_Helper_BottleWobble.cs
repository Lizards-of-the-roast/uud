using UnityEngine;

public class FaeFox_Helper_BottleWobble : MonoBehaviour
{
	public Transform target;

	public Transform up;

	private void Update()
	{
		Vector3 position = base.transform.position;
		Vector3 upwards = up.position - position;
		Vector3 forward = target.position - position;
		base.transform.rotation = Quaternion.LookRotation(forward, upwards);
	}
}
