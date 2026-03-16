using System;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.Serialization;

namespace Wotc.Mtga.Unity;

[RequireComponent(typeof(Transform))]
public class ScaleArrowheadBehavior : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 30f)]
	[FormerlySerializedAs("_shrinkDistance")]
	[Tooltip("At this arrow length and less, this arrowhead GameObject will begin linearly scaling down to zero as arrow length reduces to zero.")]
	private float _shrinkThreshold = 5f;

	private Transform _transform;

	private SplineComputer _splineComputer;

	private Vector3 _initialScaleLocal;

	private void Awake()
	{
		_transform = base.gameObject.GetComponent<Transform>();
	}

	private void Start()
	{
		_initialScaleLocal = _transform.localScale;
		_splineComputer = base.gameObject.GetComponentInParent<SplineComputer>();
		if (_splineComputer != null)
		{
			_splineComputer.onRebuild += OnSplineRebuild;
		}
		else
		{
			Debug.LogWarningFormat("ScaleArrowheadBehavior GameObject named [{0}] does not have a SplineComputer GameObject as a parent! Arrowhead scaling will not occur.");
		}
	}

	private void OnDestroy()
	{
		if (_splineComputer != null)
		{
			_splineComputer.onRebuild -= OnSplineRebuild;
		}
	}

	private void OnSplineRebuild()
	{
		float num = Math.Min(_shrinkThreshold, _splineComputer.CalculateLength());
		float num2 = ((num == 0f) ? 0f : (num / _shrinkThreshold));
		_transform.localScale = new Vector3(_initialScaleLocal.x, _initialScaleLocal.y, _initialScaleLocal.z * num2);
	}
}
