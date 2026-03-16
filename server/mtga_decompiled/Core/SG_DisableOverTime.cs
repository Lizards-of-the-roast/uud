using UnityEngine;

public class SG_DisableOverTime : MonoBehaviour
{
	public float timeUntilDisabled;

	public GameObject targetObject;

	private float _currentTime;

	private void OnEnable()
	{
		if (targetObject != null)
		{
			targetObject.SetActive(value: true);
		}
		_currentTime = 0f;
	}

	private void Update()
	{
		_currentTime += Time.deltaTime;
		if (_currentTime >= timeUntilDisabled && targetObject != null)
		{
			targetObject.SetActive(value: false);
		}
	}
}
