using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using _3rdParty.Steam;

namespace Wizards.Mtga.Store;

public class StandaloneStoreConfig
{
	public enum StandaloneStoreTypes
	{
		Inactive,
		Xsolla,
		Steam
	}

	public class ConfigFile
	{
		public string Provider;

		public string Id;
	}

	private const string StoreIdXsolla = "c660eeec-724b-4384-b6a8-2044e1a15d52";

	private const string StoreIdSteam = "468b107d-3616-406f-b38e-ff6bd3aba289";

	private const string ConfigFilename = "StoreConfig.txt";

	public StandaloneStoreTypes DesiredStoreType { get; private set; }

	public uint DesiredAppId { get; private set; }

	public StandaloneStoreConfig()
	{
		string text = Path.Combine(Application.streamingAssetsPath, "StoreConfig.txt");
		if (File.Exists(text))
		{
			ConfigFile configFile = null;
			try
			{
				configFile = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(text));
			}
			catch (Exception ex)
			{
				SimpleLog.LogFormatForRelease("Exception encountered when loading StoreConfig: {0}", ex.Message);
				return;
			}
			if (configFile.Provider == "c660eeec-724b-4384-b6a8-2044e1a15d52")
			{
				DesiredStoreType = StandaloneStoreTypes.Xsolla;
			}
			else if (configFile.Provider == "468b107d-3616-406f-b38e-ff6bd3aba289")
			{
				DesiredStoreType = StandaloneStoreTypes.Steam;
				if (!string.IsNullOrEmpty(configFile.Id))
				{
					if (uint.TryParse(configFile.Id, out var result) && Enum.IsDefined(typeof(Steam.AppId), result))
					{
						DesiredAppId = result;
						return;
					}
					SimpleLog.LogErrorFormat("Unrecognized AppId found when parsing StoreConfig: {0}", configFile.Id);
				}
			}
			else
			{
				SimpleLog.LogErrorFormat("Unrecognized Provider found when parsing StoreConfig: {0}", configFile.Provider);
			}
		}
		else if (Application.isEditor)
		{
			DesiredStoreType = StandaloneStoreTypes.Xsolla;
		}
		else
		{
			SimpleLog.LogErrorFormat("No StoreConfig found at path {0}, store will be inactive", text);
		}
	}
}
