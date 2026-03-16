using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WAS;

public static class JwtHandler
{
	private const string RolesFieldName = "wotc-rols";

	private const string FlagsFieldName = "wotc-flgs";

	public static string[] GetRolesFromAccessToken(string accessToken)
	{
		if (string.IsNullOrEmpty(accessToken))
		{
			throw new ArgumentException("accessToken can not be null");
		}
		string[] array = accessToken.Split('.');
		if (array.Length != 3)
		{
			throw new FormatException("accessToken is malformed");
		}
		Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(DecodeBytes(array[1])));
		if (!dictionary.ContainsKey("wotc-rols"))
		{
			return new string[0];
		}
		return ToRoles(dictionary["wotc-rols"]);
	}

	public static bool GetEmailVerifiedFromAccessToken(string accessToken)
	{
		if (string.IsNullOrEmpty(accessToken))
		{
			throw new ArgumentException("accessToken can not be null");
		}
		string[] array = accessToken.Split('.');
		if (array.Length != 3)
		{
			throw new FormatException("accessToken is malformed");
		}
		Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(DecodeBytes(array[1])));
		WotcFlags wotcFlags = (dictionary.ContainsKey("wotc-flgs") ? ((WotcFlags)int.Parse(dictionary["wotc-flgs"].ToString())) : WotcFlags.None);
		return wotcFlags.HasFlag(WotcFlags.EmailVerified);
	}

	private static string[] ToRoles(object roles)
	{
		return ((JArray)roles).Select((JToken t) => t.ToString()).ToArray();
	}

	public static byte[] DecodeBytes(string str)
	{
		char c = '=';
		string text = "==";
		char newChar = '+';
		char newChar2 = '/';
		char oldChar = '-';
		char oldChar2 = '_';
		str = str.Replace(oldChar, newChar);
		str = str.Replace(oldChar2, newChar2);
		switch (str.Length % 4)
		{
		case 2:
			str += text;
			break;
		case 3:
			str += c;
			break;
		default:
		{
			object[] args = new object[1] { str };
			throw new FormatException(string.Format("Unable to decode: '{0}' as Base64url encoded string.", args));
		}
		case 0:
			break;
		}
		return Convert.FromBase64String(str);
	}
}
