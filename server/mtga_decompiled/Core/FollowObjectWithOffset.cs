using UnityEngine;

[ExecuteAlways]
public class FollowObjectWithOffset : MonoBehaviour
{
	public bool followPosition = true;

	public bool followRotation = true;

	public Vector3 offsetPosition;

	public Vector3 offsetRotation;

	public Transform followThis;

	private void Update()
	{
		Transform transform = base.transform;
		if ((bool)transform && (bool)followThis)
		{
			string text = base.gameObject.name.Replace("FollowObject_", "");
			if (followPosition)
			{
				transform.position = followThis.position + offsetPosition;
			}
			if (followRotation)
			{
				transform.rotation = Quaternion.Euler(offsetRotation) * followThis.localRotation;
			}
			if (text != followThis.gameObject.name)
			{
				base.gameObject.name = "FollowObject_" + followThis.gameObject.name;
			}
		}
	}
}
