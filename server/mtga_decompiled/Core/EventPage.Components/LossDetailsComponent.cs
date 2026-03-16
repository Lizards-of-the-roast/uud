using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class LossDetailsComponent : EventComponent
{
	private class LossSlot
	{
		public GameObject GameObject;

		public GameObject LossOn;

		public LossSlot(GameObject prefab, Transform parent)
		{
			GameObject = UnityEngine.Object.Instantiate(prefab, parent);
			LossOn = GameObject.transform.Find("LossON").gameObject;
		}
	}

	[SerializeField]
	private LayoutGroup _lossSlotLayout;

	[SerializeField]
	private GameObject _lossSlotPrefab;

	private List<LossSlot> _instantiatedLossSlots = new List<LossSlot>();

	[SerializeField]
	private Localize _numberOfLossesText;

	[SerializeField]
	private Localize _eventEndCriteriaText;

	[SerializeField]
	private Image _backerImage;

	private int _maxLosses;

	public void CreateLossSlots(int maxLosses)
	{
		_lossSlotLayout.gameObject.UpdateActive(active: true);
		_maxLosses = maxLosses;
		int num = Math.Max(maxLosses, _instantiatedLossSlots.Count);
		for (int i = 0; i < num; i++)
		{
			if (_instantiatedLossSlots.Count <= i)
			{
				_instantiatedLossSlots.Add(new LossSlot(_lossSlotPrefab, _lossSlotLayout.transform));
				continue;
			}
			if (i >= maxLosses)
			{
				_instantiatedLossSlots[i].GameObject.UpdateActive(active: false);
				continue;
			}
			_instantiatedLossSlots[i].GameObject.UpdateActive(active: true);
			_instantiatedLossSlots[i].LossOn.UpdateActive(active: false);
		}
	}

	public void HideLossSlots()
	{
		_lossSlotLayout.gameObject.UpdateActive(active: false);
	}

	public void UpdateLossSlots(int losses)
	{
		for (int i = 0; i < _maxLosses; i++)
		{
			_instantiatedLossSlots[i].LossOn.UpdateActive(i < losses);
		}
	}

	public void ShowNumberOfLossesText(MTGALocalizedString text)
	{
		_eventEndCriteriaText.gameObject.UpdateActive(active: false);
		_backerImage.gameObject.SetActive(value: false);
		_numberOfLossesText.gameObject.SetActive(value: true);
		_numberOfLossesText.SetText(text);
	}

	public void ShowEventEndCriteriaText(MTGALocalizedString text)
	{
		_eventEndCriteriaText.gameObject.UpdateActive(active: true);
		_backerImage.gameObject.UpdateActive(active: true);
		_numberOfLossesText.gameObject.SetActive(value: false);
		_eventEndCriteriaText.SetText(text);
	}
}
