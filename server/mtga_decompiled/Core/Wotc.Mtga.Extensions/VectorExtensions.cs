using UnityEngine;

namespace Wotc.Mtga.Extensions;

public static class VectorExtensions
{
	public static bool Appx(this Vector2 left, Vector2 right, float threshold = 0.01f)
	{
		if (Mathf.Abs(left.x - right.x) > threshold)
		{
			return false;
		}
		if (Mathf.Abs(left.y - right.y) > threshold)
		{
			return false;
		}
		return true;
	}

	public static bool Appx(this Vector3 left, Vector3 right, float threshold = 0.01f)
	{
		if (Mathf.Abs(left.x - right.x) > threshold)
		{
			return false;
		}
		if (Mathf.Abs(left.y - right.y) > threshold)
		{
			return false;
		}
		if (Mathf.Abs(left.z - right.z) > threshold)
		{
			return false;
		}
		return true;
	}

	public static bool Appx(this Vector4 left, Vector4 right, float threshold = 0.01f)
	{
		if (Mathf.Abs(left.x - right.x) > threshold)
		{
			return false;
		}
		if (Mathf.Abs(left.y - right.y) > threshold)
		{
			return false;
		}
		if (Mathf.Abs(left.z - right.z) > threshold)
		{
			return false;
		}
		if (Mathf.Abs(left.w - right.w) > threshold)
		{
			return false;
		}
		return true;
	}

	public static bool AppxRotation(this Vector3 lhs, Vector3 rhs, float threshold = 0.01f)
	{
		lhs = RotationWrap(lhs);
		rhs = RotationWrap(rhs);
		return lhs.Appx(rhs, threshold);
	}

	public static bool AppxRotation(this Quaternion lhs, Quaternion rhs, float threshold = 0.01f)
	{
		Vector3 left = RotationWrap(lhs.eulerAngles);
		Vector3 right = RotationWrap(rhs.eulerAngles);
		return left.Appx(right, threshold);
	}

	private static Vector3 RotationWrap(Vector3 vector)
	{
		vector.x %= 360f;
		vector.y %= 360f;
		vector.z %= 360f;
		return vector;
	}

	public static bool IsNaN(this Vector3 vector)
	{
		if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y))
		{
			return float.IsNaN(vector.z);
		}
		return true;
	}

	public static bool IsInfinity(this Vector3 vector)
	{
		if (!float.IsPositiveInfinity(vector.x) && !float.IsNegativeInfinity(vector.x) && !float.IsPositiveInfinity(vector.y) && !float.IsNegativeInfinity(vector.y) && !float.IsPositiveInfinity(vector.z))
		{
			return float.IsNegativeInfinity(vector.z);
		}
		return true;
	}

	public static bool IsFloatMinMax(this Vector3 vector)
	{
		if (vector.x != float.MinValue && vector.x != float.MaxValue && vector.y != float.MinValue && vector.y != float.MaxValue && vector.z != float.MinValue)
		{
			return vector.z == float.MaxValue;
		}
		return true;
	}

	public static Vector3 ZeroIfInvalidVector(this Vector3 vector)
	{
		if (vector.IsNaN() || vector.IsInfinity() || vector.IsFloatMinMax())
		{
			return Vector3.zero;
		}
		return vector;
	}
}
