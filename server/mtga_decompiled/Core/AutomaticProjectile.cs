using AssetLookupTree;
using UnityEngine;

public class AutomaticProjectile : MonoBehaviour
{
	[SerializeField]
	private RelativeSpace _targetSpace = RelativeSpace.Target;

	[Space(10f)]
	[SerializeField]
	private SplineMovementData _spline;

	[SerializeField]
	private VfxPrefabData _projectilePrefab = new VfxPrefabData
	{
		StartTime = 0f,
		CleanupAfterTime = 1f
	};

	[SerializeField]
	private VfxPrefabData _hitPrefab = new VfxPrefabData
	{
		StartTime = 1f,
		CleanupAfterTime = 2f
	};

	[SerializeField]
	private OffsetData _hitOffsetData = new OffsetData();

	public RelativeSpace TargetSpace
	{
		get
		{
			return _targetSpace;
		}
		set
		{
			_targetSpace = value;
		}
	}

	public SplineMovementData Spline
	{
		get
		{
			return _spline;
		}
		set
		{
			_spline = value;
		}
	}

	public VfxPrefabData ProjectilePrefab
	{
		get
		{
			return _projectilePrefab;
		}
		set
		{
			_projectilePrefab = value;
		}
	}

	public VfxPrefabData HitPrefab
	{
		get
		{
			return _hitPrefab;
		}
		set
		{
			_hitPrefab = value;
		}
	}

	public OffsetData HitOffsetData
	{
		get
		{
			return _hitOffsetData;
		}
		set
		{
			_hitOffsetData = value;
		}
	}
}
