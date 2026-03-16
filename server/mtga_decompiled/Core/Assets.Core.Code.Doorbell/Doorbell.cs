using System;
using System.Collections.Generic;
using Core.Code.ClientFeatureToggle;
using Core.Shared.Code.Providers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Assets;

namespace Assets.Core.Code.Doorbell;

public class Doorbell
{
	private class DoorbellServiceResponse
	{
		public string fdURI;

		public string contentHash;

		public string[] preloadContentHashes;

		public JObject configurationRoot = new JObject();
	}

	private class DoorbellRequest
	{
		public string playerId;

		public string clientVersion;

		public string environmentKey;

		public string platformKey;
	}

	private class DoorbellRequestV2
	{
		public string code;

		public string environment;

		public string clientVersion;

		public string platform;

		public string playerId;
	}

	private const string NonProdDoorCode = "ta4kBQcrBfdGd8AUjrv7lj9pYyA3Kkj9p39byJXuTdTBiZxRC6xgRQ==";

	private const string ProdDoorCode = "46u7OAmyEZ6AtfgaPUHiXNiC55/mrtp3aAmE018KZamDhvr0vZ8mxg==";

	public static Promise<DoorbellRingResponseV2> RingDoorbell(EnvironmentDescription environmentDescription, IClientVersionInfo version, string playerId)
	{
		bool useV2 = Pantry.Get<ClientFeatureToggleDataProvider>().GetToggleValueById("RingDoorbellV2");
		if (MDNPlayerPrefs.DoorbellOverrideToggle && Debug.isDebugBuild)
		{
			string doorbellOverrideContent = MDNPlayerPrefs.DoorbellOverrideContent;
			if (!string.IsNullOrEmpty(doorbellOverrideContent))
			{
				return new SimplePromise<DoorbellRingResponseV2>(ProcessDoorbellResponse(doorbellOverrideContent, useV2));
			}
		}
		string text = EnvironmentManager.GetDoorbellUri(environmentDescription);
		string text2 = (text.Contains("w2.mtgarena.com") ? "46u7OAmyEZ6AtfgaPUHiXNiC55/mrtp3aAmE018KZamDhvr0vZ8mxg==" : "ta4kBQcrBfdGd8AUjrv7lj9pYyA3Kkj9p39byJXuTdTBiZxRC6xgRQ==");
		string text3 = null;
		if (useV2)
		{
			text3 = JsonConvert.SerializeObject(new DoorbellRequestV2
			{
				code = text2,
				clientVersion = version.ContentVersion.ToString(3),
				environment = environmentDescription.name,
				playerId = playerId,
				platform = version.Platform
			});
		}
		else
		{
			text3 = JsonConvert.SerializeObject(new DoorbellRequest
			{
				clientVersion = version.ContentVersion.ToString(3),
				environmentKey = environmentDescription.name,
				playerId = playerId,
				platformKey = version.Platform
			});
			text = text + "?code=" + text2;
		}
		string text4 = "Ringing Doorbell at " + text;
		if (Application.isEditor || Application.isBatchMode)
		{
			text4 = text4 + Environment.NewLine + "RequestBody: " + text3;
		}
		PromiseExtensions.Logger.Info(text4);
		return WebPromise.PostJson(text, new Dictionary<string, string>(), text3).Then(delegate(Promise<string> p)
		{
			PromiseExtensions.Logger.Info("Doorbell response: " + p.Result);
		}).Convert((string response) => ProcessDoorbellResponse(response, useV2));
	}

	private static DoorbellRingResponseV2 ProcessDoorbellResponse(string responseText, bool useV2)
	{
		if (useV2)
		{
			return JsonConvert.DeserializeObject<DoorbellRingResponseV2>(responseText);
		}
		DoorbellServiceResponse doorbellServiceResponse = JsonConvert.DeserializeObject<DoorbellServiceResponse>(responseText);
		List<AssetBundleManifestMetadata> list = new List<AssetBundleManifestMetadata>();
		if (!string.IsNullOrWhiteSpace(doorbellServiceResponse.contentHash))
		{
			list.Add(new AssetBundleManifestMetadata(AssetPriority.General, string.Empty, doorbellServiceResponse.contentHash));
		}
		string[] preloadContentHashes = doorbellServiceResponse.preloadContentHashes;
		if (preloadContentHashes != null && preloadContentHashes.Length != 0)
		{
			string[] preloadContentHashes2 = doorbellServiceResponse.preloadContentHashes;
			foreach (string text in preloadContentHashes2)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					list.Add(new AssetBundleManifestMetadata(AssetPriority.Future, string.Empty, text));
				}
			}
		}
		return new DoorbellRingResponseV2
		{
			FdURI = doorbellServiceResponse.fdURI,
			BundleManifests = list
		};
	}
}
