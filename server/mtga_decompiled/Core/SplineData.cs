using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SplineData
{
	[Serializable]
	public struct SplineNode
	{
		public Vector3 Position;

		public Vector3 Rotation;

		public SplineNode(Vector3 pos, Vector3 rot)
		{
			Position = pos;
			Rotation = rot;
		}
	}

	public enum NodeType
	{
		Ignored,
		Local,
		WorldAbsolute,
		WorldRelative,
		LocalAbsolute
	}

	public enum EndType
	{
		Adhere,
		Clamp
	}

	public static readonly SplineData Straight = new SplineData
	{
		Nodes = new List<SplineNode>
		{
			new SplineNode(Vector3.zero, Vector3.zero),
			new SplineNode(Vector3.back, Vector3.zero)
		}
	};

	public static readonly SplineData Parabolic = new SplineData
	{
		Nodes = new List<SplineNode>
		{
			new SplineNode(Vector3.zero, Vector3.zero),
			new SplineNode(Vector3.back + Vector3.up * 0.5f, Vector3.zero),
			new SplineNode(Vector3.back * 2f, Vector3.zero)
		}
	};

	public NodeType PositionStyle = NodeType.WorldRelative;

	public NodeType RotationStyle = NodeType.WorldRelative;

	public EndType PositionStartType = EndType.Clamp;

	public EndType PositionEndType = EndType.Clamp;

	public EndType RotationStartType = EndType.Clamp;

	public EndType RotationEndType = EndType.Clamp;

	public Vector3 ScaleClamp = Vector3.zero;

	public List<SplineNode> Nodes = new List<SplineNode>();

	private List<Vector3> _pointCache = new List<Vector3>(2);

	public int Length => Nodes.Count;

	public SplineNode Start => Nodes[0];

	public SplineNode End => Nodes[Length - 1];

	public Vector3 GetPositionOnCurve(float percent, Vector3 start, Vector3 end)
	{
		GetModifiedPositions(start, end, _pointCache);
		return GetPositionOnHermaticCurve(percent, _pointCache);
	}

	public Vector3 GetRotationOnCurve(float percent, Vector3 start, Vector3 end, Vector3 startRot, Vector3 endRot)
	{
		GetModifiedRotations(start, end, startRot, endRot, _pointCache);
		return GetRotationOnCurve(percent, _pointCache);
	}

	public Vector3 GetRotationOnCurve(float percent, Vector3 start, Vector3 end)
	{
		GetModifiedRotations(start, end, Start.Rotation, End.Rotation, _pointCache);
		return GetRotationOnCurve(percent, _pointCache);
	}

	public Vector3 GetPositionOnCurve(float percent)
	{
		GetRawPositions(_pointCache);
		return GetPositionOnHermaticCurve(percent, _pointCache);
	}

	public Vector3 GetRotationOnCurve(float percent)
	{
		GetRawRotations(_pointCache);
		return GetRotationOnCurve(percent, _pointCache);
	}

	public void DrawDebugLine(Vector3 start, Vector3 end)
	{
		Debug.DrawRay(start, Vector3.up * 3f, Color.white, 1f);
		Debug.DrawRay(end, Vector3.up * 3f, Color.black, 1f);
		Debug.DrawLine(start + Vector3.up * 2.75f, end + Vector3.up * 2.75f, Color.Lerp(Color.white, Color.black, 0.5f), 1f);
		Vector3 positionOnCurve = GetPositionOnCurve(0f, start, end);
		for (int i = 1; (long)i <= 100L; i++)
		{
			float num = (float)i / 100f;
			Debug.DrawLine(positionOnCurve, positionOnCurve = GetPositionOnCurve(num, start, end), Color.Lerp(Color.green, Color.red, num), 1f);
		}
	}

	private void GetRawPositions(List<Vector3> positions)
	{
		positions.Clear();
		for (int i = 0; i < Nodes.Count; i++)
		{
			positions.Add(Nodes[i].Position);
		}
	}

	private void GetRawRotations(List<Vector3> rotations)
	{
		rotations.Clear();
		for (int i = 0; i < Nodes.Count; i++)
		{
			rotations.Add(Nodes[i].Rotation);
		}
	}

	private void GetModifiedPositions(Vector3 start, Vector3 end, List<Vector3> translatedPoints)
	{
		translatedPoints.Clear();
		GetRawPositions(translatedPoints);
		if (translatedPoints.Count <= 1)
		{
			return;
		}
		Vector3 vector = End.Position - Start.Position;
		Vector3 vector2 = end - start;
		NodeType positionStyle = PositionStyle;
		if (positionStyle != NodeType.WorldAbsolute && positionStyle != NodeType.LocalAbsolute)
		{
			Quaternion q = Quaternion.identity;
			bool num = vector.z < 0f;
			if (vector != Vector3.zero)
			{
				q = Quaternion.LookRotation(vector, Vector3.up);
			}
			Vector3 vector3 = (num ? (-vector2) : vector2);
			if (vector3 != Vector3.zero)
			{
				q.SetLookRotation(vector3, Vector3.up);
			}
			float num2 = vector2.magnitude / vector.magnitude;
			Vector3 s = new Vector3(num2, num2, num2);
			if (ScaleClamp.x > 0f)
			{
				s.x = Mathf.Clamp(s.x, 0f, ScaleClamp.x);
			}
			if (ScaleClamp.y > 0f)
			{
				s.y = Mathf.Clamp(s.y, 0f, ScaleClamp.y);
			}
			if (ScaleClamp.z > 0f)
			{
				s.z = Mathf.Clamp(s.z, 0f, ScaleClamp.z);
			}
			Matrix4x4 matrix4x = Matrix4x4.TRS(start, q, s);
			for (int i = 0; i < translatedPoints.Count; i++)
			{
				translatedPoints[i] = matrix4x.MultiplyPoint3x4(translatedPoints[i]);
			}
		}
		if (PositionStartType == EndType.Clamp)
		{
			translatedPoints[0] = start;
		}
		if (PositionEndType == EndType.Clamp)
		{
			translatedPoints[translatedPoints.Count - 1] = end;
		}
	}

	private void GetModifiedRotations(Vector3 start, Vector3 end, Vector3 startRot, Vector3 endRot, List<Vector3> translatedRotations)
	{
		translatedRotations.Clear();
		GetRawRotations(translatedRotations);
		if (translatedRotations.Count <= 1)
		{
			return;
		}
		NodeType rotationStyle = RotationStyle;
		if (rotationStyle != NodeType.WorldAbsolute && rotationStyle != NodeType.LocalAbsolute)
		{
			Vector3 position = Nodes[0].Position;
			Vector3 vector = Nodes[Nodes.Count - 1].Position - position;
			Vector3 vector2 = end - start;
			if (vector.normalized != vector2.normalized && vector != Vector3.up)
			{
				bool flag = vector.z < 0f;
				Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
				quaternion.SetLookRotation(flag ? (-vector2) : vector2, Vector3.up);
				for (int i = 0; i < translatedRotations.Count; i++)
				{
					translatedRotations[i] += quaternion.eulerAngles;
				}
			}
		}
		if (RotationStartType == EndType.Clamp)
		{
			translatedRotations[0] = startRot;
		}
		if (RotationEndType == EndType.Clamp)
		{
			translatedRotations[translatedRotations.Count - 1] = endRot;
		}
	}

	private Vector3 GetPositionOnHermaticCurve(float percent, List<Vector3> points)
	{
		float num = (float)(points.Count - 1) * percent;
		int num2 = Mathf.Clamp((int)Mathf.Floor(num), 0, Mathf.Max(points.Count - 2, 0));
		return HermiteGetPoint(num - (float)num2, num2, points);
	}

	private Vector3 HermiteGetPoint(float t, int i, List<Vector3> points)
	{
		float num = t * t;
		float num2 = num * t;
		Vector3 result = Vector3.zero;
		if (points.Count > 0)
		{
			result = points[0];
		}
		if (i >= points.Count)
		{
			return result;
		}
		if (points.Count > 1)
		{
			GetPCFFPoints(i, points, out var p, out var p2, out var p3, out var p4);
			result.x = (float)(0.5 * (2.0 * (double)p2.x + (double)((0f - p.x + p3.x) * t) + (2.0 * (double)p.x - 5.0 * (double)p2.x + (double)(4f * p3.x) - (double)p4.x) * (double)num + ((double)(0f - p.x) + 3.0 * (double)p2.x - 3.0 * (double)p3.x + (double)p4.x) * (double)num2));
			result.y = (float)(0.5 * (2.0 * (double)p2.y + (double)((0f - p.y + p3.y) * t) + (2.0 * (double)p.y - 5.0 * (double)p2.y + (double)(4f * p3.y) - (double)p4.y) * (double)num + ((double)(0f - p.y) + 3.0 * (double)p2.y - 3.0 * (double)p3.y + (double)p4.y) * (double)num2));
			result.z = (float)(0.5 * (2.0 * (double)p2.z + (double)((0f - p.z + p3.z) * t) + (2.0 * (double)p.z - 5.0 * (double)p2.z + (double)(4f * p3.z) - (double)p4.z) * (double)num + ((double)(0f - p.z) + 3.0 * (double)p2.z - 3.0 * (double)p3.z + (double)p4.z) * (double)num2));
		}
		return result;
	}

	private Vector3 GetRotationOnCurve(float percent, List<Vector3> rotations)
	{
		Vector3 result = Vector3.zero;
		if (rotations.Count > 1)
		{
			float num = (float)(rotations.Count - 1) * percent;
			int num2 = Mathf.Clamp((int)Mathf.Floor(num), 0, Mathf.Max(rotations.Count - 2, 0));
			float t = num - (float)num2;
			result = Quaternion.Lerp(Quaternion.Euler(rotations[num2]), Quaternion.Euler(rotations[num2 + 1]), t).eulerAngles;
		}
		else if (rotations.Count > 0)
		{
			result = rotations[0];
		}
		return result;
	}

	private void GetPCFFPoints(int i, List<Vector3> points, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
	{
		if (i > 0)
		{
			p0 = points[i - 1];
		}
		else if (i + 1 < points.Count)
		{
			p0 = points[i] + (points[i] - points[i + 1]);
		}
		else
		{
			p0 = points[i];
		}
		p1 = points[i];
		if (i + 1 < points.Count)
		{
			p2 = points[i + 1];
		}
		else
		{
			p2 = p1 + (p1 - p0);
		}
		if (i + 2 < points.Count)
		{
			p3 = points[i + 2];
		}
		else
		{
			p3 = p2 + (p2 - p1);
		}
	}
}
