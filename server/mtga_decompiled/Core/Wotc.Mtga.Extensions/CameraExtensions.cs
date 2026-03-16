using System;
using UnityEngine;

namespace Wotc.Mtga.Extensions;

public static class CameraExtensions
{
	public static Rect FrustumAtPoint(this Camera camera, Transform point, out Vector3 intersectionPoint, out Vector3 cameraCenterPoint)
	{
		return camera.FrustumAtPoint(point.position, point.TransformDirection(Vector3.up), out intersectionPoint, out cameraCenterPoint);
	}

	public static Rect FrustumAtPoint(this Camera camera, Vector3 pointPosition, Vector3 pointUp, out Vector3 intersectionPoint, out Vector3 cameraCenterPoint)
	{
		Vector3 position = camera.transform.position;
		Vector3 lhs = camera.transform.forward * 60f;
		Vector3 right = camera.transform.right;
		Vector3 planeNormal = Vector3.Cross(lhs, right);
		intersectionPoint = Vector3.zero;
		UnityUtilities.LinePlaneIntersection(out intersectionPoint, pointPosition, pointUp, planeNormal, position);
		float magnitude = (position - intersectionPoint).magnitude;
		float f = Mathf.Abs(position.x - intersectionPoint.x);
		float num = Mathf.Sqrt(Mathf.Pow(magnitude, 2f) - Mathf.Pow(f, 2f));
		float num2 = 2f * num * Mathf.Tan(camera.fieldOfView * 0.5f * (MathF.PI / 180f));
		float num3 = num2 * camera.aspect;
		cameraCenterPoint = position + camera.transform.forward * num;
		return new Rect(num3 * 0.5f, num2 * 0.5f, num3, num2);
	}

	public static AspectRatio GetAspectRatio(this Camera camera)
	{
		float aspect = camera.aspect;
		if (aspect < 1.4f)
		{
			return AspectRatio.FourByThree;
		}
		if (aspect < 1.65f)
		{
			return AspectRatio.SixteenByTen;
		}
		return AspectRatio.SixteenByNine;
	}
}
