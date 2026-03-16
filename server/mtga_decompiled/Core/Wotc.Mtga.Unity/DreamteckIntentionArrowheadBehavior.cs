using System;
using Dreamteck.Splines;
using UnityEngine;

namespace Wotc.Mtga.Unity;

[RequireComponent(typeof(Transform))]
public class DreamteckIntentionArrowheadBehavior : SplineUser
{
	[SerializeField]
	[Range(0f, 30f)]
	[Tooltip("At this arrow length and less, this arrowhead GameObject will begin linearly scaling down to zero as arrow length reduces to zero.")]
	private float _shrinkThreshold = 5f;

	private Transform _transform;

	private Transform _cameraTransform;

	private SplineSample _latestSplineResult;

	private Vector3 _initialScaleLocal;

	protected override void Awake()
	{
		_initialScaleLocal = base.transform.localScale;
		base.Awake();
	}

	protected override void OnEnable()
	{
		Camera value = CurrentCamera.Value;
		_transform = base.transform;
		_cameraTransform = (value ? value.transform : _cameraTransform);
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		_transform = null;
		_cameraTransform = null;
		base.OnDisable();
	}

	protected override void Build()
	{
		base.Build();
		if (Application.isPlaying)
		{
			_latestSplineResult = Evaluate(1.0);
		}
	}

	protected override void PostBuild()
	{
		base.PostBuild();
		if (Application.isPlaying && !(_transform == null) && !(_cameraTransform == null))
		{
			_transform.position = _latestSplineResult.position;
			_transform.rotation = _latestSplineResult.rotation;
			float num = Vector3.Dot(_transform.forward, _cameraTransform.position - _transform.position);
			Vector3 upwards = Vector3.Normalize(_cameraTransform.position - _transform.forward * num - _transform.position);
			_transform.rotation = Quaternion.LookRotation(_transform.forward, upwards);
			float num2 = Math.Min(_shrinkThreshold, base.spline.CalculateLength());
			float num3 = ((_shrinkThreshold == 0f) ? 1f : (num2 / _shrinkThreshold));
			_transform.localScale = new Vector3(_initialScaleLocal.x, _initialScaleLocal.y, _initialScaleLocal.z * num3);
			float num4 = Vector3.Distance(_cameraTransform.position, Vector3.zero);
			float num5 = Vector3.Distance(_cameraTransform.position, _transform.position);
			float num6 = ((num4 == 0f) ? 0f : (num5 / num4));
			_transform.localScale *= num6;
		}
	}
}
