using System;
using System.Collections.Generic;
using Assets.Core.Shared.Code;
using Core.MainNavigation.RewardTrack;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

public class SetRotationWarning : MonoBehaviour
{
	private string _trackName;

	[SerializeField]
	private Localize _detailLabel;

	[SerializeField]
	private bool _hideForSparkRank = true;

	private TimeSpan _lastTimeLeft;

	private DateTime _expirationTime;

	private DateTime _showLabelTime;

	private bool _showLabel;

	private static SetMasteryDataProvider _masteryPassProvider => Pantry.Get<SetMasteryDataProvider>();

	private void Awake()
	{
		if (_detailLabel == null)
		{
			_detailLabel = GetComponent<Localize>();
		}
		_showLabel = _detailLabel.gameObject.activeSelf;
	}

	private void OnEnable()
	{
		if (_masteryPassProvider == null || _detailLabel == null)
		{
			base.enabled = false;
		}
		else if (string.IsNullOrEmpty(_trackName))
		{
			SetTrackName(_masteryPassProvider.CurrentBpName);
		}
	}

	public void SetTrackName(string trackname)
	{
		if (!string.IsNullOrEmpty(trackname) && _masteryPassProvider != null)
		{
			_trackName = trackname;
			_expirationTime = _masteryPassProvider.GetTrackExpirationTime(_trackName);
			_showLabelTime = _masteryPassProvider.GetTrackExpirationWarningTime(_trackName);
		}
	}

	private bool ShouldShow(DateTime expirationTime, DateTime showLabelTime, DateTime now)
	{
		bool flag = Pantry.Get<IPlayerRankServiceWrapper>().CombinedRank.constructed.rankClass == RankingClassType.Spark;
		if (expirationTime == default(DateTime) || showLabelTime == default(DateTime) || (_hideForSparkRank && flag))
		{
			return false;
		}
		if (showLabelTime <= now)
		{
			return expirationTime >= now;
		}
		return false;
	}

	private void Update()
	{
		DateTime gameTime = ServerGameTime.GameTime;
		bool flag = ShouldShow(_expirationTime, _showLabelTime, gameTime);
		if (_showLabel != flag)
		{
			_showLabel = flag;
			_detailLabel.gameObject.UpdateActive(_showLabel);
		}
		if (!flag)
		{
			return;
		}
		TimeSpan timeSpan = _expirationTime - gameTime;
		if (_lastTimeLeft.Seconds == timeSpan.Seconds)
		{
			return;
		}
		_lastTimeLeft = timeSpan;
		if (timeSpan.TotalDays >= 2.0)
		{
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/BattlePass/" + _trackName);
			if (localizedText != null)
			{
				_detailLabel.SetText("MainNav/BattlePass/SetRotation", new Dictionary<string, string>
				{
					{
						"daysCount",
						((int)timeSpan.TotalDays).ToString()
					},
					{ "previousSet", localizedText }
				});
				return;
			}
		}
		string value = timeSpan.To_HH_MM_SS();
		_detailLabel.SetText("MainNav/BattlePass/SetRotation_FullTime", new Dictionary<string, string> { { "time", value } });
	}
}
