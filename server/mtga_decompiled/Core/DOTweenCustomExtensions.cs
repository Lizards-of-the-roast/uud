using DG.Tweening;
using UnityEngine;

public static class DOTweenCustomExtensions
{
	public static Tweener DOArcMove(this Transform trans, Vector3 end, float duration, Vector3 arcStrength)
	{
		Vector3 start = trans.position;
		Vector3 middle = 0.5f * (start + end) + arcStrength;
		return DOTween.To(() => 0f, delegate(float t)
		{
			trans.position = GetArcMovePosition(start, middle, end, t);
		}, 1f, duration);
	}

	public static Tweener DOLocalArcMove(this Transform trans, Vector3 end, float duration, Vector3 arcStrength)
	{
		Vector3 start = trans.localPosition;
		Vector3 middle = 0.5f * (start + end) + arcStrength;
		return DOTween.To(() => 0f, delegate(float t)
		{
			trans.localPosition = GetArcMovePosition(start, middle, end, t);
		}, 1f, duration);
	}

	private static Vector3 GetArcMovePosition(Vector3 start, Vector3 middle, Vector3 end, float t)
	{
		return start * Mathf.Pow(1f - t, 2f) + middle * 2f * t * (1f - t) + end * Mathf.Pow(t, 2f);
	}
}
