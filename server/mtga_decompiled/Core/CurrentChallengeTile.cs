using System;
using System.Collections.Generic;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class CurrentChallengeTile : MonoBehaviour
{
	[SerializeField]
	private Localize _titleText;

	[SerializeField]
	private Localize _subTitleText;

	[SerializeField]
	private CustomButton _openChallengeScreenButton;

	public Action<Guid> OnOpenChallengeScreen;

	private Guid _challengeId;

	public void Awake()
	{
		_openChallengeScreenButton?.OnClick.AddListener(OnOpenChallengeScreenButtonClicked);
	}

	private void OnDestroy()
	{
		_openChallengeScreenButton?.OnClick.RemoveAllListeners();
	}

	public void Init(PVPChallengeData activeChallenge)
	{
		base.gameObject.UpdateActive(active: true);
		_challengeId = activeChallenge.ChallengeId;
		string displayName = activeChallenge?.ChallengePlayers[activeChallenge.ChallengeOwnerId]?.FullDisplayName;
		_titleText.SetText("MainNav/Challenges/Title", new Dictionary<string, string> { 
		{
			"username",
			SharedUtilities.FormatDisplayName(displayName, 0u)
		} });
		_subTitleText.SetText("MainNav/SocialBlade/Challenges/ActiveChallengeSubtitle");
	}

	public void OnOpenChallengeScreenButtonClicked()
	{
		OnOpenChallengeScreen?.Invoke(_challengeId);
	}
}
