using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code;
using EventPage.Components.NetworkModels;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class ObjectiveTrackComponent : EventComponent
{
	[SerializeField]
	private bool _cumulativeTrack;

	[SerializeField]
	private ObjectiveBubble _objectivePrefab;

	[SerializeField]
	private NotificationPopup _notificationPopupPrefab;

	[SerializeField]
	private Transform _popupParent;

	[SerializeField]
	private GameObject _cumulativePlusPrefab;

	[SerializeField]
	private GameObject _objectiveBar;

	[SerializeField]
	private ObjectiveProgressBar _objectiveProgressBar;

	[SerializeField]
	private float _totalBarTime = 20f;

	[SerializeField]
	private float _acceleration = 0.5f;

	private NotificationPopup _notificationPopup;

	private List<ObjectiveBubble> _initiatedObjectives = new List<ObjectiveBubble>(5);

	private List<GameObject> _initiatedPluses = new List<GameObject>(5);

	[HideInInspector]
	public RectTransform safeArea;

	private RewardDisplayData[] _rewardData;

	private int _currentWins;

	private int _previousWins;

	private GlobalCoroutineExecutor CoroutineExecutor => Pantry.Get<GlobalCoroutineExecutor>();

	public void SetRewardData(List<EventChestDescription> chestDescriptions, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder)
	{
		_rewardData = chestDescriptions.Select((EventChestDescription cd) => TempRewardTranslation.ChestDescriptionToDisplayData(cd, cardDatabase.CardDataProvider, cardMaterialBuilder)).ToArray();
		if (_initiatedObjectives != null)
		{
			for (int num = 0; num < _initiatedObjectives.Count; num++)
			{
				Object.Destroy(_initiatedObjectives[num].gameObject);
			}
			_initiatedObjectives.Clear();
		}
		if (_initiatedPluses != null)
		{
			for (int num2 = 0; num2 < _initiatedPluses.Count; num2++)
			{
				Object.Destroy(_initiatedPluses[num2].gameObject);
			}
			_initiatedPluses.Clear();
		}
		for (int num3 = 0; num3 < _rewardData.Length; num3++)
		{
			ObjectiveBubble objectiveBubble = Object.Instantiate(_objectivePrefab, base.transform);
			objectiveBubble.Init(cardDatabase, cardViewBuilder);
			objectiveBubble.InitPopupCallback += SetPopupOnBubble;
			objectiveBubble.SetReward(_rewardData[num3]);
			objectiveBubble.PopupRefreshSafeArea(safeArea, CurrentCamera.Value);
			_initiatedObjectives.Add(objectiveBubble);
			if (_cumulativePlusPrefab != null && num3 < _rewardData.Length - 1)
			{
				GameObject item = Object.Instantiate(_cumulativePlusPrefab, base.transform);
				_initiatedPluses.Add(item);
			}
		}
	}

	public void SetPopupOnBubble(ObjectiveBubble bubble, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		if (_notificationPopup == null)
		{
			_notificationPopup = Object.Instantiate(_notificationPopupPrefab, _popupParent);
			_notificationPopup.Init(cardDatabase, cardViewBuilder);
			_notificationPopup.gameObject.SetActive(value: false);
		}
		bubble.SetPopup(_notificationPopup);
	}

	public void SetActive(bool eventNotStarted)
	{
		base.gameObject.UpdateActive(active: true);
		_onActive(eventNotStarted);
	}

	public void UpdateWins(int currentWins, int previousWins)
	{
		_currentWins = currentWins;
		_previousWins = previousWins;
	}

	private bool AllObjectivesCompletedPreviously()
	{
		if (_rewardData.Length == 0)
		{
			return true;
		}
		RewardDisplayData rewardDisplayData = _rewardData[_rewardData.Length - 1];
		return _currentWins > rewardDisplayData.WinsNeeded;
	}

	private void _onActive(bool eventNotStarted)
	{
		bool flag = _currentWins != _previousWins;
		if (AllObjectivesCompletedPreviously())
		{
			flag = false;
		}
		int index = ((!_cumulativeTrack) ? _currentWins : 0);
		for (int i = 0; i < _rewardData.Length; i++)
		{
			RewardDisplayData rewardDisplayData = _rewardData[i];
			int winsNeeded = (int)rewardDisplayData.WinsNeeded;
			ObjectiveBubble objectiveBubble = _initiatedObjectives[i];
			MTGALocalizedString naiveLocalizedPluralString = Utils.GetNaiveLocalizedPluralString(winsNeeded, "MainNav/EventsPage/WinsStringSingular", "MainNav/EventsPage/WinsStringPlural", "quantity");
			if (_cumulativeTrack)
			{
				int num = (int)((i != 0) ? _rewardData[i - 1].WinsNeeded : 0);
				objectiveBubble.Reference_endProgress = winsNeeded;
				objectiveBubble.WinsText = naiveLocalizedPluralString;
				if (eventNotStarted)
				{
					if (_currentWins == 0 && _currentWins == winsNeeded)
					{
						objectiveBubble.ToHighlight();
						objectiveBubble.WinsText = GetXOfYLocString(_currentWins.ToString(), objectiveBubble.WinsText);
					}
					else
					{
						objectiveBubble.ToDim();
					}
				}
				else if (_currentWins >= winsNeeded)
				{
					index = Mathf.Min(i + 1, _rewardData.Length - 1);
					objectiveBubble.ToComplete();
					objectiveBubble.SetRadialFill(0f);
				}
				else if (_currentWins >= num && _currentWins < winsNeeded)
				{
					float radialFill = (float)(_currentWins - num) / (float)(winsNeeded - num);
					objectiveBubble.ToHighlight();
					objectiveBubble.SetRadialFill(radialFill);
					objectiveBubble.WinsText = GetXOfYLocString(_currentWins.ToString(), objectiveBubble.WinsText);
				}
				else
				{
					objectiveBubble.ToDim();
				}
				objectiveBubble.SetProgressText(GetXOfYLocString(_currentWins.ToString(), rewardDisplayData.WinsNeeded.ToString()));
			}
			else
			{
				objectiveBubble.SetDimmed(i != _previousWins);
				objectiveBubble.WinsText = i.ToString("N0");
				objectiveBubble.SetProgressText(naiveLocalizedPluralString);
			}
			objectiveBubble.SetReward(rewardDisplayData);
			objectiveBubble.SetSidebarVisible(visible: false);
			objectiveBubble.SetFooterText(rewardDisplayData.SecondaryText);
			objectiveBubble.SetPopupDescription("MainNav/General/Empty_String");
			objectiveBubble.SetClickable(clickable: false);
			objectiveBubble.SetRefreshHover(showRefreshIcon: false);
			objectiveBubble.SetPopupEnabledIfAllowed(!flag);
			objectiveBubble.PopupRefreshSafeArea(safeArea, CurrentCamera.Value);
			_objectiveBar.transform.SetAsFirstSibling();
		}
		if (flag)
		{
			_objectiveProgressBar.gameObject.UpdateActive(active: true);
			_objectiveProgressBar.EnableSpark(isEnabled: true);
			CoroutineExecutor.StartCoroutine(Coroutine_AnimateProgressBar(index));
		}
		else if (eventNotStarted)
		{
			_objectiveProgressBar.gameObject.UpdateActive(active: false);
			_objectiveProgressBar.EnableSpark(isEnabled: false);
			_objectiveProgressBar.SetPct(0f);
			foreach (ObjectiveBubble initiatedObjective in _initiatedObjectives)
			{
				initiatedObjective.SetClickable(clickable: false);
				initiatedObjective.SetRefreshHover(showRefreshIcon: false);
				initiatedObjective.SetPopupEnabledIfAllowed(popupEnabled: true);
			}
		}
		else
		{
			_objectiveProgressBar.gameObject.UpdateActive(active: true);
			_objectiveProgressBar.EnableSpark(isEnabled: true);
			CoroutineExecutor.StartCoroutine(Coroutine_SetProgressBar(index));
			foreach (ObjectiveBubble initiatedObjective2 in _initiatedObjectives)
			{
				initiatedObjective2.SetClickable(clickable: false);
				initiatedObjective2.SetRefreshHover(showRefreshIcon: false);
				initiatedObjective2.SetPopupEnabledIfAllowed(popupEnabled: true);
			}
		}
		_previousWins = _currentWins;
		static MTGALocalizedString GetXOfYLocString(string x, string y)
		{
			MTGALocalizedString mTGALocalizedString = "MainNav/General/X_Of_Y";
			mTGALocalizedString.Parameters = new Dictionary<string, string>
			{
				{ "x", x },
				{ "y", y }
			};
			return mTGALocalizedString;
		}
	}

	private float GetProgressBarPercent(int index)
	{
		if (_initiatedObjectives.Count == 0)
		{
			return 0f;
		}
		ObjectiveBubble objectiveBubble = _initiatedObjectives[index];
		Vector3[] array = new Vector3[4];
		_objectiveProgressBar.GetComponent<RectTransform>().GetWorldCorners(array);
		float x = objectiveBubble.GetIndicatorCanvasPosition().x;
		float x2 = array[0].x;
		float x3 = array[2].x;
		return (x - x2) / (x3 - x2);
	}

	private IEnumerator Coroutine_SetProgressBar(int index)
	{
		yield return new WaitForEndOfFrame();
		_objectiveProgressBar.SetPct(GetProgressBarPercent(index));
	}

	private IEnumerator Coroutine_AnimateProgressBar(int index)
	{
		int previousIndex = index - ((index > 0) ? 1 : 0);
		yield return Coroutine_SetProgressBar(previousIndex);
		if (index == previousIndex)
		{
			yield break;
		}
		float startPercent = GetProgressBarPercent(previousIndex);
		float currentSpeed = 0f;
		float elapsedTime = 0f;
		bool isAnimating = true;
		bool dimObjectiveAtIndex = _currentWins < (int)_rewardData[index].WinsNeeded;
		while (isAnimating)
		{
			currentSpeed += _acceleration * Time.deltaTime;
			Mathf.Clamp(startPercent + elapsedTime / _totalBarTime, 0f, 1f);
			elapsedTime += Time.deltaTime * currentSpeed;
			float barDisplayPct = startPercent + elapsedTime / _totalBarTime;
			barDisplayPct = Mathf.Clamp(barDisplayPct, 0f, 1f);
			yield return new WaitForEndOfFrame();
			_objectiveProgressBar.SetPct(barDisplayPct);
			float sparkXPos = _objectiveProgressBar.GetSparkXPos();
			float x = _initiatedObjectives[index].GetIndicatorWorldPosition().x;
			if (sparkXPos >= x)
			{
				isAnimating = false;
				_initiatedObjectives[previousIndex].SetDimmed(dimmed: true);
				_initiatedObjectives[index].SetDimmed(dimObjectiveAtIndex);
			}
		}
		_objectiveProgressBar.SetPct(GetProgressBarPercent(index));
		foreach (ObjectiveBubble initiatedObjective in _initiatedObjectives)
		{
			initiatedObjective.SetClickable(clickable: false);
			initiatedObjective.SetRefreshHover(showRefreshIcon: false);
			initiatedObjective.SetPopupEnabledIfAllowed(popupEnabled: true);
		}
	}
}
