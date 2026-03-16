using UnityEngine;

public class EnableDisable_fromAttr : MonoBehaviour
{
	public GameObject targetObject;

	public Animator animator;

	public string attributeName;

	private void Update()
	{
		if (animator != null && targetObject != null)
		{
			bool active = (double)animator.GetFloat(attributeName) > 0.5;
			targetObject.SetActive(active);
		}
	}
}
