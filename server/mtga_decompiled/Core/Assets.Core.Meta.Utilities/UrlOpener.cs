using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Deeplink;

namespace Assets.Core.Meta.Utilities;

public static class UrlOpener
{
	private const string UtmMedium = "utm_medium";

	private const string UtmMediumValue = "product";

	private const string UtmSource = "utm_source";

	private const string UtmSourceValue = "arena";

	private const string MagicDomain = "magic.wizards.com";

	private static Dictionary<string, string> MagicDomainTrackingValues = new Dictionary<string, string>
	{
		{ "utm_medium", "product" },
		{ "utm_source", "arena" }
	};

	private static Dictionary<string, Func<string>> KnownTokens = new Dictionary<string, Func<string>> { { "{BrazeId}", GetBrazeId } };

	private static List<string> KnownTokenDelimiters = new List<string> { "{", "}" };

	public static void OpenURL(string url)
	{
		url = GetProcessedUrl(url);
		if (!url.StartsWith("unitydl://") || !(WrapperController.Instance != null) || !DeepLinking.TryNavigateViaUrl(url, WrapperController.Instance, Pantry.Get<IBILogger>()))
		{
			Application.OpenURL(url);
		}
	}

	public static string GetProcessedUrl(string url)
	{
		UriBuilder uriBuilder = null;
		try
		{
			uriBuilder = new UriBuilder(url);
		}
		catch (Exception ex)
		{
			SimpleLog.LogPreProdError(ex.Message);
		}
		try
		{
			if (uriBuilder != null)
			{
				AddTrackingInfoToUrl(uriBuilder);
				SubstituteUriTokens(uriBuilder);
				url = uriBuilder.Uri.ToString();
			}
			CheckIsWellFormattedUri(url);
		}
		catch (Exception ex2)
		{
			SimpleLog.LogPreProdError(ex2.Message);
		}
		return url;
	}

	private static void AddTrackingInfoToUrl(UriBuilder uriBuilder)
	{
		if (UrlGoesToDomain(uriBuilder, "magic.wizards.com"))
		{
			EnsureParamsAreAddedToURL(uriBuilder, MagicDomainTrackingValues);
		}
	}

	public static bool UrlGoesToDomain(UriBuilder uriBuilder, string domain)
	{
		return uriBuilder.Host == domain;
	}

	public static void EnsureParamsAreAddedToURL(UriBuilder uriBuilder, Dictionary<string, string> paramsToAdd)
	{
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
		foreach (KeyValuePair<string, string> item in paramsToAdd)
		{
			if (string.IsNullOrEmpty(nameValueCollection.Get(item.Key)))
			{
				nameValueCollection.Add(item.Key, item.Value);
			}
		}
		uriBuilder.Query = nameValueCollection.ToString();
	}

	public static void SubstituteUriTokens(UriBuilder uriBuilder)
	{
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
		NameValueCollection nameValueCollection2 = HttpUtility.ParseQueryString("");
		if (nameValueCollection.AllKeys != null)
		{
			string[] allKeys = nameValueCollection.AllKeys;
			foreach (string name in allKeys)
			{
				string paramValue = nameValueCollection[name];
				paramValue = SubstituteUriTokensInParam(uriBuilder, paramValue);
				if (!string.IsNullOrEmpty(paramValue))
				{
					nameValueCollection2.Add(name, paramValue);
				}
			}
		}
		uriBuilder.Query = nameValueCollection2.ToString();
	}

	private static string SubstituteUriTokensInParam(UriBuilder uriBuilder, string paramValue)
	{
		foreach (KeyValuePair<string, Func<string>> knownToken in KnownTokens)
		{
			if (paramValue.Contains(knownToken.Key))
			{
				string text = knownToken.Value();
				if (string.IsNullOrEmpty(text))
				{
					return null;
				}
				paramValue = paramValue.Replace(knownToken.Key, text);
			}
		}
		foreach (string knownTokenDelimiter in KnownTokenDelimiters)
		{
			if (paramValue.Contains(knownTokenDelimiter))
			{
				SimpleLog.LogWarningForRelease("UrlOpener.SubstituteUriTokensInParam: URL (" + uriBuilder.Uri.ToString() + ") contains extra \"" + knownTokenDelimiter + "\" delimiter");
				return null;
			}
		}
		return paramValue;
	}

	public static void CheckIsWellFormattedUri(string url)
	{
		if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
		{
			SimpleLog.LogWarningForRelease("UrlOpener.CheckIsWellFormattedUri: \"" + url + "\" is not a well formed Uri");
		}
	}

	private static string GetBrazeId()
	{
		IAccountClient accountClient;
		try
		{
			accountClient = Pantry.Get<IAccountClient>();
			if (accountClient == null)
			{
				SimpleLog.LogWarningForRelease("UrlOpener.GetBrazeId: Unable to retrieve IAccountClient");
				return null;
			}
		}
		catch (Exception ex)
		{
			SimpleLog.LogPreProdError(ex.Message);
			return null;
		}
		string obj = accountClient.AccountInformation?.ExternalID;
		if (string.IsNullOrEmpty(obj))
		{
			SimpleLog.LogWarningForRelease("UrlOpener.GetBrazeId: Unable to retrieve BrazeId for player: " + (accountClient.AccountInformation?.PersonaID ?? "Unknown Player"));
		}
		return obj;
	}
}
