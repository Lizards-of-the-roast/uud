using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class LineAnchor : MonoBehaviour
{
	[HideInInspector]
	public Animator Animator;

	public Transform Anchor1;

	public Transform Anchor2;

	public float pivot = 0.5f;

	public float padding1;

	public float padding2;

	private void Awake()
	{
		Animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		UpdateLine();
	}

	private void UpdateLine()
	{
		if (!(Anchor1 == null) && !(Anchor2 == null))
		{
			RectTransform rectTransform = (RectTransform)base.transform;
			Vector3 vector = Anchor1.parent.InverseTransformPoint(Anchor2.position);
			float num = Vector2.Distance(Anchor1.localPosition, vector);
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, num - padding1 - padding2);
			float num2 = padding1 / num;
			float num3 = padding2 / num;
			float t = num2 + pivot * (1f - num2 - num3);
			rectTransform.position = Vector3.LerpUnclamped(Anchor1.position, Anchor2.position, t);
			Vector3 toDirection = rectTransform.parent.InverseTransformPoint(Anchor2.position) - rectTransform.parent.InverseTransformPoint(Anchor1.position);
			rectTransform.localRotation = Quaternion.FromToRotation(Vector3.up, toDirection);
		}
	}
}
