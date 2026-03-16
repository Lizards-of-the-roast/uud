using UnityEngine;

[ExecuteInEditMode]
public class PositionAnchor : MonoBehaviour
{
	public Transform Anchor;

	private void OnEnable()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		base.transform.position = Anchor.position;
	}
}
