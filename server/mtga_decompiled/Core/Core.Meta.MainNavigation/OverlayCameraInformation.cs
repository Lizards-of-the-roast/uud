using System;
using UnityEngine;

namespace Core.Meta.MainNavigation;

public struct OverlayCameraInformation
{
	private Camera _camera;

	private int _priority;

	public Camera Camera => _camera;

	public int Priority => _priority;

	public OverlayCameraInformation(Camera camera, int priority)
	{
		if (priority == 0)
		{
			throw new ArgumentException("Priority must be non-zero! Zero is used for overlay cameras that are not set through this system, so these should not infringe on this priority.", "priority");
		}
		_camera = camera;
		_priority = priority;
	}
}
