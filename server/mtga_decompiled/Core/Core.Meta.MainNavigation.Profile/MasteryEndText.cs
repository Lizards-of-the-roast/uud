using System;
using Assets.Core.Shared.Code;
using Core.MainNavigation.RewardTrack;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Profile;

public class MasteryEndText : MonoBehaviour
{
	private TMP_Text text;

	private SetMasteryDataProvider _setMasteryDataProvider;

	private IClientLocProvider _localization;

	private TMP_Text _text
	{
		get
		{
			if (text == null)
			{
				text = GetComponent<TMP_Text>();
			}
			return text;
		}
	}

	public void Init(SetMasteryDataProvider setMasteryDataProvider, IClientLocProvider clientLocManager)
	{
		_setMasteryDataProvider = setMasteryDataProvider;
		_localization = clientLocManager;
	}

	public void Update()
	{
		if (_text == null)
		{
			return;
		}
		if (_localization == null || _setMasteryDataProvider == null)
		{
			_text.text = "";
			return;
		}
		if (_setMasteryDataProvider.GetTrackExpirationWarningTime(_setMasteryDataProvider.CurrentBpName) > ServerGameTime.GameTime)
		{
			_text.text = "";
			return;
		}
		DateTime trackExpirationTime = _setMasteryDataProvider.GetTrackExpirationTime(_setMasteryDataProvider.CurrentBpName);
		if (trackExpirationTime < ServerGameTime.GameTime)
		{
			_text.text = "";
			return;
		}
		TimeSpan timeLeft = trackExpirationTime - ServerGameTime.GameTime;
		_text.text = ProfileUtilities.GetMasteryEndingWarningMessage(timeLeft, _localization);
	}
}
