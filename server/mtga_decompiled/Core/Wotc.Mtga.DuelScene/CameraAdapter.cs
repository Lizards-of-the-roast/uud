using System;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class CameraAdapter : MonoBehaviour, ICameraAdapter
{
	[SerializeField]
	private Camera _camera;

	public Camera CameraReference => _camera;

	public Transform CameraRoot => base.transform;

	public Vector3 ViewportToWorldPoint(Vector2 viewport, float depth)
	{
		return ViewportToWorldPoint(viewport, depth, base.transform.position, base.transform.rotation, _camera.fieldOfView, _camera.aspect);
	}

	public static Vector3 ViewportToWorldPoint(Vector2 viewport, float depth, Vector3 camPosition, Quaternion camRotation, float verticalFovDeg, float aspect)
	{
		Vector3 vector = camRotation * Vector3.right;
		Vector3 vector2 = camRotation * Vector3.up;
		Vector3 vector3 = camRotation * Vector3.forward;
		float num = viewport.x * 2f - 1f;
		float num2 = viewport.y * 2f - 1f;
		float num3 = Mathf.Tan(0.5f * verticalFovDeg * (MathF.PI / 180f));
		Vector3 normalized = (vector * (num * num3 * aspect) + vector2 * (num2 * num3) + vector3).normalized;
		float a = Vector3.Dot(vector3, normalized);
		float num4 = depth / Mathf.Max(a, 1E-06f);
		return camPosition + normalized * num4;
	}
}
