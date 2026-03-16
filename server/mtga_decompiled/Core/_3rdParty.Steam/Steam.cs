using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Store;

namespace _3rdParty.Steam;

public static class Steam
{
	public enum AppId : uint
	{
		MTGA = 2141910u,
		WhisperingWoods = 2100820u
	}

	public enum SteamStatus
	{
		Uninitialized,
		Available,
		Unavailable
	}

	private class GetAuthToken : Promise<string>
	{
		public GetAuthToken()
		{
			Start();
			runAsync();
		}

		private async Task runAsync()
		{
			while (!LoggedIn)
			{
				await Task.Yield();
			}
			AuthTicket authTicket = await SteamUser.GetAuthSessionTicketAsync(SteamClient.SteamId);
			if (authTicket != null)
			{
				Complete(EncodeAuthTicket(authTicket));
			}
			else
			{
				SetError(new Error(-1, "Error retrieving steam AuthTicket"));
			}
		}
	}

	public const string USESTEAM_EDITOR_PREF = "UseSteam";

	public const string APPID_EDITOR_PREF = "AppId";

	private static SteamStatus _status;

	private static bool _determinedThatThisIsASteamBuild = false;

	private static bool _steamEnabled = false;

	private static uint _steamAppId = 0u;

	public static string OverrideIsoCurrencyCode = string.Empty;

	public static string OverrideIsoRegionCode = string.Empty;

	private static readonly Dictionary<string, string> steamLanguages = new Dictionary<string, string>
	{
		{ "arabic", "ar" },
		{ "bulgarian", "bg" },
		{ "schinese", "zh-CN" },
		{ "tchinese", "zh-TW" },
		{ "czech", "cs" },
		{ "danish", "da" },
		{ "dutch", "nl" },
		{ "english", "en" },
		{ "finnish", "fi" },
		{ "french", "fr" },
		{ "german", "de" },
		{ "greek", "el" },
		{ "hungarian", "hu" },
		{ "italian", "it" },
		{ "japanese", "ja" },
		{ "koreana", "ko" },
		{ "norwegian", "no" },
		{ "polish", "pl" },
		{ "portuguese", "pt" },
		{ "brazilian", "pt-BR" },
		{ "romanian", "ro" },
		{ "russian", "ru" },
		{ "spanish", "es" },
		{ "latam", "es-419" },
		{ "swedish", "sv" },
		{ "thai", "th" },
		{ "turkish", "tr" },
		{ "ukrainian", "uk" },
		{ "vietnamese", "vn" }
	};

	public static SteamStatus Status => _status;

	public static bool LoggedIn
	{
		get
		{
			if (Status == SteamStatus.Available)
			{
				return SteamClient.IsLoggedOn;
			}
			return false;
		}
	}

	public static string AuthToken { get; private set; }

	public static uint SteamAppId
	{
		get
		{
			DetermineIfThisIsASteamBuild();
			return _steamAppId;
		}
	}

	public static bool IsThisASteamBuild
	{
		get
		{
			DetermineIfThisIsASteamBuild();
			return _steamEnabled;
		}
	}

	private static void DetermineIfThisIsASteamBuild()
	{
		if (!_determinedThatThisIsASteamBuild)
		{
			uint steamAppId = 0u;
			bool flag = !Application.isMobilePlatform;
			if (!Application.isEditor)
			{
				StandaloneStoreConfig standaloneStoreConfig = Pantry.Get<StandaloneStoreConfig>();
				flag &= standaloneStoreConfig.DesiredStoreType == StandaloneStoreConfig.StandaloneStoreTypes.Steam;
				steamAppId = ((standaloneStoreConfig.DesiredAppId != 0) ? standaloneStoreConfig.DesiredAppId : 2141910u);
			}
			_steamEnabled = flag;
			_steamAppId = steamAppId;
			_determinedThatThisIsASteamBuild = true;
		}
	}

	public static void Init()
	{
		_status = SteamStatus.Uninitialized;
		if (!IsThisASteamBuild)
		{
			SimpleLog.LogForRelease("Skipping Steam Initialization");
			_status = SteamStatus.Unavailable;
			return;
		}
		try
		{
			SimpleLog.LogForRelease("Initializing Steam");
			SteamClient.Init(SteamAppId);
			_status = (SteamClient.IsValid ? SteamStatus.Available : SteamStatus.Unavailable);
		}
		catch (Exception ex)
		{
			SimpleLog.LogWarningForRelease(ex.Message);
			_status = SteamStatus.Unavailable;
			if (!Application.isEditor)
			{
				SimpleLog.LogWarningForRelease("Restarting via Steam");
				SteamClient.RestartAppIfNecessary(SteamAppId);
				Application.Quit();
			}
		}
		new GetAuthToken().IfSuccess(delegate(Promise<string> p)
		{
			AuthToken = p.Result;
			PromiseExtensions.Logger.Info("Steam AuthToken retrieved.");
		}).IfError(delegate(Promise<string> p)
		{
			PromiseExtensions.Logger.Error(p.Error.ToString());
		});
		SimpleLog.LogForRelease($"Steam status: {Status}");
	}

	public static void Shutdown()
	{
		if (Status == SteamStatus.Available)
		{
			SteamClient.Shutdown();
		}
		_status = SteamStatus.Uninitialized;
	}

	public static string EncodeAuthTicket(AuthTicket ticket)
	{
		byte[] data = ticket.Data;
		StringBuilder stringBuilder = new StringBuilder(data.Length * 2);
		byte[] array = data;
		foreach (byte b in array)
		{
			stringBuilder.Append($"{b:x2}");
		}
		return stringBuilder.ToString();
	}

	public static string GetIsoLanguageCode()
	{
		if (!steamLanguages.TryGetValue(SteamApps.GameLanguage, out var value))
		{
			SimpleLog.LogError("No language entry found for steam language " + SteamApps.GameLanguage);
			return "en";
		}
		return value;
	}

	public static string GetIsoCurrencyCode()
	{
		if (!string.IsNullOrWhiteSpace(OverrideIsoCurrencyCode))
		{
			return OverrideIsoCurrencyCode;
		}
		return SteamInventory.Currency;
	}

	public static string GetIsoRegionCode()
	{
		if (!string.IsNullOrWhiteSpace(OverrideIsoRegionCode))
		{
			return OverrideIsoRegionCode;
		}
		return SteamUtils.IpCountry;
	}
}
