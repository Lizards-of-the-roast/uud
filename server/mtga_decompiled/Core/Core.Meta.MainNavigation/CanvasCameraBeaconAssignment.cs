using System.Collections;
using System.Linq;
using UnityEngine;
using Wizards.GeneralUtilities.ObjectCommunication;

namespace Core.Meta.MainNavigation;

[RequireComponent(typeof(Canvas))]
public class CanvasCameraBeaconAssignment : MonoBehaviour
{
	[SerializeField]
	private BeaconIdentifier _baseCameraBeacon;

	[SerializeField]
	private Canvas _canvas;

	private void Awake()
	{
		if (_canvas == null)
		{
			_canvas = GetComponent<Canvas>();
		}
	}

	private IEnumerator Start()
	{
		int currentFramesAttempted = 0;
		while (_canvas.worldCamera == null && currentFramesAttempted < 60)
		{
			_canvas.worldCamera = _baseCameraBeacon.GetBeaconObject<Camera>().FirstOrDefault();
			if (_canvas.worldCamera == null)
			{
				currentFramesAttempted++;
				yield return null;
			}
		}
	}
}
