using System;
using System.Collections;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Challenge;
using Core.NPEStitcher;
using Core.Shared.Code;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.LearnMore;

public class TutorialLauncher : ITutorialLauncher
{
	private readonly IAccountClient _accountClient;

	private readonly ISocialManager _socialManager;

	private readonly INpeStrategy _npeStrategy;

	private readonly NPEState _npeState;

	private readonly GlobalCoroutineExecutor _coroutineExecutor;

	private readonly PVPChallengeController _challengeController;

	public static ITutorialLauncher PantryCreate()
	{
		return new TutorialLauncher(Pantry.Get<IAccountClient>(), Pantry.Get<ISocialManager>(), Pantry.Get<INpeStrategy>(), Pantry.Get<NPEState>(), Pantry.Get<GlobalCoroutineExecutor>(), Pantry.Get<PVPChallengeController>());
	}

	public TutorialLauncher(IAccountClient accountClient, ISocialManager socialManager, INpeStrategy npeStrategy, NPEState npeState, GlobalCoroutineExecutor coroutineExecutor, PVPChallengeController pvpChallengeController)
	{
		_accountClient = accountClient;
		_socialManager = socialManager;
		_npeStrategy = npeStrategy;
		_npeState = npeState;
		_coroutineExecutor = coroutineExecutor;
		_challengeController = pvpChallengeController;
	}

	public void LaunchTutorial()
	{
		if (_socialManager == null)
		{
			StartTutorial();
		}
		else if (_challengeController.GetAllChallenges().Exists((KeyValuePair<Guid, PVPChallengeData> pair) => pair.Value.ChallengePlayers.ContainsKey(pair.Value.LocalPlayerId)))
		{
			_socialManager.ShowEnteringQueueWithOutgoingChallengeMessage(StartTutorial);
		}
		else
		{
			StartTutorial();
		}
	}

	public bool CanLaunchTutorial()
	{
		NPEState.TutorialStates tutorialState = _npeState.TutorialState;
		if (_accountClient.CurrentLoginState == LoginState.FullyRegisteredLogin)
		{
			return tutorialState == NPEState.TutorialStates.Completed;
		}
		return false;
	}

	private void StartTutorial()
	{
		_coroutineExecutor.StartGlobalCoroutine(ReplayTutorial());
	}

	private IEnumerator ReplayTutorial()
	{
		_npeState.BI_NPEProgressUpdate(new NPEState.NPEProgressContext(NPEState.NPEProgressMarker.Tutorial_Replayed));
		_npeState.SetStateToEngageTutorialEventFlow();
		yield return new WaitUntil(() => _npeStrategy.Initialized);
		_npeStrategy.ReplayTutorial(delegate
		{
			PAPA.SceneLoading.LoadNPEScene(_npeStrategy);
		});
	}
}
