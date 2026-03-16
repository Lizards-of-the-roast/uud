using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Pooling;
using UnityEngine;

public class View_Battlefield_ClickFeedback : MonoBehaviour
{
	[Serializable]
	public class SmudgeData
	{
		public Renderer SmudgeRenderer;

		public float FadeInDuration = 0.36f;

		public Ease FadeInEase = Ease.InSine;

		public float FadeOutDuration = 1.44f;

		public Ease FadeOutEase = Ease.InSine;

		public string Property = "_Color";

		public void PerformFeedback()
		{
			Sequence s = DOTween.Sequence();
			if (FadeInDuration > 0f)
			{
				TweenerCore<Color, Color, ColorOptions> tweenerCore = SmudgeRenderer.material.DOFade(0f, Property, 0f);
				if (tweenerCore != null)
				{
					s.Append(tweenerCore);
				}
				tweenerCore = SmudgeRenderer.material.DOFade(1f, Property, FadeInDuration).SetEase(FadeInEase);
				if (tweenerCore != null)
				{
					s.Append(tweenerCore);
				}
			}
			TweenerCore<Color, Color, ColorOptions> tweenerCore2 = SmudgeRenderer.material.DOFade(0f, Property, FadeOutDuration).SetEase(FadeOutEase);
			if (tweenerCore2 != null)
			{
				s.Append(tweenerCore2);
			}
		}
	}

	[SerializeField]
	private SmudgeData _smudgeData;

	[SerializeField]
	private float _lifetime = 1.8f;

	[SerializeField]
	private string wwiseSfxKey = "sfx_click_rock";

	private IUnityObjectPool _objectPool = NullUnityObjectPool.Default;

	public void SetObjectPool(IUnityObjectPool pool)
	{
		_objectPool = pool;
	}

	public void BeginFeedback()
	{
		StartCoroutine(CR_PerformFeedback());
	}

	private IEnumerator CR_PerformFeedback()
	{
		Transform transform = base.gameObject.transform;
		Vector3 position = base.gameObject.transform.position;
		AkSoundEngine.SetObjectPosition(base.gameObject, position.x, position.y, position.z, transform.forward.x, transform.forward.y, transform.forward.z, transform.up.x, transform.up.y, transform.up.z);
		AudioManager.PlayAudio(wwiseSfxKey, base.gameObject);
		_smudgeData.PerformFeedback();
		if (_lifetime > 0f)
		{
			yield return new WaitForSeconds(_lifetime);
			if (_objectPool != null)
			{
				_objectPool.PushObject(base.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		_objectPool = NullUnityObjectPool.Default;
	}
}
