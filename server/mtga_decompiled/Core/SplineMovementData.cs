using UnityEngine;

[CreateAssetMenu(fileName = "SplineMovementData", menuName = "ScriptableObject/Spline/SplineMovementData", order = 1)]
public class SplineMovementData : ScriptableObject
{
	public float Speed = 1f;

	public AnimationCurve Easing = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public SplineData Spline;

	public void PlaceOnCurveEased(Transform tran, float time, Vector3 start, Vector3 end)
	{
		if (!(tran == null))
		{
			float num = Easing.Evaluate(time);
			switch (Spline.PositionStyle)
			{
			case SplineData.NodeType.Ignored:
				tran.position = Vector3.LerpUnclamped(start, end, num);
				break;
			case SplineData.NodeType.Local:
			case SplineData.NodeType.LocalAbsolute:
				tran.localPosition = Spline.GetPositionOnCurve(num, start, end);
				break;
			case SplineData.NodeType.WorldAbsolute:
			case SplineData.NodeType.WorldRelative:
				tran.position = Spline.GetPositionOnCurve(num, start, end);
				break;
			}
			switch (Spline.RotationStyle)
			{
			case SplineData.NodeType.Local:
			case SplineData.NodeType.LocalAbsolute:
				tran.localEulerAngles = Spline.GetRotationOnCurve(num, start, end);
				break;
			case SplineData.NodeType.WorldAbsolute:
			case SplineData.NodeType.WorldRelative:
				tran.eulerAngles = Spline.GetRotationOnCurve(num, start, end);
				break;
			}
		}
	}
}
