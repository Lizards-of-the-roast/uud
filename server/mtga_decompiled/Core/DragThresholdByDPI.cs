using UnityEngine;
using UnityEngine.EventSystems;

public class DragThresholdByDPI : MonoBehaviour
{
	private void Start()
	{
		int pixelDragThreshold = EventSystem.current.pixelDragThreshold;
		EventSystem.current.pixelDragThreshold = (int)((float)pixelDragThreshold * Screen.dpi / 160f);
	}
}
