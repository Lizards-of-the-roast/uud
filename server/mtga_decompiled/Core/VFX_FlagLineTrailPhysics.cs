using UnityEngine;

[ExecuteInEditMode]
public class VFX_FlagLineTrailPhysics : MonoBehaviour
{
	public Vector3 _targetOffset;

	public float _tightness = 0.5f;

	public bool _reverseDirection;

	[Space(20f)]
	public Vector3 _currentOffet;

	public Vector3 _lastPos;

	public Vector3 _LastOffset;

	public int _lengthPoints = 2;

	private LineRenderer _lr;

	public LineRenderer _lineRenderer
	{
		get
		{
			if (_lr == null)
			{
				_lr = GetComponent<LineRenderer>();
			}
			return _lr;
		}
	}

	private void Update()
	{
		SetPointPositions();
	}

	private void SetPointPositions()
	{
		_currentOffet = Vector3.Lerp(_lastPos - base.transform.position + _LastOffset, _targetOffset, _tightness * Time.deltaTime);
		_lengthPoints = _lineRenderer.positionCount;
		Vector3[] array = new Vector3[_lengthPoints];
		if (_reverseDirection)
		{
			for (int i = 0; i < _lengthPoints; i++)
			{
				array[i] = Vector3.Lerp(_currentOffet, Vector3.zero, (float)i / ((float)_lengthPoints - 1f));
			}
		}
		else
		{
			for (int j = 0; j < _lengthPoints; j++)
			{
				array[j] = Vector3.Lerp(Vector3.zero, _currentOffet, (float)j / ((float)_lengthPoints - 1f));
			}
		}
		_lineRenderer.SetPositions(array);
		Debug.DrawLine(Vector3.zero, base.transform.position + _currentOffet, Color.red, 0.1f);
		_lastPos = base.transform.position;
		_LastOffset = _currentOffet;
	}
}
