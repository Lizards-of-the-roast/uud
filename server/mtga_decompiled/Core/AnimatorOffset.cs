using UnityEngine;

public class AnimatorOffset : MonoBehaviour
{
	private float randomOffsetRotation;

	private float randomOffsetBlink;

	private Animator eyeballAnim;

	private Animator lLidAnim;

	private Animator rLidAnim;

	private void Start()
	{
		eyeballAnim = base.gameObject.transform.GetChild(0).GetComponent<Animator>();
		lLidAnim = base.gameObject.transform.GetChild(1).GetComponent<Animator>();
		rLidAnim = base.gameObject.transform.GetChild(2).GetComponent<Animator>();
		randomOffsetRotation = Random.Range(0f, 1f);
		randomOffsetBlink = Random.Range(0f, 1f);
		eyeballAnim.Play("EyeballRotate", 0, randomOffsetRotation);
		lLidAnim.Play("leftLid", 0, randomOffsetBlink);
		rLidAnim.Play("rightLid", 0, randomOffsetBlink);
	}
}
