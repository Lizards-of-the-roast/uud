using UnityEngine;

[SelectionBase]
[ExecuteAlways]
public class ControlNode : MonoBehaviour
{
	public enum ControlGizmoShape
	{
		Off,
		Sphere,
		Circle2D,
		Rectangle2D
	}

	public ControlGizmoShape shape = ControlGizmoShape.Circle2D;

	[Min(0.01f)]
	public float sizeScale = 1f;

	public Transform targetBone;

	private void OnDrawGizmos()
	{
		if (shape != ControlGizmoShape.Off)
		{
			Transform obj = base.transform;
			float num = 0.25f * Mathf.Max(0.01f, sizeScale);
			Color color = Gizmos.color;
			Gizmos.color = new Color(0.05f, 0.7f, 1f, 0.18f);
			Gizmos.DrawSphere(obj.position, num * 0.8f);
			Gizmos.color = new Color(0.05f, 0.9f, 1f, 1f);
			Gizmos.DrawWireSphere(obj.position, num * 0.8f);
			Gizmos.color = color;
		}
	}
}
