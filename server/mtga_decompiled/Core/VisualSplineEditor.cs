using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VisualSplineEditor : MonoBehaviour
{
	[Range(10f, 1000f)]
	public float PreviewGranularity = 100f;

	public Gradient PreviewGradient = new Gradient();

	[Space(5f)]
	[Range(0f, 10f)]
	public float NodeGizmoSize = 0.25f;

	[Range(0f, 10f)]
	public float EaseGizmoSize = 0.1f;

	[Range(10f, 1000f)]
	public float EasingPreviewGranularity = 10f;

	[Space(5f)]
	public bool realtimeMode;

	[Range(0f, 1f)]
	public float previewScrub;

	public GameObject PreviewObject;

	[HideInInspector]
	public SplineMovementData _dataToEdit;

	[HideInInspector]
	public SplineData _spline;

	private float speed;

	private SplineMovementData _splineMoveData;

	private GameObject _previousPreviewObject;

	private GameObject _previewObject;

	private bool loop = true;

	private float lastTime;

	public void Awake()
	{
		PreviewGradient = new Gradient
		{
			mode = GradientMode.Blend,
			alphaKeys = new GradientAlphaKey[2]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(1f, 1f)
			},
			colorKeys = new GradientColorKey[6]
			{
				new GradientColorKey(Color.red, 0f),
				new GradientColorKey(Color.yellow, 0.2f),
				new GradientColorKey(Color.green, 0.4f),
				new GradientColorKey(Color.cyan, 0.6f),
				new GradientColorKey(Color.blue, 0.8f),
				new GradientColorKey(Color.magenta, 1f)
			}
		};
	}

	public void SetSpline(SplineMovementData data)
	{
		base.name = "Spline Editor: " + data.name;
		_dataToEdit = data;
		_spline = new SplineData
		{
			Nodes = new List<SplineData.SplineNode>(_dataToEdit.Spline.Nodes)
		};
		speed = data.Speed;
		_splineMoveData = data;
	}

	private void OnValidate()
	{
		PreviewGranularity = Mathf.Abs(PreviewGranularity);
		if (_dataToEdit == null)
		{
			_spline = null;
		}
	}

	private void OnDrawGizmos()
	{
		if (_dataToEdit == null || _spline == null)
		{
			return;
		}
		float num = 1f / PreviewGranularity;
		for (float num2 = 0f; num2 < 1f; num2 += num)
		{
			if (PreviewGradient != null)
			{
				Gizmos.color = PreviewGradient.Evaluate((2f * num2 + num) / 2f);
			}
			else
			{
				Gizmos.color = new Color(1f, 1f, 1f, 1f);
			}
			Gizmos.DrawLine(_spline.GetPositionOnCurve(num2), _spline.GetPositionOnCurve(num2 + num));
		}
		if (EaseGizmoSize > 0f)
		{
			float num3 = 1f / EasingPreviewGranularity;
			for (float num4 = 0f; num4 <= 1f; num4 += num3)
			{
				Gizmos.color = PreviewGradient.Evaluate(_dataToEdit.Easing.Evaluate(num4));
				Gizmos.DrawSphere(_spline.GetPositionOnCurve(num4), EaseGizmoSize);
			}
		}
		if (NodeGizmoSize > 0f)
		{
			for (int i = 0; i < _spline.Length; i++)
			{
				SplineData.SplineNode splineNode = _spline.Nodes[i];
				Gizmos.matrix = Matrix4x4.TRS(splineNode.Position, Quaternion.Euler(splineNode.Rotation), Vector3.one * NodeGizmoSize);
				Gizmos.color = PreviewGradient.Evaluate((float)i / ((float)_spline.Length - 1f));
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	public void PlaceOnCurve()
	{
		_previewObject = Object.Instantiate(PreviewObject, base.transform);
	}

	public void Update()
	{
		float num = Time.realtimeSinceStartup - lastTime;
		lastTime = Time.realtimeSinceStartup;
		if (null != PreviewObject)
		{
			if (_dataToEdit == null || _spline == null)
			{
				OnDestroy();
			}
			else if (PreviewObject != _previousPreviewObject)
			{
				OnDestroy();
				_previewObject = Object.Instantiate(PreviewObject, base.transform);
			}
			if (realtimeMode)
			{
				if (previewScrub >= 1f)
				{
					previewScrub = 0f;
					if (!loop)
					{
						realtimeMode = false;
					}
				}
				else
				{
					previewScrub += num * speed;
				}
			}
			_splineMoveData.PlaceOnCurveEased(_previewObject.transform, previewScrub, _spline.Start.Position, _spline.End.Position);
			_previewObject.transform.eulerAngles = _spline.GetRotationOnCurve(previewScrub);
		}
		_previousPreviewObject = PreviewObject;
	}

	private void OnDisable()
	{
		OnDestroy();
	}

	private void OnDestroy()
	{
		if (null != _previewObject)
		{
			Object.DestroyImmediate(_previewObject);
		}
	}
}
