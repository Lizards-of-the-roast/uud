using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

public sealed class AuthInterface : Handle
{
	private delegate void OnLoginStatusChangedCallbackInternal(IntPtr messagePtr);

	private delegate void OnVerifyUserAuthCallbackInternal(IntPtr messagePtr);

	private delegate void OnLogoutCallbackInternal(IntPtr messagePtr);

	private delegate void OnLoginCallbackInternal(IntPtr messagePtr);

	public AuthInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void Login(LoginOptions options, object clientData, OnLoginCallback completionDelegate)
	{
		object obj = default(LoginOptions_);
		Helper.CopyProperties(options, obj);
		LoginOptions_ options2 = (LoginOptions_)obj;
		OnLoginCallbackInternal onLoginCallbackInternal = OnLogin;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onLoginCallbackInternal);
		EOS_Auth_Login(base.InnerHandle, ref options2, clientDataAddress, onLoginCallbackInternal);
		options2.Dispose();
	}

	public void Logout(LogoutOptions options, object clientData, OnLogoutCallback completionDelegate)
	{
		object obj = default(LogoutOptions_);
		Helper.CopyProperties(options, obj);
		LogoutOptions_ options2 = (LogoutOptions_)obj;
		OnLogoutCallbackInternal onLogoutCallbackInternal = OnLogout;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onLogoutCallbackInternal);
		EOS_Auth_Logout(base.InnerHandle, ref options2, clientDataAddress, onLogoutCallbackInternal);
		options2.Dispose();
	}

	public void VerifyUserAuth(VerifyUserAuthOptions options, object clientData, OnVerifyUserAuthCallback completionDelegate)
	{
		object obj = default(VerifyUserAuthOptions_);
		Helper.CopyProperties(options, obj);
		VerifyUserAuthOptions_ options2 = (VerifyUserAuthOptions_)obj;
		OnVerifyUserAuthCallbackInternal onVerifyUserAuthCallbackInternal = OnVerifyUserAuth;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onVerifyUserAuthCallbackInternal);
		EOS_Auth_VerifyUserAuth(base.InnerHandle, ref options2, clientDataAddress, onVerifyUserAuthCallbackInternal);
		options2.Dispose();
	}

	public int GetLoggedInAccountsCount()
	{
		return EOS_Auth_GetLoggedInAccountsCount(base.InnerHandle);
	}

	public EpicAccountId GetLoggedInAccountByIndex(int index)
	{
		IntPtr intPtr = EOS_Auth_GetLoggedInAccountByIndex(base.InnerHandle, index);
		if (!(intPtr == IntPtr.Zero))
		{
			return new EpicAccountId(intPtr);
		}
		return null;
	}

	public LoginStatus GetLoginStatus(EpicAccountId localUserId)
	{
		return EOS_Auth_GetLoginStatus(base.InnerHandle, localUserId.InnerHandle);
	}

	public Result CopyUserAuthToken(CopyUserAuthTokenOptions options, EpicAccountId localUserId, out Token outUserAuthToken)
	{
		object obj = default(CopyUserAuthTokenOptions_);
		Helper.CopyProperties(options, obj);
		CopyUserAuthTokenOptions_ options2 = (CopyUserAuthTokenOptions_)obj;
		outUserAuthToken = Helper.GetDefault<Token>();
		IntPtr outUserAuthToken2 = IntPtr.Zero;
		Result result = EOS_Auth_CopyUserAuthToken(base.InnerHandle, ref options2, localUserId.InnerHandle, ref outUserAuthToken2);
		options2.Dispose();
		if (outUserAuthToken2 != IntPtr.Zero)
		{
			Token_ token_ = Marshal.PtrToStructure<Token_>(outUserAuthToken2);
			outUserAuthToken = new Token();
			Helper.CopyProperties(token_, outUserAuthToken);
			EOS_Auth_Token_Release(outUserAuthToken2);
			token_.Dispose();
		}
		return result;
	}

	public ulong AddNotifyLoginStatusChanged(AddNotifyLoginStatusChangedOptions options, object clientData, OnLoginStatusChangedCallback notification)
	{
		object obj = default(AddNotifyLoginStatusChangedOptions_);
		Helper.CopyProperties(options, obj);
		AddNotifyLoginStatusChangedOptions_ options2 = (AddNotifyLoginStatusChangedOptions_)obj;
		OnLoginStatusChangedCallbackInternal onLoginStatusChangedCallbackInternal = OnLoginStatusChanged;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, notification, onLoginStatusChangedCallbackInternal);
		ulong result = EOS_Auth_AddNotifyLoginStatusChanged(base.InnerHandle, ref options2, clientDataAddress, onLoginStatusChangedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyLoginStatusChanged(ulong inId)
	{
		EOS_Auth_RemoveNotifyLoginStatusChanged(base.InnerHandle, inId);
	}

	[MonoPInvokeCallback]
	private static void OnLoginStatusChanged(IntPtr messageAddress)
	{
		LoginStatusChangedCallbackInfo_ loginStatusChangedCallbackInfo_ = Marshal.PtrToStructure<LoginStatusChangedCallbackInfo_>(messageAddress);
		LoginStatusChangedCallbackInfo loginStatusChangedCallbackInfo = new LoginStatusChangedCallbackInfo();
		Helper.CopyProperties(loginStatusChangedCallbackInfo_, loginStatusChangedCallbackInfo);
		IntPtr clientDataAddress = loginStatusChangedCallbackInfo_.ClientDataAddress;
		loginStatusChangedCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, loginStatusChangedCallbackInfo) as OnLoginStatusChangedCallback)(loginStatusChangedCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnVerifyUserAuth(IntPtr messageAddress)
	{
		VerifyUserAuthCallbackInfo_ verifyUserAuthCallbackInfo_ = Marshal.PtrToStructure<VerifyUserAuthCallbackInfo_>(messageAddress);
		VerifyUserAuthCallbackInfo verifyUserAuthCallbackInfo = new VerifyUserAuthCallbackInfo();
		Helper.CopyProperties(verifyUserAuthCallbackInfo_, verifyUserAuthCallbackInfo);
		IntPtr clientDataAddress = verifyUserAuthCallbackInfo_.ClientDataAddress;
		verifyUserAuthCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, verifyUserAuthCallbackInfo) as OnVerifyUserAuthCallback)(verifyUserAuthCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnLogout(IntPtr messageAddress)
	{
		LogoutCallbackInfo_ logoutCallbackInfo_ = Marshal.PtrToStructure<LogoutCallbackInfo_>(messageAddress);
		LogoutCallbackInfo logoutCallbackInfo = new LogoutCallbackInfo();
		Helper.CopyProperties(logoutCallbackInfo_, logoutCallbackInfo);
		IntPtr clientDataAddress = logoutCallbackInfo_.ClientDataAddress;
		logoutCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, logoutCallbackInfo) as OnLogoutCallback)(logoutCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnLogin(IntPtr messageAddress)
	{
		LoginCallbackInfo_ loginCallbackInfo_ = Marshal.PtrToStructure<LoginCallbackInfo_>(messageAddress);
		LoginCallbackInfo loginCallbackInfo = new LoginCallbackInfo();
		Helper.CopyProperties(loginCallbackInfo_, loginCallbackInfo);
		IntPtr clientDataAddress = loginCallbackInfo_.ClientDataAddress;
		loginCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, loginCallbackInfo) as OnLoginCallback)(loginCallbackInfo);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Auth_Token_Release(IntPtr authToken);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Auth_RemoveNotifyLoginStatusChanged(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern ulong EOS_Auth_AddNotifyLoginStatusChanged(IntPtr handle, ref AddNotifyLoginStatusChangedOptions_ options, IntPtr clientData, OnLoginStatusChangedCallbackInternal notification);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Auth_CopyUserAuthToken(IntPtr handle, ref CopyUserAuthTokenOptions_ options, IntPtr localUserId, ref IntPtr outUserAuthToken);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern LoginStatus EOS_Auth_GetLoginStatus(IntPtr handle, IntPtr localUserId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Auth_GetLoggedInAccountByIndex(IntPtr handle, int index);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern int EOS_Auth_GetLoggedInAccountsCount(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Auth_VerifyUserAuth(IntPtr handle, ref VerifyUserAuthOptions_ options, IntPtr clientData, OnVerifyUserAuthCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Auth_Logout(IntPtr handle, ref LogoutOptions_ options, IntPtr clientData, OnLogoutCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Auth_Login(IntPtr handle, ref LoginOptions_ options, IntPtr clientData, OnLoginCallbackInternal completionDelegate);
}
