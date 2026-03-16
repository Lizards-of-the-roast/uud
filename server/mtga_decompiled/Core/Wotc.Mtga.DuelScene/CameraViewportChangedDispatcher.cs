using System;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class CameraViewportChangedDispatcher : IUpdate
{
	private readonly ICameraAdapter _cameraAdapterAdapter;

	private readonly ISignalDispatch<CameraViewportChangedSignalArgs> _viewportChangedEvent;

	private float _lastFOV;

	private float _lastAspect;

	private Vector3 _lastPos;

	public CameraViewportChangedDispatcher(ICameraAdapter cameraAdapter, ISignalDispatch<CameraViewportChangedSignalArgs> viewportChangedEvent)
	{
		_cameraAdapterAdapter = cameraAdapter;
		_viewportChangedEvent = viewportChangedEvent;
		_lastFOV = _cameraAdapterAdapter.CameraReference.fieldOfView;
		_lastAspect = _cameraAdapterAdapter.CameraReference.aspect;
		_lastPos = _cameraAdapterAdapter.CameraRoot.position;
	}

	public void OnUpdate(float time)
	{
		Camera cameraReference = _cameraAdapterAdapter.CameraReference;
		Transform cameraRoot = _cameraAdapterAdapter.CameraRoot;
		float fieldOfView = cameraReference.fieldOfView;
		float aspect = cameraReference.aspect;
		Vector3 position = cameraRoot.position;
		if (ViewportChanged(fieldOfView, aspect, position))
		{
			_lastFOV = fieldOfView;
			_lastAspect = aspect;
			_lastPos = position;
			_viewportChangedEvent.Dispatch(new CameraViewportChangedSignalArgs(this));
		}
	}

	private bool ViewportChanged(float fov, float aspect, Vector3 position)
	{
		if (Approx(fov, _lastFOV) && Approx(aspect, _lastAspect))
		{
			return !Approx(position, _lastPos);
		}
		return true;
	}

	private static bool Approx(Vector3 a, Vector3 b)
	{
		if (Approx(a.x, b.x) && Approx(a.y, b.y))
		{
			return Approx(a.z, b.z);
		}
		return false;
	}

	private static bool Approx(float a, float b, float tolerance = 1E-05f)
	{
		return Math.Abs(a - b) < tolerance;
	}
}
