using System;
using System.Collections;
using System.Collections.Generic;
using Core.Meta.NewPlayerExperience.Graph;
using Core.Shared.Code.Providers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.QueueTips;
using Wizards.Arena.Models;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Loc;

public class CyclingTipsView : MonoBehaviour
{
	[Serializable]
	public class FadeAnimation
	{
		public float Duration = 1f;

		public Ease EaseMethod = Ease.InCubic;
	}

	public CanvasGroup CanvasGroup;

	private RectTransform _rectTransform;

	public TMP_Text TipsLabel;

	[Tooltip("current use: phyrexian text (tip keys ending with 's' will use this label instead)")]
	public TMP_Text TipsLabelSpecial;

	public float TipsBaseTime = 6f;

	public float TipsCharacterTime = 0.0667f;

	public string TipsResourceFileName = "ConstTips";

	public bool ManualControl;

	public FadeAnimation TipsFadeInAnimation;

	public FadeAnimation TipsFadeOutAnimation;

	private bool _dirty;

	private bool _started;

	private float _nextTipsTime;

	private List<QueueTip> _allTips;

	private const string NPE_COMPLETE_MILESTONE = "NPE_Completed";

	public int CurrentTipIndex { get; private set; }

	public int TipsCount => _allTips.Count;

	private void Awake()
	{
		_rectTransform = (RectTransform)base.transform;
	}

	public async void StartTips(IQueueTipProvider queueTipProvider, NewPlayerExperienceStrategy npeGraphStrategy)
	{
		if (_allTips == null)
		{
			bool isHandheld = PlatformUtils.GetCurrentDeviceType() == DeviceType.Handheld;
			QueueTipGroup group = ((!isHandheld) ? QueueTipGroup.Desktop : QueueTipGroup.Handheld);
			Dictionary<string, bool> dictionary = await npeGraphStrategy.GetNpeGraphMilestones();
			if (dictionary == null || !dictionary.TryGetValue("NPE_Completed", out var value) || !value)
			{
				group = (isHandheld ? QueueTipGroup.NPE_Handheld : QueueTipGroup.NPE_Desktop);
			}
			_allTips = queueTipProvider.GetTipsInGroup(group);
		}
		DOTween.Kill(CanvasGroup);
		if (_allTips.Count > 0)
		{
			CurrentTipIndex = UnityEngine.Random.Range(0, _allTips.Count);
			_dirty = true;
			if (!ManualControl)
			{
				_started = true;
			}
		}
	}

	public void StopTips()
	{
		_started = false;
	}

	private void Update()
	{
		if (_started)
		{
			_nextTipsTime -= Time.deltaTime;
			if (_nextTipsTime <= 0f)
			{
				StartCoroutine(Coroutine_SetNextTip());
			}
		}
		if (_dirty)
		{
			_dirty = false;
			ShowCurrentTip();
		}
	}

	private void ShowCurrentTip()
	{
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText(_allTips[CurrentTipIndex].Key);
		SetLabelText(localizedText);
		_nextTipsTime = TipsBaseTime + TipsCharacterTime * (float)localizedText.Length;
		LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
	}

	private IEnumerator Coroutine_SetNextTip()
	{
		CurrentTipIndex = UnityEngine.Random.Range(0, _allTips.Count);
		string currentTip = Languages.ActiveLocProvider.GetLocalizedText(_allTips[CurrentTipIndex].Key);
		_nextTipsTime = TipsBaseTime + TipsCharacterTime * (float)currentTip.Length + TipsFadeOutAnimation.Duration + TipsFadeInAnimation.Duration;
		DOTween.Kill(CanvasGroup);
		yield return CanvasGroup.DOFade(0f, TipsFadeOutAnimation.Duration).SetEase(TipsFadeOutAnimation.EaseMethod).SetTarget(CanvasGroup)
			.WaitForCompletion();
		SetLabelText(currentTip);
		CanvasGroup.DOFade(1f, TipsFadeInAnimation.Duration).SetEase(TipsFadeInAnimation.EaseMethod).SetTarget(CanvasGroup);
	}

	public void SetPreviousTipNoAnimation()
	{
		if (--CurrentTipIndex < 0)
		{
			CurrentTipIndex = _allTips.Count - 1;
		}
		MarkDirty();
	}

	public void SetNextTipNoAnimation()
	{
		if (++CurrentTipIndex >= _allTips.Count)
		{
			CurrentTipIndex = 0;
		}
		MarkDirty();
	}

	public void SetTipNumber(int index)
	{
		CurrentTipIndex = index;
		MarkDirty();
	}

	public void MarkDirty()
	{
		_dirty = true;
	}

	private void SetLabelText(string text)
	{
		if (_allTips[CurrentTipIndex].UseSpecialText)
		{
			TipsLabel.gameObject.SetActive(value: false);
			TipsLabelSpecial.gameObject.SetActive(value: true);
			TipsLabelSpecial.text = text;
		}
		else
		{
			TipsLabel.gameObject.SetActive(value: true);
			TipsLabel.text = text;
			TipsLabelSpecial.gameObject.SetActive(value: false);
		}
	}
}
