using UnityEngine;

[ExecuteAlways]
public class AttachmentPoint : MonoBehaviour
{
	public Transform attachment;

	public bool followPosition = true;

	public bool followRotation = true;

	public Vector3 offsetPosition;

	public Vector3 offsetRotation;

	private void LateUpdate()
	{
		if ((bool)attachment)
		{
			if (followPosition)
			{
				base.transform.position = attachment.TransformPoint(offsetPosition);
			}
			if (followRotation)
			{
				base.transform.rotation = attachment.rotation * Quaternion.Euler(offsetRotation);
			}
		}
	}
}
