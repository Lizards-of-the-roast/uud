using Core.BI;
using WAS;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;

public static class LoginFlowAnalytics
{
	public enum LoginStyleType
	{
		Automatic,
		LoginPage
	}

	public const string BLCBegin = "blc_begin";

	public const string BLCMonthChosen = "blc_month_chosen";

	public const string BLCDayChosen = "blc_day_chosen";

	public const string BLCYearChosen = "blc_year_chosen";

	public const string BLCCountryChosen = "blc_country_chosen";

	public const string BLCSuccess = "blc_success";

	public const string BLCExperiencePressed = "blc_experience_pressed";

	public const string BLCExperienceChosen = "blc_experience_chosen";

	public const string LoginBegin = "login_begin";

	public const string LoginEmailEntered = "login_email_entered";

	public const string LoginPasswordEntered = "login_password_entered";

	public const string LoginAttempted = "login_attempted";

	public const string LoginSuccess = "login_success";

	public const string ForgotPasswordAttempted = "forgotPassword_attempted";

	public const string ForgotPasswordSuccess = "forgotPassword_success";

	public const string RegistrationBegin = "registration_begin";

	public const string RegistrationDisplayNameAttempted = "registration_displayName_attempted";

	public const string RegistrationDisplayNameSuccess = "registration_displayName_success";

	public const string RegistrationEmail1Entered = "registration_email1_entered";

	public const string RegistrationEmail2Entered = "registration_email2_entered";

	public const string RegistrationPassword1Entered = "registration_password1_entered";

	public const string RegistrationPassword2Entered = "registration_password2_entered";

	public const string RegistrationAttempted = "registration_attempted";

	public const string RegistrationSuccessfulPasswordEntered = "registration_password_success";

	public const string RegistrationSuccess = "registration_success";

	public const string TokenExpireBegin = "tokenExpire_begin";

	public static void SendEvent_Registration(string stepName, bool isAccountUpdate = false)
	{
		BIEventType.RegistrationBI.SendWithDefaults(("Step", stepName), ("IsAccountUpdate", isAccountUpdate.ToString()));
	}

	public static void SendEvent_Registration_Completed()
	{
		BIEventType.RegistrationBI.SendWithDefaults(("Step", "registration_success"));
	}

	public static void SendEvent_Login(string stepName)
	{
		BIEventType.LoginBI.SendWithDefaults(("Step", stepName));
	}

	public static void SendEvent_LoginCompleted()
	{
		BIEventType.LoginBI.SendWithDefaults(("Step", "login_success"));
	}

	public static void SendEvent_AttemptLogin(LoginAction action, ILoginContext context)
	{
		BIEventType.AttemptLogin.SendWithDefaults(("Action", action.ToString()), ("Type", context.SocialType));
		BIEventType.LoginStyle.SendWithDefaults(("Style", (action == LoginAction.Automatic) ? "Automatic" : "LoginPage"));
	}

	public static void SendEvent_SocialAccountLinked(ILoginContext context)
	{
		BIEventType.SocialAccountLinked.SendWithDefaults(("Type", context.SocialType));
	}

	public static void SendEvent_AccountConflictResolved(ILoginContext context, ConflictingPersona personaToKeep, ConflictingPersona personaToDiscard)
	{
		BIEventType.AccountConflictResolved.SendWithDefaults(("Type", context.SocialType), ("PersonaIdKept", personaToKeep.personaID), ("PersonaTypeKept", personaToKeep.personaType), ("PersonaIdDiscarded", personaToDiscard.personaID));
	}

	public static void SendEvent_SocialAccountLinkedCancelled(ILoginContext context)
	{
		BIEventType.SocialAccountLinkCancelled.SendWithDefaults(("Type", context.SocialType));
	}
}
