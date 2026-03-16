using UnityEngine;
using UnityEngine.Events;

public class AccessoryEvent_TriggerFunc : MonoBehaviour
{
	[SerializeField]
	private UnityEvent function;

	private void PlayFunction()
	{
		function.Invoke();
	}
}
