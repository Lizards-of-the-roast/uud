using UnityEngine;

public class FrontDoorPoller : MonoBehaviour
{
	public FrontDoorConnectionAWS Connection;

	private void Update()
	{
		if (Connection != null)
		{
			Connection.ProcessMessages();
		}
	}
}
