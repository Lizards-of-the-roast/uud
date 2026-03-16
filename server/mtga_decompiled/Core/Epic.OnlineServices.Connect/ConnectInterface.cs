using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

public sealed class ConnectInterface : Handle
{
	private delegate void OnLoginStatusChangedCallbackInternal(IntPtr messagePtr);

	private delegate void OnAuthExpirationCallbackInternal(IntPtr messagePtr);

	private delegate void OnQueryExternalAccountMappingsCallbackInternal(IntPtr messagePtr);

	private delegate void OnLinkAccountCallbackInternal(IntPtr messagePtr);

	private delegate void OnCreateUserCallbackInternal(IntPtr messagePtr);

	private delegate void OnLoginCallbackInternal(IntPtr messagePtr);

	public ConnectInterface(IntPtr innerHandle)
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
		EOS_Connect_Login(base.InnerHandle, ref options2, clientDataAddress, onLoginCallbackInternal);
		options2.Dispose();
	}

	public void CreateUser(CreateUserOptions options, object clientData, OnCreateUserCallback completionDelegate)
	{
		object obj = default(CreateUserOptions_);
		Helper.CopyProperties(options, obj);
		CreateUserOptions_ options2 = (CreateUserOptions_)obj;
		OnCreateUserCallbackInternal onCreateUserCallbackInternal = OnCreateUser;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onCreateUserCallbackInternal);
		EOS_Connect_CreateUser(base.InnerHandle, ref options2, clientDataAddress, onCreateUserCallbackInternal);
		options2.Dispose();
	}

	public void LinkAccount(LinkAccountOptions options, object clientData, OnLinkAccountCallback completionDelegate)
	{
		object obj = default(LinkAccountOptions_);
		Helper.CopyProperties(options, obj);
		LinkAccountOptions_ options2 = (LinkAccountOptions_)obj;
		OnLinkAccountCallbackInternal onLinkAccountCallbackInternal = OnLinkAccount;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onLinkAccountCallbackInternal);
		EOS_Connect_LinkAccount(base.InnerHandle, ref options2, clientDataAddress, onLinkAccountCallbackInternal);
		options2.Dispose();
	}

	public void QueryExternalAccountMappings(QueryExternalAccountMappingsOptions options, object clientData, OnQueryExternalAccountMappingsCallback completionDelegate)
	{
		object obj = default(QueryExternalAccountMappingsOptions_);
		Helper.CopyProperties(options, obj);
		QueryExternalAccountMappingsOptions_ options2 = (QueryExternalAccountMappingsOptions_)obj;
		OnQueryExternalAccountMappingsCallbackInternal onQueryExternalAccountMappingsCallbackInternal = OnQueryExternalAccountMappings;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onQueryExternalAccountMappingsCallbackInternal);
		EOS_Connect_QueryExternalAccountMappings(base.InnerHandle, ref options2, clientDataAddress, onQueryExternalAccountMappingsCallbackInternal);
		options2.Dispose();
	}

	public ProductUserId GetExternalAccountMapping(GetExternalAccountMappingsOptions options)
	{
		object obj = default(GetExternalAccountMappingsOptions_);
		Helper.CopyProperties(options, obj);
		GetExternalAccountMappingsOptions_ options2 = (GetExternalAccountMappingsOptions_)obj;
		IntPtr intPtr = EOS_Connect_GetExternalAccountMapping(base.InnerHandle, ref options2);
		options2.Dispose();
		if (!(intPtr == IntPtr.Zero))
		{
			return new ProductUserId(intPtr);
		}
		return null;
	}

	public int GetLoggedInUsersCount()
	{
		return EOS_Connect_GetLoggedInUsersCount(base.InnerHandle);
	}

	public ProductUserId GetLoggedInUserByIndex(int index)
	{
		IntPtr intPtr = EOS_Connect_GetLoggedInUserByIndex(base.InnerHandle, index);
		if (!(intPtr == IntPtr.Zero))
		{
			return new ProductUserId(intPtr);
		}
		return null;
	}

	public LoginStatus GetLoginStatus(ProductUserId localUserId)
	{
		return EOS_Connect_GetLoginStatus(base.InnerHandle, localUserId.InnerHandle);
	}

	public ulong AddNotifyAuthExpiration(AddNotifyAuthExpirationOptions options, object clientData, OnAuthExpirationCallback notification)
	{
		object obj = default(AddNotifyAuthExpirationOptions_);
		Helper.CopyProperties(options, obj);
		AddNotifyAuthExpirationOptions_ options2 = (AddNotifyAuthExpirationOptions_)obj;
		OnAuthExpirationCallbackInternal onAuthExpirationCallbackInternal = OnAuthExpiration;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, notification, onAuthExpirationCallbackInternal);
		ulong result = EOS_Connect_AddNotifyAuthExpiration(base.InnerHandle, ref options2, clientDataAddress, onAuthExpirationCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyAuthExpiration(ulong inId)
	{
		EOS_Connect_RemoveNotifyAuthExpiration(base.InnerHandle, inId);
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
		ulong result = EOS_Connect_AddNotifyLoginStatusChanged(base.InnerHandle, ref options2, clientDataAddress, onLoginStatusChangedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyLoginStatusChanged(ulong inId)
	{
		EOS_Connect_RemoveNotifyLoginStatusChanged(base.InnerHandle, inId);
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
	private static void OnAuthExpiration(IntPtr messageAddress)
	{
		AuthExpirationCallbackInfo_ authExpirationCallbackInfo_ = Marshal.PtrToStructure<AuthExpirationCallbackInfo_>(messageAddress);
		AuthExpirationCallbackInfo authExpirationCallbackInfo = new AuthExpirationCallbackInfo();
		Helper.CopyProperties(authExpirationCallbackInfo_, authExpirationCallbackInfo);
		IntPtr clientDataAddress = authExpirationCallbackInfo_.ClientDataAddress;
		authExpirationCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, authExpirationCallbackInfo) as OnAuthExpirationCallback)(authExpirationCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnQueryExternalAccountMappings(IntPtr messageAddress)
	{
		QueryExternalAccountMappingsCallbackInfo_ queryExternalAccountMappingsCallbackInfo_ = Marshal.PtrToStructure<QueryExternalAccountMappingsCallbackInfo_>(messageAddress);
		QueryExternalAccountMappingsCallbackInfo queryExternalAccountMappingsCallbackInfo = new QueryExternalAccountMappingsCallbackInfo();
		Helper.CopyProperties(queryExternalAccountMappingsCallbackInfo_, queryExternalAccountMappingsCallbackInfo);
		IntPtr clientDataAddress = queryExternalAccountMappingsCallbackInfo_.ClientDataAddress;
		queryExternalAccountMappingsCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, queryExternalAccountMappingsCallbackInfo) as OnQueryExternalAccountMappingsCallback)(queryExternalAccountMappingsCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnLinkAccount(IntPtr messageAddress)
	{
		LinkAccountCallbackInfo_ linkAccountCallbackInfo_ = Marshal.PtrToStructure<LinkAccountCallbackInfo_>(messageAddress);
		LinkAccountCallbackInfo linkAccountCallbackInfo = new LinkAccountCallbackInfo();
		Helper.CopyProperties(linkAccountCallbackInfo_, linkAccountCallbackInfo);
		IntPtr clientDataAddress = linkAccountCallbackInfo_.ClientDataAddress;
		linkAccountCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, linkAccountCallbackInfo) as OnLinkAccountCallback)(linkAccountCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnCreateUser(IntPtr messageAddress)
	{
		CreateUserCallbackInfo_ createUserCallbackInfo_ = Marshal.PtrToStructure<CreateUserCallbackInfo_>(messageAddress);
		CreateUserCallbackInfo createUserCallbackInfo = new CreateUserCallbackInfo();
		Helper.CopyProperties(createUserCallbackInfo_, createUserCallbackInfo);
		IntPtr clientDataAddress = createUserCallbackInfo_.ClientDataAddress;
		createUserCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, createUserCallbackInfo) as OnCreateUserCallback)(createUserCallbackInfo);
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
	private static extern void EOS_Connect_RemoveNotifyLoginStatusChanged(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern ulong EOS_Connect_AddNotifyLoginStatusChanged(IntPtr handle, ref AddNotifyLoginStatusChangedOptions_ options, IntPtr clientData, OnLoginStatusChangedCallbackInternal notification);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Connect_RemoveNotifyAuthExpiration(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern ulong EOS_Connect_AddNotifyAuthExpiration(IntPtr handle, ref AddNotifyAuthExpirationOptions_ options, IntPtr clientData, OnAuthExpirationCallbackInternal notification);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern LoginStatus EOS_Connect_GetLoginStatus(IntPtr handle, IntPtr localUserId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Connect_GetLoggedInUserByIndex(IntPtr handle, int index);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern int EOS_Connect_GetLoggedInUsersCount(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Connect_GetExternalAccountMapping(IntPtr handle, ref GetExternalAccountMappingsOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Connect_QueryExternalAccountMappings(IntPtr handle, ref QueryExternalAccountMappingsOptions_ options, IntPtr clientData, OnQueryExternalAccountMappingsCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Connect_LinkAccount(IntPtr handle, ref LinkAccountOptions_ options, IntPtr clientData, OnLinkAccountCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Connect_CreateUser(IntPtr handle, ref CreateUserOptions_ options, IntPtr clientData, OnCreateUserCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Connect_Login(IntPtr handle, ref LoginOptions_ options, IntPtr clientData, OnLoginCallbackInternal completionDelegate);
}
