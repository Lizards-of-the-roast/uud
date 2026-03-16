using System;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace HasbroGo;

public class ChallengeMenu : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI challengeDisplayText;

	[SerializeField]
	private TMP_InputField inputField;

	private bool isCurrentlyChallenging;

	private bool isChallengeAccepted;

	private string playerDisplayName = string.Empty;

	private string challengerName = string.Empty;

	private const string testDeckType = "deck1";

	private void OnEnable()
	{
		inputField.onSubmit.AddListener(SendChallenge);
		SocialManager.Instance.GetProfileErrorEvent += OnDisplayNameError;
		SocialManager.Instance.SendChallengeErrorEvent += OnChallengeError;
	}

	private void OnDisable()
	{
		inputField.onSubmit.RemoveListener(SendChallenge);
		SocialManager.Instance.GetProfileErrorEvent -= OnDisplayNameError;
		SocialManager.Instance.SendChallengeErrorEvent -= OnChallengeError;
		ResetChallengeMenu();
	}

	private void LateUpdate()
	{
		if (isCurrentlyChallenging && isChallengeAccepted)
		{
			isCurrentlyChallenging = false;
			challengeDisplayText.text = challengerName + " Accepted";
		}
	}

	public void Init(string displayName)
	{
		inputField.text = displayName;
	}

	public void UpdateDisplayName(string playerName)
	{
		playerDisplayName = playerName;
	}

	public void ResetChallengeMenu()
	{
		isCurrentlyChallenging = false;
		isChallengeAccepted = false;
		challengerName = string.Empty;
		challengeDisplayText.text = "No challenger";
	}

	private void SendChallenge(string challengerDisplayName)
	{
		if (string.IsNullOrEmpty(challengerDisplayName))
		{
			challengeDisplayText.text = "Display name is empty";
		}
		else if (!isCurrentlyChallenging && !isChallengeAccepted)
		{
			challengerName = challengerDisplayName;
			isCurrentlyChallenging = true;
			challengeDisplayText.text = "Challenging " + challengerName;
			string message = JsonConvert.SerializeObject(new ChallengeSendData(playerDisplayName, "deck1"));
			SocialManager.Instance.SendChallengeGameMessage(challengerDisplayName, "ChallengeSendData", DateTime.Now.AddYears(1), message);
		}
	}

	private void SendChallengeConfirm()
	{
		string message = JsonConvert.SerializeObject(new ChallengeConfirmData(playerDisplayName, "deck1"));
		SocialManager.Instance.SendChallengeGameMessage(challengerName, "ChallengeConfirmData", DateTime.Now.AddYears(1), message);
	}

	public void HandleChallengeSendData(ChallengeSendData message)
	{
		if (message.DisplayName == challengerName && isCurrentlyChallenging)
		{
			isChallengeAccepted = true;
			SendChallengeConfirm();
		}
	}

	public void HandleChallengeConfirmData(ChallengeConfirmData message)
	{
		if (message.DisplayName == challengerName && isCurrentlyChallenging)
		{
			isChallengeAccepted = true;
		}
	}

	private void OnDisplayNameError(object sender, EventArgs e)
	{
		ResetChallengeMenu();
		ErrorEventArgs e2 = e as ErrorEventArgs;
		challengeDisplayText.text = $"{e2.ErrorCategory} not found";
	}

	private void OnChallengeError(object sender, EventArgs e)
	{
		ErrorEventArgs e2 = e as ErrorEventArgs;
		ResetChallengeMenu();
		challengeDisplayText.text = $"{e2.Error} during challenge";
	}
}
