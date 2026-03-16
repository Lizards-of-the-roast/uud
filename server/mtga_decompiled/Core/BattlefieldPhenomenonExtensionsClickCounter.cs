using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class BattlefieldPhenomenonExtensionsClickCounter : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private int counter;

	[SerializeField]
	private int threshold = 3;

	[SerializeField]
	private UnityEvent _thingsToDo;

	public void AddToClick()
	{
		if (counter < threshold)
		{
			counter++;
		}
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		if (counter == threshold)
		{
			_thingsToDo.Invoke();
			counter = 0;
		}
	}
}
