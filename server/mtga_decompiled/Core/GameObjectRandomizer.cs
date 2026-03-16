using UnityEngine;

public class GameObjectRandomizer : MonoBehaviour
{
	public GameObject[] objects;

	public void EnableRandomObject()
	{
		if (objects == null || objects.Length == 0)
		{
			Debug.LogWarning("No GameObjects assigned to GameObjectRandomizer");
			return;
		}
		GameObject[] array = objects;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
		}
		int num = Random.Range(0, objects.Length);
		GameObject gameObject2 = objects[num];
		if (gameObject2 != null)
		{
			gameObject2.SetActive(value: true);
		}
	}

	private void Start()
	{
		EnableRandomObject();
	}
}
