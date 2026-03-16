using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Core.Meta.Utilities;
using Core.Code.Promises;
using Core.Shared.Code.Connection;
using UnityEngine;
using WAS;
using Wizards.Arena.Promises;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code;

public static class TokenManager
{
	public static void ValidateToken(IAccountClient accountClient, IFrontDoorConnectionServiceWrapper fd, FrontDoorConnectionManager frontDoorConnectionManager, bool inDuelOrNPE)
	{
		if (accountClient.CurrentLoginState != LoginState.FullyRegisteredLogin)
		{
			return;
		}
		AccountInformation accountInformation = accountClient.AccountInformation;
		if (accountInformation == null)
		{
			return;
		}
		switch (accountInformation.CredentialsState)
		{
		case TokenState.Valid:
			if (accountInformation.Credentials.ExpiresAt - 30 < DateTimeOffset.Now.ToUnixTimeSeconds())
			{
				accountInformation.CredentialsState = TokenState.Refreshing;
				PAPA.StartGlobalCoroutine(Coroutine_RefreshAccessToken(1, accountClient, fd, inDuelOrNPE));
			}
			break;
		case TokenState.Invalid:
			if (!inDuelOrNPE)
			{
				frontDoorConnectionManager.LogoutAndRestartGame("Error refreshing access token.");
			}
			break;
		case TokenState.Expired:
			if (!inDuelOrNPE)
			{
				accountInformation.CredentialsState = TokenState.WaitingToRefresh;
				ShowRefreshTokenFailedMessage(2, accountClient, fd, inDuelOrNPE);
			}
			break;
		case TokenState.Refreshing:
		case TokenState.WaitingToRefresh:
			break;
		}
	}

	private static IEnumerator Coroutine_RefreshAccessToken(int attempt, IAccountClient accountClient, IFrontDoorConnectionServiceWrapper fd, bool inDuelOrNPE)
	{
		Debug.Log("[Accounts - SharedContextView] Refreshing access token.");
		yield return accountClient.RefreshAccessToken().IfSuccess(delegate
		{
			if (accountClient.AccountInformation != null)
			{
				PromiseExtensions.Logger.Info("[Accounts - SharedContextView] Access token refreshed.");
				accountClient.AccountInformation.CredentialsState = TokenState.Valid;
				fd.SessionTicket = accountClient.AccountInformation.Credentials.Jwt;
			}
		}).ThenOnMainThreadIfError(delegate(Error e)
		{
			OnRefreshError(attempt, accountClient, fd, inDuelOrNPE, WASUtils.ToAccountError(e));
		})
			.AsCoroutine();
	}

	private static void OnRefreshError(int attempt, IAccountClient accountClient, IFrontDoorConnectionServiceWrapper fd, bool inDuelOrNPE, AccountError error)
	{
		Debug.Log("[Accounts - SharedContextView] Error refreshing access token. Attempt " + attempt + ". Error: " + error.ErrorCode + " | " + error.ErrorMessage);
		AccountInformation accountInformation = accountClient.AccountInformation;
		if (accountInformation != null && accountInformation.CredentialsState != TokenState.Invalid)
		{
			if (attempt <= 1)
			{
				PAPA.StartGlobalCoroutine(Coroutine_RefreshAccessToken(attempt + 1, accountClient, fd, inDuelOrNPE));
				return;
			}
			if (inDuelOrNPE)
			{
				accountInformation.CredentialsState = TokenState.Expired;
				return;
			}
			accountInformation.CredentialsState = TokenState.WaitingToRefresh;
			ShowRefreshTokenFailedMessage(attempt + 1, accountClient, fd, inDuelOrNPE);
		}
	}

	private static void ShowRefreshTokenFailedMessage(int attempt, IAccountClient accountClient, IFrontDoorConnectionServiceWrapper fd, bool inDuelOrNPE)
	{
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Connection_Lost_Title");
		string localizedText2 = Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Connection_Lost_Text");
		SystemMessageManager.SystemMessageButtonData item = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/EscapeMenu/CheckStatus"),
			Callback = delegate
			{
				UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/StatusPage"));
			},
			HideOnClick = false
		};
		SystemMessageManager.SystemMessageButtonData item2 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Connection_Lost_Reconnect_Button"),
			Callback = delegate
			{
				PAPA.StartGlobalCoroutine(Coroutine_RefreshAccessToken(attempt, accountClient, fd, inDuelOrNPE));
			}
		};
		SystemMessageManager.SystemMessageButtonData item3 = new SystemMessageManager.SystemMessageButtonData
		{
			Text = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/EscapeMenu/Exit_Button_Text"),
			Callback = SceneLoader.ApplicationQuit
		};
		List<SystemMessageManager.SystemMessageButtonData> list = new List<SystemMessageManager.SystemMessageButtonData>();
		list.Add(item);
		list.Add(item2);
		if (!PlatformUtils.IsHandheld())
		{
			list.Add(item3);
		}
		SystemMessageManager.Instance.ShowMessage(localizedText, localizedText2, list);
	}
}
