using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Events;

namespace EventPage.CampaignGraph;

public class CampaignGraphTrackModule : EventTrackModule
{
	[SerializeField]
	private CampaignGraphObjectiveBubble _objectivePrefab;

	[SerializeField]
	private GameObject _objectiveBar;

	[SerializeField]
	private ObjectiveProgressBar _objectiveProgressBar;

	[SerializeField]
	private float _totalBarTime = 20f;

	[SerializeField]
	private float _acceleration = 0.5f;

	[SerializeField]
	private bool _isClickToOpen;

	private List<CampaignGraphObjectiveBubble> _initiatedObjectives = new List<CampaignGraphObjectiveBubble>(5);

	private IColorChallengePlayerEvent _cachedEvent;

	private IColorChallengePlayerEvent Event
	{
		get
		{
			if (!(_parentTemplate != null) || !(_parentTemplate.EventContext?.PlayerEvent is IColorChallengePlayerEvent cachedEvent))
			{
				return _cachedEvent;
			}
			return _cachedEvent = cachedEvent;
		}
	}

	public override void Show()
	{
		IColorChallengeTrack currentTrack = Event.CurrentTrack;
		for (int i = 0; i < currentTrack.Nodes.Count; i++)
		{
			Client_ColorChallengeMatchNode client_ColorChallengeMatchNode = currentTrack.Nodes[i];
			CampaignGraphObjectiveBubble campaignGraphObjectiveBubble;
			if (_initiatedObjectives.Count <= i)
			{
				campaignGraphObjectiveBubble = UnityEngine.Object.Instantiate(_objectivePrefab, base.transform);
				campaignGraphObjectiveBubble.Init(_assetLookupSystem);
				if (_isClickToOpen)
				{
					CampaignGraphObjectiveBubble campaignGraphObjectiveBubble2 = campaignGraphObjectiveBubble;
					campaignGraphObjectiveBubble2.OnMouseClickObjectiveBubble = (Action<CampaignGraphObjectiveBubble>)Delegate.Combine(campaignGraphObjectiveBubble2.OnMouseClickObjectiveBubble, new Action<CampaignGraphObjectiveBubble>(OnObjectiveBubbleMouseClick));
				}
				else
				{
					CampaignGraphObjectiveBubble campaignGraphObjectiveBubble3 = campaignGraphObjectiveBubble;
					campaignGraphObjectiveBubble3.OnMouseOverObjectiveBubble = (Action<CampaignGraphObjectiveBubble>)Delegate.Combine(campaignGraphObjectiveBubble3.OnMouseOverObjectiveBubble, new Action<CampaignGraphObjectiveBubble>(OnObjectiveBubbleMouseOver));
				}
				campaignGraphObjectiveBubble.SetIsClickToOpen(_isClickToOpen);
				_initiatedObjectives.Add(campaignGraphObjectiveBubble);
			}
			else
			{
				campaignGraphObjectiveBubble = _initiatedObjectives[i];
			}
			campaignGraphObjectiveBubble.Init(client_ColorChallengeMatchNode, currentTrack, _onTrackNodeClicked, IndexToRoman(i));
			campaignGraphObjectiveBubble.ResetAnimations();
			setAvailability(currentTrack, client_ColorChallengeMatchNode.Id, campaignGraphObjectiveBubble);
			if (client_ColorChallengeMatchNode.Id == Event.CurrentMatchNode.Id)
			{
				campaignGraphObjectiveBubble.SetSelected();
			}
			_objectiveBar.transform.SetAsFirstSibling();
		}
		if (Event.InPlayingMatchesModule)
		{
			base.gameObject.SetActive(value: true);
			PostMatchContext postMatchContext = base.EventContext.PostMatchContext;
			if (postMatchContext != null && postMatchContext.WonGame)
			{
				StartCoroutine(Coroutine_AnimateProgressBar(base.EventContext.PlayerEvent.CurrentWins));
			}
			else
			{
				StartCoroutine(Coroutine_SetProgressBar(base.EventContext.PlayerEvent.CurrentWins));
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		static string IndexToRoman(int num)
		{
			return num switch
			{
				0 => "I", 
				1 => "II", 
				2 => "III", 
				3 => "IV", 
				4 => "V", 
				_ => "", 
			};
		}
	}

	private static void setAvailability(IColorChallengeTrack track, string nodeId, CampaignGraphObjectiveBubble currentBubble)
	{
		if (track.IsNodeNextToUnlock(nodeId))
		{
			currentBubble.SetNextToUnlock();
		}
		else if (track.IsNodeCompleted(nodeId))
		{
			currentBubble.SetToCompleted();
		}
		else
		{
			currentBubble.SetToLock();
		}
	}

	private void _onTrackNodeClicked(string nodeID)
	{
		Event.SelectMatchNode(nodeID);
		_parentTemplate.UpdateModules();
	}

	public void DeactivatePopups()
	{
		foreach (CampaignGraphObjectiveBubble initiatedObjective in _initiatedObjectives)
		{
			initiatedObjective.DeactivatePopup();
		}
	}

	public override void UpdateModule()
	{
		if (Event.InPlayingMatchesModule && !base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
			StartCoroutine(Coroutine_SetProgressBar(base.EventContext.PlayerEvent.CurrentWins));
		}
		foreach (CampaignGraphObjectiveBubble initiatedObjective in _initiatedObjectives)
		{
			initiatedObjective.ResetAnimations();
			IColorChallengeTrack currentTrack = Event.CurrentTrack;
			string iD = initiatedObjective.ID;
			setAvailability(currentTrack, iD, initiatedObjective);
			if (initiatedObjective.ID == Event.CurrentMatchNode.Id)
			{
				initiatedObjective.SetSelected();
			}
			_objectiveBar.transform.SetAsFirstSibling();
		}
	}

	private void OnEnable()
	{
		if (Event != null)
		{
			UpdateModule();
		}
	}

	public override void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private float GetProgressBarPercent(int index)
	{
		if (_initiatedObjectives.Count == 0 || index < 0)
		{
			return 0f;
		}
		CampaignGraphObjectiveBubble campaignGraphObjectiveBubble = _initiatedObjectives[index];
		Vector3[] array = new Vector3[4];
		_objectiveProgressBar.GetComponent<RectTransform>().GetWorldCorners(array);
		float x = campaignGraphObjectiveBubble.GetIndicatorCanvasPosition().x;
		float x2 = array[0].x;
		float x3 = array[2].x;
		return (x - x2) / (x3 - x2);
	}

	public void OnObjectiveBubbleMouseClick(CampaignGraphObjectiveBubble hoveredObjectiveBubble)
	{
		if (hoveredObjectiveBubble.IsPopupActive)
		{
			hoveredObjectiveBubble.DeactivatePopup();
		}
		else
		{
			SetActiveObjectiveBubble(hoveredObjectiveBubble);
		}
	}

	public void OnObjectiveBubbleMouseOver(CampaignGraphObjectiveBubble hoveredObjectiveBubble)
	{
		if (!hoveredObjectiveBubble.IsPopupActive)
		{
			SetActiveObjectiveBubble(hoveredObjectiveBubble);
		}
	}

	private void SetActiveObjectiveBubble(CampaignGraphObjectiveBubble hoveredObjectiveBubble)
	{
		foreach (CampaignGraphObjectiveBubble initiatedObjective in _initiatedObjectives)
		{
			if (initiatedObjective == hoveredObjectiveBubble)
			{
				initiatedObjective.ActivatePopup();
			}
			else
			{
				initiatedObjective.DeactivatePopup();
			}
		}
	}

	private IEnumerator Coroutine_SetProgressBar(int index)
	{
		yield return new WaitForEndOfFrame();
		if (index >= _initiatedObjectives.Count)
		{
			index = _initiatedObjectives.Count - 1;
		}
		_objectiveProgressBar.SetPct(GetProgressBarPercent(index));
	}

	private IEnumerator Coroutine_AnimateProgressBar(int index)
	{
		yield return Coroutine_SetProgressBar(index - 1);
		float startPercent = GetProgressBarPercent(index - 1);
		float currentSpeed = 0f;
		float elapsedTime = 0f;
		bool isAnimating = true;
		if (index >= _initiatedObjectives.Count)
		{
			index = _initiatedObjectives.Count - 1;
		}
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
			float x = _initiatedObjectives[index].GetIndicatorPosition().x;
			if (sparkXPos >= x)
			{
				isAnimating = false;
			}
		}
		_objectiveProgressBar.SetPct(GetProgressBarPercent(index));
	}
}
