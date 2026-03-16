using UnityEngine;

namespace MTGA;

public class PackProgressMeterPopup : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _popupObjects;

	private bool _showing;

	public void TogglePopup()
	{
		_showing = !_showing;
		DisplayPopup(_showing);
	}

	public void DisplayPopup(bool display)
	{
		_showing = display;
		GameObject[] popupObjects = _popupObjects;
		for (int i = 0; i < popupObjects.Length; i++)
		{
			popupObjects[i]?.SetActive(display);
		}
	}
}
