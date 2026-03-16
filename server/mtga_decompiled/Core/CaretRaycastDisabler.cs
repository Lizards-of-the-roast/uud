using TMPro;
using UnityEngine;

public class CaretRaycastDisabler : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField _inputField;

	private void Awake()
	{
		if (_inputField == null)
		{
			_inputField = GetComponent<TMP_InputField>();
		}
	}

	private void Update()
	{
		if (_inputField == null)
		{
			Object.Destroy(this);
			return;
		}
		TMP_SelectionCaret componentInChildren = _inputField.GetComponentInChildren<TMP_SelectionCaret>();
		if (componentInChildren != null)
		{
			componentInChildren.raycastTarget = false;
			Object.Destroy(this);
		}
	}
}
