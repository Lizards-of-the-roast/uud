using UnityEngine;

public class Event_EnableDisableObject : MonoBehaviour
{
	[SerializeField]
	private string groupName;

	[SerializeField]
	public GameObject[] ObjectToDisableOrEnable;

	public void SetObjectActiveToTrue(string objString = "")
	{
		if (string.IsNullOrEmpty(objString))
		{
			return;
		}
		string[] array = objString.Split(",");
		foreach (string text in array)
		{
			for (int j = 0; j < ObjectToDisableOrEnable.Length; j++)
			{
				if (!(ObjectToDisableOrEnable[j] == null) && ObjectToDisableOrEnable[j].name == text)
				{
					ObjectToDisableOrEnable[j].SetActive(value: true);
				}
			}
		}
	}

	public void SetObjectActiveToFalse(string objString = "")
	{
		if (string.IsNullOrEmpty(objString))
		{
			return;
		}
		string[] array = objString.Split(",");
		foreach (string text in array)
		{
			for (int j = 0; j < ObjectToDisableOrEnable.Length; j++)
			{
				if (!(ObjectToDisableOrEnable[j] == null) && ObjectToDisableOrEnable[j].name == text)
				{
					ObjectToDisableOrEnable[j].SetActive(value: false);
				}
			}
		}
	}
}
