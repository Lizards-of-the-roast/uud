using System.Collections.Generic;
using UnityEngine;

public class SplineAccuracyTester : MonoBehaviour
{
	private const int MAX_TEST = 360;

	public SplineMovementData TestData;

	public Vector3 StartPosition = Vector3.zero;

	[Range(0.01f, 100f)]
	public float TestDistance = 10f;

	public Vector3 TestEndOffset = Vector3.zero;

	[Range(4f, 360f)]
	public int TestCount = 100;

	public Vector3 TestAxis = Vector3.up;

	[Range(3f, 100f)]
	public int TestGranularity = 10;

	private Material _mat;

	private Transform _activeTestHolder;

	private Transform _inactiveTestHolder;

	private LineRenderer _axisLine;

	private LineRenderer _originalLine;

	private readonly List<LineRenderer> _testLines = new List<LineRenderer>();

	private void Start()
	{
		_mat = new Material(Shader.Find("Particles/Fast"));
		_activeTestHolder = new GameObject("Active Tests").GetComponent<Transform>();
		_inactiveTestHolder = new GameObject("Inactive Tests").GetComponent<Transform>();
		_inactiveTestHolder.gameObject.SetActive(value: false);
		_axisLine = new GameObject("Axis").AddComponent<LineRenderer>();
		_axisLine.sharedMaterial = _mat;
		_axisLine.positionCount = 2;
		_axisLine.startColor = Color.grey;
		_axisLine.endColor = Color.black;
		_axisLine.widthMultiplier = 0.1f;
		_originalLine = new GameObject("Original").AddComponent<LineRenderer>();
		_originalLine.startColor = Color.yellow;
		_originalLine.endColor = Color.red;
		_originalLine.widthMultiplier = 0.1f;
		_originalLine.sharedMaterial = _mat;
		for (int i = 0; i < 360; i++)
		{
			LineRenderer lineRenderer = new GameObject("Test " + i).AddComponent<LineRenderer>();
			lineRenderer.startColor = Color.cyan;
			lineRenderer.startColor = Color.blue;
			lineRenderer.widthMultiplier = 0.1f;
			lineRenderer.sharedMaterial = _mat;
			lineRenderer.transform.parent = _inactiveTestHolder;
			_testLines.Add(lineRenderer);
		}
	}

	private void Update()
	{
		_axisLine.SetPositions(new Vector3[2]
		{
			StartPosition,
			StartPosition + TestAxis * 10f
		});
		if (TestData == null)
		{
			return;
		}
		float num = 1f / (float)(TestGranularity - 1);
		List<Vector3> list = new List<Vector3>(TestGranularity);
		for (int i = 0; i < TestGranularity; i++)
		{
			list.Add(TestData.Spline.GetPositionOnCurve((float)i * num));
		}
		_originalLine.positionCount = TestGranularity;
		_originalLine.numCornerVertices = TestGranularity;
		_originalLine.SetPositions(list.ToArray());
		float num2 = 360f / (float)TestCount;
		for (int j = 0; j < 360; j++)
		{
			if (j < TestCount)
			{
				_testLines[j].transform.parent = _activeTestHolder;
				Vector3 vector = Vector3.forward * TestDistance + TestEndOffset;
				vector = Quaternion.AngleAxis(num2 * (float)j, TestAxis) * vector;
				List<Vector3> list2 = new List<Vector3>(TestGranularity);
				for (int k = 0; k < TestGranularity; k++)
				{
					list2.Add(TestData.Spline.GetPositionOnCurve((float)k * num, StartPosition, vector));
				}
				_testLines[j].positionCount = TestGranularity;
				_testLines[j].numCornerVertices = TestGranularity;
				_testLines[j].SetPositions(list2.ToArray());
			}
			else
			{
				_testLines[j].transform.parent = _inactiveTestHolder;
			}
		}
	}
}
