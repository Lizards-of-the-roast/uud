using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlideShow : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField]
	private RectTransform _slideShowControlPanel;

	[SerializeField]
	private SlideElipse _elipsePrefab;

	[SerializeField]
	private Image _mainImage;

	[SerializeField]
	private float AutoTimerInSeconds = 3f;

	[SerializeField]
	private Ease _easeOnSlideTransition = Ease.InOutQuad;

	[SerializeField]
	private float _slideTransitionDuration = 0.5f;

	[SerializeField]
	private float _minAlpha = 0.8f;

	[SerializeField]
	private float _maxAlpha = 1f;

	private List<SlideElipse> _elipses;

	private int _currSlideIndex = -1;

	private float _elapsedTime;

	private bool _isPaused;

	private bool _forcePasued;

	public bool ForcePaused
	{
		set
		{
			_forcePasued = value;
		}
	}

	public bool IsPaused
	{
		set
		{
			_isPaused = value;
		}
	}

	public void SetSlides(List<SlideContent> slides)
	{
		if (slides != null && slides.Any())
		{
			_currSlideIndex = 0;
			if (slides.Count > 1)
			{
				_elipses = new List<SlideElipse>();
				int count = slides.Count;
				for (int i = 0; i < count; i++)
				{
					SlideElipse slideElipse = Object.Instantiate(_elipsePrefab);
					slideElipse.Index = (uint)i;
					slideElipse.IsActive = ((i == _currSlideIndex) ? true : false);
					slideElipse.SlideContent = slides[i];
					slideElipse.OnClickHandler = GoToSlide;
					_elipses.Add(slideElipse);
					slideElipse.transform.parent = _slideShowControlPanel;
					slideElipse.transform.localPosition = Vector3.zero;
					slideElipse.transform.localScale = Vector3.one;
				}
			}
		}
		else
		{
			_currSlideIndex = -1;
			_elipses = null;
		}
	}

	public void GoToSlide(uint index)
	{
		int nIndex = (int)index;
		if (_currSlideIndex == nIndex)
		{
			return;
		}
		int numElipse = _elipses.Count;
		DOTween.ToAlpha(() => _mainImage.color, delegate(Color x)
		{
			_mainImage.color = new Color(_mainImage.color.r, _mainImage.color.g, _mainImage.color.b, x.a);
		}, _minAlpha, _slideTransitionDuration).SetEase(_easeOnSlideTransition).OnComplete(delegate
		{
			for (int i = 0; i < numElipse; i++)
			{
				_elipses[i].IsActive = ((nIndex == i) ? true : false);
			}
			_mainImage.sprite = _elipses[nIndex].SlideContent.Sprite;
			DOTween.ToAlpha(() => _mainImage.color, delegate(Color x)
			{
				_mainImage.color = new Color(_mainImage.color.r, _mainImage.color.g, _mainImage.color.b, x.a);
			}, _maxAlpha, _slideTransitionDuration).SetEase(_easeOnSlideTransition);
		});
		_ = _elipses[nIndex].SlideContent.NavContentType;
		_currSlideIndex = (int)index;
		_elapsedTime = 0f;
	}

	private void Update()
	{
		if (_isPaused || _forcePasued)
		{
			return;
		}
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime >= AutoTimerInSeconds)
		{
			uint num = (uint)(_currSlideIndex + 1);
			if (_elipses.Count <= num)
			{
				num = 0u;
			}
			GoToSlide(num);
			_elapsedTime = 0f;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isPaused = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isPaused = false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}
}
