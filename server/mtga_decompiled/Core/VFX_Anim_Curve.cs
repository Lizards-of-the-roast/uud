using UnityEngine;

public class VFX_Anim_Curve : MonoBehaviour
{
	public GameObject myObject;

	public string valueName = "Power";

	public int valueInt;

	public float value;

	private void Update()
	{
		Animator component = myObject.GetComponent<Animator>();
		AnimationCurve component2 = myObject.GetComponent<AnimationCurve>();
		value = component2.Evaluate(component.GetCurrentAnimatorStateInfo(valueInt).normalizedTime);
		Debug.Log("currently, " + valueName + " = " + value);
	}
}
