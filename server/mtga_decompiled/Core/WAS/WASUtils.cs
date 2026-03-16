using Core.BI;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace WAS;

public static class WASUtils
{
	public static AccountError ToAccountError(Error error)
	{
		return ToAccountError(new WASHTTPClient.WASError(error.Code, error.Message ?? error.Exception.Message));
	}

	private static AccountError ToAccountError(WASHTTPClient.WASError error)
	{
		AccountError accountError = new AccountError();
		try
		{
			ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(error.Message);
			accountError.ErrorCode = errorResponse.code;
			accountError.ErrorMessage = errorResponse.error;
			PromiseExtensions.Logger.Info($"[Accounts - Client] Platform error: {errorResponse.code} | {errorResponse.error}");
			if (errorResponse.error.StartsWith("ACCOUNT ALREADY IN CONFLICT"))
			{
				accountError.ErrorType = AccountError.ErrorTypes.InConflict;
				return accountError;
			}
			switch (errorResponse.error)
			{
			case "INVALID DATE OF BIRTH":
			case "AGE REQUIREMENT":
				accountError.ErrorType = AccountError.ErrorTypes.Age;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Minimum_Age_Feedback");
				break;
			case "UPDATE REQUIRED":
				accountError.ErrorType = AccountError.ErrorTypes.UpdateRequired;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Upgraded_Account_Description");
				accountError.UpdateToken = errorResponse.update_token;
				break;
			case "ACCEPT TERMS AND CONDITIONS":
				accountError.ErrorType = AccountError.ErrorTypes.TermsConditions;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Accept_TC");
				break;
			case "PASSWORD NOT STRONG ENOUGH":
				accountError.ErrorType = AccountError.ErrorTypes.Password;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Password_Too_Simple");
				break;
			case "DISPLAY NAME LENGTH":
				accountError.ErrorType = AccountError.ErrorTypes.DisplayName;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/DisplayName_Length_Feedback");
				break;
			case "DISPLAY NAME TOO COMMON":
				accountError.ErrorType = AccountError.ErrorTypes.DisplayName;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/DisplayName_NotAvailable_Feedback");
				break;
			case "EMAIL ADDRESS IN USE":
				accountError.ErrorType = AccountError.ErrorTypes.Email;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Acccount_Registered_Email_Feedback");
				break;
			case "ACCOUNT NOT FOUND":
			case "INVALID ACCOUNT CREDENTIALS":
				accountError.ErrorType = AccountError.ErrorTypes.Email;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Invalid_Email_Password");
				break;
			case "PASSWORD RESET REQUIRED":
				accountError.ErrorType = AccountError.ErrorTypes.ResetPassword;
				break;
			case "FORBIDDEN DISPLAY NAME":
			case "DISPLAY NAME MODERATED":
			case "MODERATED":
				accountError.ErrorType = AccountError.ErrorTypes.DisplayName;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Display_Name_Moderated");
				break;
			case "VERIFY WIZARDS ACCOUNT":
				accountError.ErrorType = AccountError.ErrorTypes.NotVerified;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Email_Not_Verified");
				break;
			case "INVALID EMAIL ADDRESS":
				accountError.ErrorType = AccountError.ErrorTypes.Email;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Invalid_Email_Address");
				break;
			case "REFRESH TOKEN EXPIRED":
				accountError.ErrorType = AccountError.ErrorTypes.Token;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/TokenExpiredMessage");
				PromiseExtensions.Logger.InfoFormat("[Accounts - Client] It's been long enough ({0:d\\d\\:h\\h\\:m\\m}) since the user last played that they'll need to log in again.", MDNPlayerPrefs.Accounts_TimeSinceLastLogin);
				MDNPlayerPrefs.Accounts_LoggedOutReason = "Token Expired";
				BIEventType.LoginFailure.SendWithDefaults(("Type", "Token Expiry"), ("TimeSinceLastLogin", MDNPlayerPrefs.Accounts_TimeSinceLastLogin.ToString()));
				LoginFlowAnalytics.SendEvent_Login("tokenExpire_begin");
				break;
			case "INVALID SOCIAL CREDENTIALS":
				accountError.ErrorType = AccountError.ErrorTypes.Token;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/InvalidSocialCredentials");
				break;
			case "SOCIAL IDENTITY IN USE":
				accountError.ErrorType = AccountError.ErrorTypes.Token;
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/SocialIdentityInUse");
				break;
			case "ACCOUNT ALREADY HAS PERSONA FOR GAME":
				accountError.ErrorType = AccountError.ErrorTypes.InConflict;
				break;
			case "SOCIAL ACCOUNT ALREADY LINKED":
				accountError.ErrorType = AccountError.ErrorTypes.AlreadyLinked;
				break;
			default:
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Default_Error_Message", ("code", errorResponse.error));
				PromiseExtensions.Logger.ErrorFormat("[Accounts - Client] Unhandled account error occurred:\n{0}\n{1}", errorResponse.code.ToString(), errorResponse.error);
				BIEventType.LoginFailure.SendWithDefaults(("Type", "Unhandled Wizards Account Error that should be properly handled by the client."), ("ErrorCode", errorResponse.code.ToString()), ("ErrorMessage", errorResponse.error));
				break;
			}
		}
		catch
		{
			if (error.Code == 504)
			{
				accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Timeout");
			}
			else
			{
				accountError.LocalizedErrorMessage = error.Message;
			}
			accountError.ErrorCode = error.Code;
			accountError.LocalizedErrorMessage = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Default_Error_Message", ("code", error.Code.ToString()));
		}
		return accountError;
	}
}
