using UnityEngine;

[CreateAssetMenu(fileName = "AE_ST_New", menuName = "ScriptableObject/AccessoryEvents/SetTransform", order = 1)]
public class AccessoryEvent_SetTransform : AccessoryEventSO
{
	[SerializeField]
	private bool _additive;

	[SerializeField]
	private Vector3 _newLocalPosition;

	[SerializeField]
	private Vector3 _newEulerRotation;

	[SerializeField]
	private Vector3 _newLocalScale;

	public void Execute(Transform transform = null)
	{
		transform.localPosition = (_additive ? (transform.localPosition + _newLocalPosition) : _newLocalPosition);
		transform.localEulerAngles = (_additive ? (transform.localEulerAngles + _newEulerRotation) : _newEulerRotation);
		transform.localScale = (_additive ? (transform.localScale + _newLocalScale) : _newLocalScale);
	}
}
