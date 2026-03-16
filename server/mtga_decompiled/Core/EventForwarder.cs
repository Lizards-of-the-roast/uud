using UnityEngine;
using UnityEngine.Events;

public class EventForwarder : MonoBehaviour
{
	public UnityEvent eventToForward;

	public void PlayEvent()
	{
		eventToForward.Invoke();
	}
}
