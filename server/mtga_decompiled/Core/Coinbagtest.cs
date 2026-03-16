using UnityEngine;

public class Coinbagtest : MonoBehaviour
{
	public Animation bag;

	public GameObject coins;

	public WindEvent force;

	private GameObject coinInstance;

	private void OnGUI()
	{
		if (GUI.Button(new Rect(0f, 0f, 200f, 100f), "Spill"))
		{
			Object.Destroy(coinInstance);
			coinInstance = Object.Instantiate(coins);
			coinInstance.GetComponent<Animation>().Play();
			bag.Play();
			force.Play();
		}
	}
}
