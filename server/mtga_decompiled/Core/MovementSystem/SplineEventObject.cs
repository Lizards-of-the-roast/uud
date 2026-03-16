using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

namespace MovementSystem;

public class SplineEventObject : SplineEventDuration
{
	public string PrefabPath = string.Empty;

	public bool Follow;

	public Vector3 Offset = Vector3.zero;

	public Vector3 Rotation = Vector3.zero;

	public Vector3? OverridePosition;

	private readonly IUnityObjectPool _objectPool;

	protected GameObject _instance;

	protected virtual GameObject InstantiatePrefab()
	{
		GameObject gameObject = _objectPool.PopObject(PrefabPath);
		gameObject.AddOrGetComponent<SelfCleanup>().SetLifetime(Duration, SelfCleanup.CleanupType.DuelScenePool);
		return gameObject;
	}

	public SplineEventObject(float time, float duration, string prefabPath, bool follow, Vector3 offset, Vector3 rotation, Vector3? overridePosition = null)
		: base(time, duration)
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
		PrefabPath = prefabPath;
		Follow = follow;
		Offset = offset;
		Rotation = rotation;
		OverridePosition = overridePosition;
	}

	protected override bool CanUpdate()
	{
		return !string.IsNullOrEmpty(PrefabPath);
	}

	protected override void Activate(float progress, IdealPoint currPoint)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (OverridePosition.HasValue)
		{
			zero = OverridePosition.Value + Offset;
			zero2 = Rotation;
		}
		else
		{
			zero = currPoint.Position + Offset;
			zero2 = (currPoint.Rotation * Quaternion.Euler(Rotation)).eulerAngles;
		}
		_instance = InstantiatePrefab();
		_instance.transform.parent = null;
		_instance.transform.position = zero;
		_instance.transform.eulerAngles = zero2;
	}

	protected override void Update(float progress, IdealPoint currPoint)
	{
		if (Follow && _instance != null)
		{
			_instance.transform.position = currPoint.Position + Offset;
			_instance.transform.rotation = currPoint.Rotation * Quaternion.Euler(Rotation);
		}
	}

	protected override void Deactivate(float progress, IdealPoint currPoint)
	{
	}
}
