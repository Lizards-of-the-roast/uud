using UnityEngine;

public class TestOpenAnimation : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Animation component = GetComponent<Animation>();
			if (component != null)
			{
				component.Play();
			}
		}
	}
}
