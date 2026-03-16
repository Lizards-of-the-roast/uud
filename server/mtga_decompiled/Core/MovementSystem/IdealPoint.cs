using UnityEngine;

namespace MovementSystem;

public struct IdealPoint
{
	public Vector3 Position;

	public Quaternion Rotation;

	public Vector3 Scale;

	public float Speed;

	public IdealPoint(Vector3 pos, Quaternion rot, Vector3 scale, float speed = 1f)
	{
		Position = pos;
		Rotation = rot;
		Scale = scale;
		Speed = speed;
	}

	public IdealPoint(Transform tran, float speed = 1f)
	{
		Position = tran.position;
		Rotation = tran.rotation;
		Scale = tran.localScale;
		Speed = speed;
	}
}
