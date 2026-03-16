using UnityEngine;

public class DuelSceneBrowserElementData
{
	public bool CanHide { get; set; }

	public GameObject GameObject { get; private set; }

	public string ElementKey { get; private set; }

	public DuelSceneBrowserElementData(GameObject go, bool canHide, string elementKey)
	{
		GameObject = go;
		CanHide = canHide;
		ElementKey = elementKey;
	}
}
