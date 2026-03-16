using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

namespace Wotc.Mtga.Unity;

[RequireComponent(typeof(SplineComputer))]
[RequireComponent(typeof(SplineRenderer))]
public class DreamteckIntentionArrowBehavior : MonoBehaviour
{
	public enum Space
	{
		Local,
		World
	}

	private SplineComputer _splineComputer;

	private SplineRenderer _splineRenderer;

	private SplinePoint[] _initialSpline;

	private SplinePoint[] _fittedSpline;

	private Vector3 _startPosition;

	private Transform _startTransform;

	private Vector3 _startOffset;

	private Space _startOffsetSpace;

	private Vector3 _endPosition;

	private Transform _endTransform;

	private Vector3 _endOffset;

	private Space _endOffsetSpace;

	private Camera _camera;

	private Transform _cameraTransform;

	private float _cameraPlaneDistance;

	private Vector3 _preferredArcDirection;

	[SerializeField]
	[Range(0f, 1f)]
	private float _roundness = 0.333f;

	private bool _isDirty;

	private WaitForEndOfFrame _waitForEndOfFrame;

	private Coroutine _setTransformChangedFalseCoroutine;

	public float Roundness
	{
		get
		{
			return _roundness;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (value != _roundness)
			{
				_roundness = value;
				_isDirty = true;
			}
		}
	}

	public Vector3 PreferredArcDirection
	{
		get
		{
			return _preferredArcDirection;
		}
		set
		{
			if (value == default(Vector3))
			{
				value = Vector3.up;
			}
			if (value != _preferredArcDirection)
			{
				_preferredArcDirection = value;
				_isDirty = true;
			}
		}
	}

	public Vector3 GetStartPosition()
	{
		Vector3 vector = _startPosition + _startOffset;
		if ((bool)_startTransform)
		{
			switch (_startOffsetSpace)
			{
			case Space.Local:
				vector = _startTransform.position + _startTransform.TransformVector(_startOffset);
				break;
			case Space.World:
				vector = _startTransform.position + _startOffset;
				break;
			}
		}
		if ((bool)_cameraTransform)
		{
			vector = ProjectOntoCameraPlane(vector, _cameraTransform, _cameraPlaneDistance);
		}
		return vector;
	}

	public Vector3 GetEndPosition()
	{
		Vector3 vector = _endPosition + _endOffset;
		if ((bool)_endTransform)
		{
			switch (_endOffsetSpace)
			{
			case Space.Local:
				vector = _endTransform.position + _endTransform.TransformVector(_endOffset);
				break;
			case Space.World:
				vector = _endTransform.position + _endOffset;
				break;
			}
		}
		if ((bool)_cameraTransform)
		{
			vector = ProjectOntoCameraPlane(vector, _cameraTransform, _cameraPlaneDistance);
		}
		return vector;
	}

	public void SetCamera(Camera camera, float cameraPlaneDistance = 1f)
	{
		_camera = null;
		_cameraTransform = null;
		_cameraPlaneDistance = 1f;
		if ((bool)camera)
		{
			_camera = camera;
			_cameraTransform = _camera.gameObject.transform;
			_cameraPlaneDistance = cameraPlaneDistance;
		}
		_isDirty = true;
	}

	public void SetStart(Vector3 startPosition, Vector3 startOffset = default(Vector3))
	{
		_startPosition = startPosition;
		_startTransform = null;
		_startOffset = startOffset;
		_startOffsetSpace = Space.World;
		_isDirty = true;
	}

	public void SetStart(Transform startTransform, Vector3 startOffset = default(Vector3), Space startOffsetSpace = Space.World)
	{
		_startPosition = default(Vector3);
		_startTransform = startTransform;
		_startOffset = startOffset;
		_startOffsetSpace = startOffsetSpace;
		_isDirty = true;
	}

	public void SetEnd(Vector3 endPosition, Vector3 endOffset = default(Vector3))
	{
		_endPosition = endPosition;
		_endTransform = null;
		_endOffset = endOffset;
		_endOffsetSpace = Space.World;
		_isDirty = true;
	}

	public void SetEnd(Transform endTransform, Vector3 endOffset = default(Vector3), Space endOffsetSpace = Space.World)
	{
		_endPosition = default(Vector3);
		_endTransform = endTransform;
		_endOffset = endOffset;
		_endOffsetSpace = endOffsetSpace;
		_isDirty = true;
	}

	public void Flush()
	{
		if (_initialSpline != null && _fittedSpline != null)
		{
			Vector3 startPosition = GetStartPosition();
			Vector3 endPosition = GetEndPosition();
			FitSpline(startPosition, endPosition, ref _fittedSpline);
			_splineComputer.SetPoints(_fittedSpline);
			_isDirty = false;
		}
	}

	private void Awake()
	{
		_preferredArcDirection = Vector3.up;
		_splineComputer = base.gameObject.GetComponent<SplineComputer>();
		_splineRenderer = base.gameObject.GetComponent<SplineRenderer>();
		_waitForEndOfFrame = new WaitForEndOfFrame();
	}

	private void OnEnable()
	{
		_setTransformChangedFalseCoroutine = StartCoroutine(SetTransformChangedFalseRoutine());
	}

	private void OnDisable()
	{
		if (_setTransformChangedFalseCoroutine != null)
		{
			StopCoroutine(_setTransformChangedFalseCoroutine);
			_setTransformChangedFalseCoroutine = null;
		}
	}

	private void OnDestroy()
	{
		_waitForEndOfFrame = null;
		_splineRenderer = null;
		_splineComputer = null;
	}

	private void Start()
	{
		_initialSpline = _splineComputer.GetPoints();
		if (_initialSpline.Length != 5)
		{
			Array.Resize(ref _initialSpline, 5);
		}
		_fittedSpline = new SplinePoint[_initialSpline.Length];
		_splineRenderer.spline.updateMode = SplineComputer.UpdateMode.FixedUpdate;
		_splineRenderer.updateMethod = SplineUser.UpdateMethod.FixedUpdate;
		_splineRenderer.calculateTangents = false;
		_splineRenderer.slices = Mathf.Min(_splineRenderer.slices, 10);
	}

	private void LateUpdate()
	{
		if ((_startTransform != null && _startTransform.hasChanged) || (_endTransform != null && _endTransform.hasChanged) || (_cameraTransform != null && _cameraTransform.hasChanged))
		{
			_isDirty = true;
		}
		if (_isDirty)
		{
			Flush();
		}
	}

	private static Vector3 ProjectOntoCameraPlane(Vector3 worldPosition, Transform cameraTransform, float cameraPlaneDistance)
	{
		Vector3 direction = worldPosition - cameraTransform.position;
		Plane plane = new Plane(-cameraTransform.forward, cameraTransform.position + cameraTransform.forward * cameraPlaneDistance);
		Ray ray = new Ray(cameraTransform.position, direction);
		if (plane.Raycast(ray, out var enter))
		{
			return ray.GetPoint(enter);
		}
		return worldPosition;
	}

	private void FitSpline(Vector3 startPosWorld, Vector3 endPosWorld, ref SplinePoint[] result)
	{
		Array.Copy(_initialSpline, result, _initialSpline.Length);
		Vector3 vector = endPosWorld - startPosWorld;
		Vector3 vector2 = Vector3.Normalize(vector);
		float num = Vector3.Magnitude(vector) * 0.5f;
		Vector3 vector3 = Vector3.Normalize(Vector3.Cross(vector, Vector3.Cross(PreferredArcDirection, vector)));
		float num2 = _roundness;
		if ((bool)_camera)
		{
			Vector3 vector4 = _camera.WorldToScreenPoint(startPosWorld);
			Vector3 vector5 = _camera.WorldToScreenPoint(endPosWorld);
			Vector3 vector6 = (vector5 + vector4) * 0.5f;
			float num3 = (vector5 - vector4).magnitude * 0.5f;
			if (vector3.y > 0f && vector6.y + num3 > (float)Screen.height)
			{
				num2 = Mathf.Min(num2, ((float)Screen.height - vector6.y) / num3);
			}
			else if (vector3.y < 0f && vector6.y - num3 < 0f)
			{
				num2 = Mathf.Min(num2, (vector6.y - 0f) / num3);
			}
		}
		Vector3 vector7 = (startPosWorld + endPosWorld) * 0.5f;
		result[0].position = startPosWorld;
		result[1].position = vector7 - vector2 * (num * 0.70710677f) + vector3 * (num * 0.70710677f * num2);
		result[2].position = vector7 + vector3 * (num * num2);
		result[3].position = vector7 + vector2 * (num * 0.70710677f) + vector3 * (num * 0.70710677f * num2);
		result[4].position = endPosWorld;
	}

	private IEnumerator SetTransformChangedFalseRoutine()
	{
		while (true)
		{
			if ((bool)_startTransform && _startTransform.hasChanged)
			{
				_startTransform.hasChanged = false;
			}
			if ((bool)_endTransform && _endTransform.hasChanged)
			{
				_endTransform.hasChanged = false;
			}
			if ((bool)_cameraTransform && _cameraTransform.hasChanged)
			{
				_cameraTransform.hasChanged = false;
			}
			yield return _waitForEndOfFrame;
		}
	}
}
