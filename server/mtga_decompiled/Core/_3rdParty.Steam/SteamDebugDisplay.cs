using System;
using System.Reflection;
using Core.Code.ClientFeatureToggle;
using Core.Code.Promises;
using Core.Shared.Code.Network;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using WAS;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Login;

namespace _3rdParty.Steam;

public static class SteamDebugDisplay
{
	private static ConflictingPersonas conflict;

	public static void RenderDebugUI(DebugInfoIMGUIOnGui gui)
	{
		gui.ShowLabelInfo("Steam status:", $"{Steam.Status}");
		if (Steam.Status == Steam.SteamStatus.Unavailable)
		{
			return;
		}
		gui.ShowLabelInfo("Logged into steam:", $"{Steam.LoggedIn}");
		gui.ShowLabelInfo("SteamID:", $"{SteamClient.SteamId}");
		gui.ShowLabelInfo("Steam AuthToken:", Steam.AuthToken ?? "");
		gui.ShowLabelInfo("Steam Username", SteamClient.Name ?? "");
		gui.ShowLabelInfo("Steam Currency Code", SteamInventory.Currency ?? "");
		gui.ShowLabelInfo("Steam Region Code", SteamUtils.IpCountry ?? "");
		Steam.OverrideIsoCurrencyCode = gui.ShowInputField("Currency Code Override", Steam.OverrideIsoCurrencyCode);
		Steam.OverrideIsoRegionCode = gui.ShowInputField("Region Code Override", Steam.OverrideIsoRegionCode);
		if (!Pantry.Get<ClientFeatureToggleDataProvider>().GetToggleValueById("SteamSocialAccounts"))
		{
			return;
		}
		if (SceneManager.GetActiveScene().name.Equals("Login"))
		{
			if (gui.ShowDebugButton("Register with Steam (uses login panel for email)", 500f))
			{
				RegisterWithSteam();
			}
			if (gui.ShowDebugButton("Log In with Steam", 500f))
			{
				LogInWithSteam();
			}
		}
		else
		{
			if (!SceneManager.GetActiveScene().name.Equals("MainNavigation"))
			{
				return;
			}
			if (conflict == null)
			{
				Pantry.Get<IAccountClient>();
				if (gui.ShowDebugButton("Link Steam Account", 500f))
				{
					LinkSteamAccount();
				}
				if (gui.ShowDebugButton("Get Conflict Info", 500f))
				{
					GetLinkConflictInfo();
				}
			}
			else
			{
				if (gui.ShowDebugButton("Resolve Conflict with " + conflict.conflictingPersonas[0].displayName, 500f))
				{
					ResolveWith(conflict.conflictingPersonas[0], conflict.conflictingPersonas[1]);
				}
				if (gui.ShowDebugButton("Resolve Conflict with " + conflict.conflictingPersonas[1].displayName, 500f))
				{
					ResolveWith(conflict.conflictingPersonas[1], conflict.conflictingPersonas[0]);
				}
			}
			if (gui.ShowDebugButton("Cancel Linking", 500f))
			{
				CancelLink();
			}
			if (gui.ShowDebugButton("Get Linked Accounts", 500f))
			{
				GetLinkedAccounts();
			}
		}
	}

	private static void GetLinkConflictInfo()
	{
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		IConflictingAccountsServiceWrapper conflictServiceWrapper = Pantry.Get<IConflictingAccountsServiceWrapper>();
		accountClient.GetLinkConflictInfo().IfSuccess(delegate(Promise<ConflictingPersonas> p)
		{
			conflict = p.Result;
			PromiseExtensions.Logger.Info(JsonConvert.SerializeObject(conflict) ?? "");
			conflictServiceWrapper.GetConflictingAccountsData(conflict).IfSuccess(delegate(Promise<Client_ConflictingAccounts> response)
			{
				PromiseExtensions.Logger.Info(JsonConvert.SerializeObject(response.Result) ?? "");
			});
		}).IfError(HandleError);
	}

	private static void ResolveWith(ConflictingPersona personaToKeep, ConflictingPersona personaToDiscard)
	{
		Pantry.Get<IAccountClient>().ResolveLinkConflict(personaToKeep, personaToDiscard).IfSuccess(delegate
		{
			conflict = null;
			PromiseExtensions.Logger.Info("Resolved with " + personaToKeep.displayName);
		})
			.IfError(HandleError);
	}

	private static void LinkSteamAccount()
	{
		Pantry.Get<IAccountClient>().LinkSocialAccount().IfSuccess(delegate
		{
			PromiseExtensions.Logger.Info("Successfully linked steam account.");
		})
			.IfError(HandleError);
	}

	private static void CancelLink()
	{
		Pantry.Get<IAccountClient>().CancelAccountLinking().IfSuccess(delegate
		{
			PromiseExtensions.Logger.Info("Accounts " + conflict.conflictingPersonas[0].displayName + " and " + conflict.conflictingPersonas[1].displayName + " unlinked.");
			conflict = null;
		})
			.IfError(HandleError);
	}

	private static void RegisterWithSteam()
	{
		LoginScene loginScene = UnityEngine.Object.FindObjectOfType<LoginScene>();
		LoginPanel obj = UnityEngine.Object.FindObjectOfType<LoginPanel>();
		string text = ((UIWidget_InputField_Registration)(typeof(LoginPanel).GetField("_email_inputField", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj)))?.InputField.text;
		if (string.IsNullOrWhiteSpace(text))
		{
			Debug.LogError("Email must be set. Use the field in the login screen.");
			return;
		}
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		accountClient.RegisterAsSocialAccount(text, SteamClient.Name, receiveOffersOptIn: true, dataShareOptIn: true, loginScene.Birthday, loginScene.SelectedCountry).ThenOnMainThreadIfSuccess((Action<CreateUserResponse>)delegate
		{
			loginScene.ConnectToFrontDoor(accountClient.AccountInformation);
		}).IfError(HandleError);
	}

	private static void LogInWithSteam()
	{
		LoginScene loginScene = UnityEngine.Object.FindObjectOfType<LoginScene>();
		IAccountClient accountClient = Pantry.Get<IAccountClient>();
		accountClient.SocialLogin_Fast(manualLogin: true).ThenOnMainThreadIfSuccess((Action<AccountInformation>)delegate
		{
			loginScene.ConnectToFrontDoor(accountClient.AccountInformation);
		}).IfError(HandleError);
	}

	private static void GetLinkedAccounts()
	{
		Pantry.Get<IAccountClient>().GetLinkedAccounts().Then(delegate(Promise<SocialIdentities> p)
		{
			PromiseExtensions.Logger.Info(JsonConvert.SerializeObject(p.Result) ?? "");
		})
			.IfError(HandleError);
	}

	private static void HandleError(Error e)
	{
		PromiseExtensions.Logger.Error($"{e}");
		WASUtils.ToAccountError(e);
	}
}
