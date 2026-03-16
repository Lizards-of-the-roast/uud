using UnityEngine;

public class Battlefield_SpawnedObject : MonoBehaviour
{
	public float velocity;

	private void Update()
	{
		base.transform.Translate(Vector3.forward * Time.deltaTime * velocity);
	}
}
