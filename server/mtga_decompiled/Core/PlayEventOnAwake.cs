using UnityEngine;
using UnityEngine.Events;

public class PlayEventOnAwake : MonoBehaviour
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private UnityEvent[] _events;

	private void OnEnable()
	{
		Debug.Log("PrintOnEnable: script was enabled");
		UnityEvent[] events = _events;
		for (int i = 0; i < events.Length; i++)
		{
			events[i].Invoke();
		}
	}
}
