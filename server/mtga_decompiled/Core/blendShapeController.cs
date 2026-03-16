using UnityEngine;

public class blendShapeController : MonoBehaviour
{
	public SkinnedMeshRenderer targetMeshRenderer;

	public string attributeName;

	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			Debug.LogError("Animator component not found for blend shape controller");
		}
	}

	private void LateUpdate()
	{
		if (animator != null)
		{
			targetMeshRenderer.SetBlendShapeWeight(0, animator.GetFloat(attributeName) * 100f);
		}
	}
}
