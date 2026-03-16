using UnityEngine;

public class SimpleCardDrag : MonoBehaviour
{
	public float tiltWeight = 0.07f;

	public float tiltSmooth = 0.7f;

	public float maxMouseDelta = 8f;

	public float cardDragSpeed = 40f;

	private Vector2 _prevScreenPosition = Vector3.zero;

	private Vector2 tiltDirection = Vector2.zero;

	private static Vector2 MousePointScreen => Input.mousePosition;

	private Vector3 MousePointWorld => CurrentCamera.Value.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));

	private void Update()
	{
		Transform obj = base.transform;
		Vector3 position = obj.position;
		Vector3 vector = CurrentCamera.Value.WorldToScreenPoint(position);
		Vector2 vector2 = new Vector2(vector.x - MousePointScreen.x, vector.y - MousePointScreen.y);
		Vector3 lookRotation = Vector3.Lerp(b: Vector2.ClampMagnitude(vector2, maxMouseDelta), a: Vector3.forward, t: tiltWeight);
		Quaternion b = default(Quaternion);
		b.SetLookRotation(lookRotation);
		obj.rotation = Quaternion.Slerp(obj.rotation, b, Time.deltaTime * (5f / tiltSmooth));
		obj.position = Vector3.Lerp(position, MousePointWorld, Time.deltaTime * cardDragSpeed);
	}
}
