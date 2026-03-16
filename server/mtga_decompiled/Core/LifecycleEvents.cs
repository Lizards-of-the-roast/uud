using UnityEngine;
using UnityEngine.Events;

public class LifecycleEvents : MonoBehaviour
{
	public UnityEvent OnEnabled;

	public UnityEvent OnDisabled;

	public bool IsEnabled { get; private set; }

	private void OnEnable()
	{
		IsEnabled = true;
		OnEnabled.Invoke();
	}

	private void OnDisable()
	{
		IsEnabled = false;
		OnDisabled.Invoke();
	}
}
