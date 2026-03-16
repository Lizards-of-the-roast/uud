using UnityEngine;

public class PendingEvent
{
	public float _delay;

	public string _eventname;

	public GameObject _object;

	public bool isStop;

	public PendingEvent(float delay, string eventname, GameObject obj)
	{
		_delay = delay;
		_eventname = eventname;
		_object = obj;
	}
}
