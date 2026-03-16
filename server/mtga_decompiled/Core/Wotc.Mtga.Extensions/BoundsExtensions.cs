using UnityEngine;

namespace Wotc.Mtga.Extensions;

public static class BoundsExtensions
{
	public static Rect GetScreenRect(this Bounds bounds, Camera camera)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		Vector2[] obj = new Vector2[8]
		{
			GetScreenPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z), camera),
			GetScreenPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z), camera),
			GetScreenPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z), camera),
			GetScreenPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z), camera),
			GetScreenPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z), camera),
			GetScreenPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z), camera),
			GetScreenPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z), camera),
			GetScreenPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z), camera)
		};
		Vector2 lhs = obj[0];
		Vector2 lhs2 = obj[0];
		Vector2[] array = obj;
		foreach (Vector2 rhs in array)
		{
			lhs = Vector2.Min(lhs, rhs);
			lhs2 = Vector2.Max(lhs2, rhs);
		}
		return new Rect(lhs.x, lhs.y, lhs2.x - lhs.x, lhs2.y - lhs.y);
		static Vector2 GetScreenPoint(Vector3 world, Camera cam)
		{
			Vector2 result = cam.WorldToScreenPoint(world);
			result.y = (float)Screen.height - result.y;
			return result;
		}
	}
}
