using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class BoosterFloatingTextSpawner : MonoBehaviour
{
	[Serializable]
	public class AnimationData
	{
		public float Delay;

		public float Duration = 0.2f;

		public Ease EaseMethod = Ease.InOutSine;
	}

	[Serializable]
	public class ScaleAnimationData : AnimationData
	{
		public float Scale = 1f;
	}

	[SerializeField]
	private BoosterFloatingText _prefab;

	[SerializeField]
	private Vector3 _offsetFromTarget = new Vector3(200f, 200f, 0f);

	[SerializeField]
	[Tooltip("First, this runs to completion. Then, labelFadeOut and quantityMove begin.")]
	private AnimationData _fadeIn;

	[SerializeField]
	[Tooltip("This begins once fadeIn is complete.")]
	private AnimationData _labelFadeOut;

	[SerializeField]
	[Tooltip("This begins once quantityMove is complete.")]
	private AnimationData _quantityFadeOut;

	public void Spawn(Vector3 startTarget, Vector3 quantityTargetLeft, Vector3 quantityTargetRight, int uncommons, int commons)
	{
		BoosterFloatingText instance = UnityEngine.Object.Instantiate(_prefab, base.transform, worldPositionStays: true);
		instance.transform.ZeroOut();
		instance.transform.position = startTarget;
		instance.transform.localPosition += _offsetFromTarget;
		instance.UncommonsQuantity.text = $"+{uncommons}";
		instance.CommonsQuantity.text = $"+{commons}";
		Graphic[] array = new Graphic[4] { instance.UncommonsLabel, instance.UncommonsQuantity, instance.CommonsLabel, instance.CommonsQuantity };
		Graphic[] array2 = new Graphic[2] { instance.UncommonsLabel, instance.CommonsLabel };
		Graphic[] array3 = new Graphic[2] { instance.UncommonsQuantity, instance.CommonsQuantity };
		Sequence sequence = DOTween.Sequence();
		Graphic[] array4 = array;
		foreach (Graphic target in array4)
		{
			sequence.Insert(0f, target.DOFade(0f, _fadeIn.Duration).SetEase(_fadeIn.EaseMethod).SetDelay(_fadeIn.Delay)
				.From());
		}
		Sequence sequence2 = DOTween.Sequence();
		array4 = array2;
		foreach (Graphic target2 in array4)
		{
			sequence2.Insert(0f, target2.DOFade(0f, _labelFadeOut.Duration).SetEase(_labelFadeOut.EaseMethod).SetDelay(_labelFadeOut.Delay));
		}
		Sequence sequence3 = DOTween.Sequence();
		array4 = array3;
		foreach (Graphic target3 in array4)
		{
			sequence3.Insert(0f, target3.DOFade(0f, _quantityFadeOut.Duration).SetEase(_quantityFadeOut.EaseMethod).SetDelay(_quantityFadeOut.Delay));
		}
		Sequence sequence4 = DOTween.Sequence();
		sequence4.Insert(0f, sequence);
		Sequence sequence5 = DOTween.Sequence();
		sequence5.Insert(0f, sequence2);
		sequence5.Insert(0f, sequence3);
		Sequence sequence6 = DOTween.Sequence();
		sequence6.Append(sequence4);
		sequence6.Append(sequence5);
		sequence6.OnComplete(delegate
		{
			UnityEngine.Object.Destroy(instance.gameObject);
		});
	}
}
