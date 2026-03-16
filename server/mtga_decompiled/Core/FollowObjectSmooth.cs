using UnityEngine;

[ExecuteAlways]
public class FollowObjectSmooth : MonoBehaviour
{
	public Transform FollowThisSmoothly;

	public float Tightness = 1f;

	private void Update()
	{
		Transform transform = base.transform;
		if ((bool)transform && (bool)FollowThisSmoothly)
		{
			transform.position = Vector3.Lerp(base.transform.position, FollowThisSmoothly.position, Tightness * Time.deltaTime);
			transform.rotation = Quaternion.Lerp(base.transform.rotation, FollowThisSmoothly.rotation, Tightness * Time.deltaTime);
		}
	}
}
