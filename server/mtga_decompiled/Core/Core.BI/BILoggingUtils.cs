using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Win32;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;

namespace Core.BI;

public static class BILoggingUtils
{
	private static readonly string appSessionId = Guid.NewGuid().ToString();

	private static IAccountClient _accountClient;

	private static string _supplementalInstallId = string.Empty;

	private static IAccountClient AccountClient => _accountClient ?? (_accountClient = Pantry.Get<IAccountClient>());

	public static string PersonaID => AccountClient?.AccountInformation?.PersonaID;

	public static string AccountId => AccountClient?.AccountInformation?.AccountID;

	public static string InstallId
	{
		get
		{
			try
			{
				if (Registry.LocalMachine != null)
				{
					using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Wizards of the Coast\\", writable: false);
					object obj = registryKey?.GetValue("userId");
					if (obj != null)
					{
						return obj as string;
					}
				}
			}
			catch (Exception arg)
			{
				SimpleLog.LogError($"[BILoggingUtils] -- Exception thrown when trying to get install ID key. {arg}");
			}
			return _supplementalInstallId;
		}
	}

	public static void SendWithDefaults(this BIEventType eventType, params (string, string)[] data)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (data != null)
		{
			for (int i = 0; i < data.Length; i++)
			{
				(string, string) tuple = data[i];
				dictionary[tuple.Item1] = tuple.Item2;
			}
		}
		dictionary["RegionId"] = RegionInfo.CurrentRegion.ThreeLetterISORegionName;
		dictionary["InstallId"] = InstallId;
		dictionary["PersonaId"] = (string.IsNullOrEmpty(PersonaID) ? InstallId : PersonaID);
		dictionary["AccountId"] = (string.IsNullOrEmpty(AccountId) ? InstallId : AccountId);
		dictionary["ClientPlatform"] = PlatformUtils.GetClientPlatform();
		dictionary["AppSessionId"] = appSessionId;
		Pantry.Get<IBILogger>().Send(ClientBusinessEventType.Generic, new ClientBusinessEvent
		{
			EventName = eventType.ToString(),
			EventTime = DateTime.UtcNow,
			Data = dictionary
		});
	}

	public static void SetSupplementalInstallID(string supplementalInstallID)
	{
		_supplementalInstallId = supplementalInstallID;
	}
}
