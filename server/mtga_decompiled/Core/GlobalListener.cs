using UnityEngine;

public class GlobalListener : MonoBehaviour
{
	private static GlobalListener _instance;

	public static GlobalListener Instance => _instance;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			Object.DontDestroyOnLoad(this);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
