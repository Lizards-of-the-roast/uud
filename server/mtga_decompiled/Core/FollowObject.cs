using UnityEngine;

[ExecuteAlways]
public class FollowObject : MonoBehaviour
{
	public bool FollowPosition = true;

	public bool FollowRotation = true;

	public Transform FollowThis;

	private void Update()
	{
		Transform transform = base.transform;
		if ((bool)transform && (bool)FollowThis)
		{
			string text = base.gameObject.name.Replace("FollowObject_", "");
			if (FollowPosition)
			{
				transform.position = FollowThis.position;
			}
			if (FollowRotation)
			{
				transform.rotation = FollowThis.rotation;
			}
			if (text != FollowThis.gameObject.name)
			{
				base.gameObject.name = "FollowObject_" + FollowThis.gameObject.name;
			}
		}
	}
}
