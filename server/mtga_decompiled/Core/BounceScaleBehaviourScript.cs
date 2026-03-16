using DG.Tweening;
using UnityEngine;

public class BounceScaleBehaviourScript : MonoBehaviour
{
	private void Start()
	{
		GetComponent<RectTransform>().DOScale(1.05f, 0.5f).SetLoops(-1, LoopType.Yoyo);
	}

	private void Update()
	{
	}
}
