using UnityEngine;

public class ToggleActive : MonoBehaviour
{
	[SerializeField]
	private GameObject[] TargetObjects;

	public void ActivateObject(string objName = "")
	{
		if (string.IsNullOrEmpty(objName))
		{
			return;
		}
		for (int i = 0; i < TargetObjects.Length; i++)
		{
			if (TargetObjects[i].name == objName)
			{
				TargetObjects[i].SetActive(value: true);
			}
		}
	}

	public void DeactivateObject(string objName = "")
	{
		if (string.IsNullOrEmpty(objName))
		{
			return;
		}
		for (int i = 0; i < TargetObjects.Length; i++)
		{
			if (TargetObjects[i].name == objName)
			{
				TargetObjects[i].SetActive(value: false);
			}
		}
	}
}
