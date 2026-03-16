using AssetLookupTree;
using UnityEngine;

public class BattleFieldStaticLayoutWorldSpaceElementData : MonoBehaviour
{
	public bool Enabled = true;

	public float TargetPlaneY;

	public OffsetData Offsets;

	private Vector3 position;

	private Vector3 localPosition;

	private Quaternion rotation;

	private Quaternion localRotation;

	private Vector3 localScale;

	public void StoreTransformValues(Transform transform)
	{
		position = transform.position;
		localPosition = transform.localPosition;
		rotation = transform.rotation;
		localRotation = transform.localRotation;
		localScale = transform.localScale;
	}

	public void RestoreTransformValues(Transform transform)
	{
		transform.position = position;
		transform.localPosition = localPosition;
		transform.rotation = rotation;
		transform.localRotation = localRotation;
		transform.localScale = localScale;
	}
}
