using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;

namespace Wizards.Mtga.Store;

public static class StoreUtils
{
	public const string MsgConfirmed = "Confirmed";

	public const string MsgPending = "Pending";

	public const string MsgSuccess = "Success";

	public static bool MultiPurchasing = true;

	private const string EURO_SYMBOL = "€";

	private const string CURRENCY_SIGN = "¤";

	private const string USD_SYMBOL = "$";

	private const string EUR = "EUR";

	private const string USD = "USD";

	private const string JPY = "JPY";

	private static readonly Dictionary<string, CultureInfo> IsoCodesToCurrencyCulture = new Dictionary<string, CultureInfo>();

	public static readonly string[] XsollaSupportedCurrencyCodes = new string[3] { "USD", "EUR", "JPY" };

	public static void ForcedCrash(Action<string> log)
	{
		log?.Invoke("Forcing Crash");
		Application.ForceCrash(0);
	}

	public static string GetLocalizedRmtPriceString(float price, string currency)
	{
		return price.ToString("C", GetCurrencyCultureFromIsoCode(currency));
	}

	private static CultureInfo GetCurrencyCultureFromIsoCode(string isoCurrencyCode)
	{
		if (!IsoCodesToCurrencyCulture.TryGetValue(isoCurrencyCode, out var value))
		{
			value = AttemptGetCommonCultureInfoForIsoCode(isoCurrencyCode);
			if (value == null)
			{
				value = AttemptGetCultureInfoForIsoCodeFromRegionInfo(isoCurrencyCode);
			}
			if (value == null)
			{
				value = SimpleCurrencyCulture("¤");
			}
			IsoCodesToCurrencyCulture[isoCurrencyCode] = value;
		}
		return value;
	}

	private static CultureInfo AttemptGetCommonCultureInfoForIsoCode(string isoCurrencyCode)
	{
		string text = ((isoCurrencyCode == "USD") ? "$" : ((!(isoCurrencyCode == "EUR")) ? null : "€"));
		string text2 = text;
		if (text2 == null)
		{
			return null;
		}
		return SimpleCurrencyCulture(text2);
	}

	private static CultureInfo AttemptGetCultureInfoForIsoCodeFromRegionInfo(string isoCurrencyCode)
	{
		CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
		foreach (CultureInfo cultureInfo in cultures)
		{
			RegionInfo regionInfo;
			try
			{
				regionInfo = new RegionInfo(cultureInfo.Name);
			}
			catch
			{
				continue;
			}
			if (regionInfo.ISOCurrencySymbol == isoCurrencyCode)
			{
				NumberFormatInfo numberFormat = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
				return new CultureInfo(cultureInfo.Name)
				{
					NumberFormat = numberFormat
				};
			}
		}
		return null;
	}

	private static CultureInfo SimpleCurrencyCulture(string currencySymbol)
	{
		return new CultureInfo(Thread.CurrentThread.CurrentCulture.Name)
		{
			NumberFormat = 
			{
				CurrencySymbol = currencySymbol,
				CurrencyDecimalDigits = 2
			}
		};
	}

	public static string ValidatedIsoCurrencyCode(string currencyCode)
	{
		if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
		{
			return null;
		}
		return currencyCode;
	}

	public static string GetStoreCurrencySelection(this IAccountClient accountClient)
	{
		return CurrencyCodeForCountry(accountClient?.AccountInformation?.CountryCode);
	}

	public static string CurrencyCodeForCountry(string countryCode)
	{
		if (CountryCodes.IsCountryInRegion(CountryCodes.Regions.Europe, countryCode))
		{
			return "EUR";
		}
		if (countryCode == "JP")
		{
			return "JPY";
		}
		return "USD";
	}
}
