using UnityEngine;
using UnityEngine.Events;

public class AccessoryEventCycler : MonoBehaviour
{
	[SerializeField]
	private string cycleName;

	[SerializeField]
	private UnityEvent[] cycledRotateEvents;

	private int counter;

	public void Cycle()
	{
		counter %= cycledRotateEvents.Length;
		cycledRotateEvents[counter].Invoke();
		counter++;
	}

	public void AssignCycledRotateEvents(int numOfElementsToAdd)
	{
		cycledRotateEvents = new UnityEvent[numOfElementsToAdd];
	}
}
