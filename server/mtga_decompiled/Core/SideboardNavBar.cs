using System;
using System.Collections.Generic;
using Core.Code.Decks;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

public class SideboardNavBar : MonoBehaviour
{
	private const float FUDGED_TIME_DIFF = 1.5f;

	public CustomToggle DeckViewToggle;

	public TMP_Text PlayerName;

	public GameObject[] PlayerWinPips;

	public TMP_Text OpponentName;

	public GameObject[] OpponentWinPips;

	public TMP_Text TimerText;

	public Slider TimerSlider;

	private float _timerSecondsStarting;

	private float _timerSecondsCurrent;

	private Dictionary<GREPlayerNum, TMP_Text> _namesByPlayerType;

	private Dictionary<GREPlayerNum, GameObject[]> _winPipsByPlayerType;

	private IAccountClient _accountClient;

	public float TimerSeconds => _timerSecondsCurrent;

	private DeckBuilderLayoutState DeckBuilderLayoutState => Pantry.Get<DeckBuilderLayoutState>();

	private void Awake()
	{
		_accountClient = Pantry.Get<IAccountClient>();
		_namesByPlayerType = new Dictionary<GREPlayerNum, TMP_Text>
		{
			{
				GREPlayerNum.LocalPlayer,
				PlayerName
			},
			{
				GREPlayerNum.Opponent,
				OpponentName
			}
		};
		_winPipsByPlayerType = new Dictionary<GREPlayerNum, GameObject[]>
		{
			{
				GREPlayerNum.LocalPlayer,
				PlayerWinPips
			},
			{
				GREPlayerNum.Opponent,
				OpponentWinPips
			}
		};
		DeckViewToggle.OnValueChanged.AddListener(DeckViewToggle_OnValueChanged);
		DeckViewToggle.Value = DeckBuilderLayoutState.LayoutInUse == DeckBuilderLayout.Column;
	}

	private void DeckViewToggle_OnValueChanged()
	{
		DeckBuilderLayoutState.LayoutInUse = (DeckViewToggle.Value ? DeckBuilderLayout.Column : DeckBuilderLayout.List);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
	}

	public void StartTimer(MtgTimer timer)
	{
		float num = timer.RemainingTime - calcTimerOffset();
		SetTime(_timerSecondsStarting = num - 1.5f);
		float calcTimerOffset()
		{
			if (timer == null)
			{
				return 0f;
			}
			float num2 = (float)(DateTime.UtcNow - timer.CreatedAt).TotalSeconds;
			if (num2 <= 0f)
			{
				return 0f;
			}
			return num2;
		}
	}

	public void SetTimerActive(bool isActive)
	{
		TimerText.gameObject.UpdateActive(isActive);
		TimerSlider.gameObject.UpdateActive(isActive);
	}

	public void SetPlayerWins(GREPlayerNum playerType, int wins)
	{
		if (_winPipsByPlayerType.TryGetValue(playerType, out var value))
		{
			for (int i = 0; i < value.Length; i++)
			{
				value[i].UpdateActive(i < wins);
			}
		}
	}

	public void SetPlayerName(GREPlayerNum playerType, string name)
	{
		if (_namesByPlayerType.TryGetValue(playerType, out var value))
		{
			value.text = name;
		}
	}

	private void Update()
	{
		if (_timerSecondsCurrent > 0f)
		{
			SetTime(_timerSecondsCurrent - Time.deltaTime);
		}
	}

	private void SetTime(float time)
	{
		_timerSecondsCurrent = Mathf.Max(0f, time);
		TimeSpan timeSpan = new TimeSpan(0, 0, Mathf.RoundToInt(_timerSecondsCurrent));
		TimerText.text = $"{timeSpan.Minutes:0}:{timeSpan.Seconds:00}";
		if (_timerSecondsStarting > 0f)
		{
			TimerSlider.value = _timerSecondsCurrent / _timerSecondsStarting;
		}
	}
}
