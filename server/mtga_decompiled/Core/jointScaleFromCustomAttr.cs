using UnityEngine;

public class jointScaleFromCustomAttr : MonoBehaviour
{
	private Animator animator;

	public Transform targetBone;

	public string attributeName;

	public Vector3 scaleOffset = new Vector3(1f, 1f, 1f);

	private void Start()
	{
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			Debug.LogError("Animator component not found for bone scaler");
		}
	}

	private void LateUpdate()
	{
		if (animator != null && targetBone != null)
		{
			targetBone.localScale = new Vector3(animator.GetFloat(attributeName + "X") * scaleOffset.x, animator.GetFloat(attributeName + "Y") * scaleOffset.y, animator.GetFloat(attributeName + "Z") * scaleOffset.z);
		}
	}
}
