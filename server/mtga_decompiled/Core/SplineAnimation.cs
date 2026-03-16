using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

[RequireComponent(typeof(SplineComputer))]
public class SplineAnimation : MonoBehaviour
{
	public Action OnHit;

	public Action OnComplete;

	[SerializeField]
	private SplineRenderer _renderer;

	[SerializeField]
	private SplinePositioner _positioner;

	[SerializeField]
	private GameObject _startVFX;

	[SerializeField]
	private GameObject _endVFX;

	[SerializeField]
	private GameObject _fadeVFX;

	public float duration = 1f;

	[Range(0f, 1f)]
	public float hitPercent = 1f;

	public float easePower = 2f;

	public bool lineFollow;

	[Range(0.125f, 5f)]
	public float fadeSpeed = 2f;

	public float completionDelay;

	public bool disableWhenComplete = true;

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		_renderer.clipFrom = 0.0;
		_renderer.clipTo = 1.0;
		_renderer.color = Color.white;
	}

	public void Hide(float duration = 0.25f)
	{
		StartCoroutine(FadeOut(duration));
	}

	private void OnEnable()
	{
		if (_renderer == null)
		{
			_renderer = GetComponentInChildren<SplineRenderer>();
		}
		if (_positioner == null)
		{
			_positioner = GetComponentInChildren<SplinePositioner>();
		}
		if (_renderer != null)
		{
			_renderer.clipTo = 0.0;
		}
		if (_positioner != null)
		{
			_positioner.position = 0.0;
		}
		if (_startVFX != null)
		{
			_startVFX.gameObject.SetActive(value: true);
		}
		if (_endVFX != null)
		{
			_endVFX.gameObject.SetActive(value: false);
		}
		StartCoroutine(AnimateIn());
	}

	private IEnumerator AnimateIn()
	{
		yield return null;
		if ((bool)_positioner && !_positioner.gameObject.activeSelf)
		{
			_startVFX.transform.position = _renderer.spline.GetPoint(0).position;
		}
		if (_startVFX != null)
		{
			_startVFX.transform.position = _renderer.spline.GetPoint(0).position;
		}
		for (float delta = 0f; delta <= duration; delta += Time.deltaTime)
		{
			float num = Mathf.Min(delta / duration, 1f);
			num = 1f - Mathf.Pow(1f - num, easePower);
			if (num >= hitPercent && OnHit != null)
			{
				OnHit();
				OnHit = null;
			}
			if ((bool)_renderer)
			{
				_renderer.clipTo = num;
			}
			if ((bool)_positioner)
			{
				_positioner.position = num;
			}
			yield return null;
		}
		if (_endVFX != null)
		{
			_endVFX.transform.position = _renderer.spline.GetPoint(_renderer.spline.pointCount - 1).position;
			_endVFX.gameObject.SetActive(value: true);
		}
		StartCoroutine(FadeOut(completionDelay, invokeComplete: true));
	}

	private IEnumerator FadeOut(float delay, bool invokeComplete = false)
	{
		yield return null;
		Color rendererColor = _renderer.color;
		Vector3 fadeScale = ((_fadeVFX != null) ? _fadeVFX.transform.localScale : default(Vector3));
		for (float delta = 0f; delta <= delay; delta += Time.deltaTime)
		{
			float num = Mathf.Min(delta / delay, 1f);
			num = 1f - Mathf.Pow(1f - num, easePower);
			if (_fadeVFX != null)
			{
				_fadeVFX.transform.localScale = fadeScale * Mathf.Max(1f - num * fadeSpeed, 0.001f);
			}
			if (disableWhenComplete)
			{
				if (lineFollow)
				{
					_renderer.clipFrom = num;
				}
				else
				{
					_renderer.color = rendererColor * Mathf.Max(1f - num, 0f);
				}
			}
			yield return null;
		}
		if (disableWhenComplete)
		{
			base.gameObject.SetActive(value: false);
		}
		if (invokeComplete && OnComplete != null)
		{
			OnComplete();
			OnComplete = null;
		}
	}
}
