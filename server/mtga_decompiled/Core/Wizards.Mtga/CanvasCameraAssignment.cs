using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Wizards.Mtga;

public class CanvasCameraAssignment : MonoBehaviour
{
	[SerializeField]
	private UnityEvent<Camera> _cameraFoundToAssign;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => CurrentCamera.Value != null);
		_cameraFoundToAssign.Invoke(CurrentCamera.Value);
	}
}
