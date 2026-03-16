using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo;

public sealed class UserInfoInterface : Handle
{
	private delegate void OnQueryUserInfoByDisplayNameCallbackInternal(IntPtr messagePtr);

	private delegate void OnQueryUserInfoCallbackInternal(IntPtr messagePtr);

	public UserInfoInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryUserInfo(QueryUserInfoOptions options, object clientData, OnQueryUserInfoCallback completionDelegate)
	{
		object obj = default(QueryUserInfoOptions_);
		Helper.CopyProperties(options, obj);
		QueryUserInfoOptions_ options2 = (QueryUserInfoOptions_)obj;
		OnQueryUserInfoCallbackInternal onQueryUserInfoCallbackInternal = OnQueryUserInfo;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onQueryUserInfoCallbackInternal);
		EOS_UserInfo_QueryUserInfo(base.InnerHandle, ref options2, clientDataAddress, onQueryUserInfoCallbackInternal);
		options2.Dispose();
	}

	public void QueryUserInfoByDisplayName(QueryUserInfoByDisplayNameOptions options, object clientData, OnQueryUserInfoByDisplayNameCallback completionDelegate)
	{
		object obj = default(QueryUserInfoByDisplayNameOptions_);
		Helper.CopyProperties(options, obj);
		QueryUserInfoByDisplayNameOptions_ options2 = (QueryUserInfoByDisplayNameOptions_)obj;
		OnQueryUserInfoByDisplayNameCallbackInternal onQueryUserInfoByDisplayNameCallbackInternal = OnQueryUserInfoByDisplayName;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onQueryUserInfoByDisplayNameCallbackInternal);
		EOS_UserInfo_QueryUserInfoByDisplayName(base.InnerHandle, ref options2, clientDataAddress, onQueryUserInfoByDisplayNameCallbackInternal);
		options2.Dispose();
	}

	public Result CopyUserInfo(CopyUserInfoOptions options, out UserInfoData outUserInfo)
	{
		object obj = default(CopyUserInfoOptions_);
		Helper.CopyProperties(options, obj);
		CopyUserInfoOptions_ options2 = (CopyUserInfoOptions_)obj;
		outUserInfo = Helper.GetDefault<UserInfoData>();
		IntPtr outUserInfo2 = IntPtr.Zero;
		Result result = EOS_UserInfo_CopyUserInfo(base.InnerHandle, ref options2, ref outUserInfo2);
		options2.Dispose();
		if (outUserInfo2 != IntPtr.Zero)
		{
			UserInfoData_ userInfoData_ = Marshal.PtrToStructure<UserInfoData_>(outUserInfo2);
			outUserInfo = new UserInfoData();
			Helper.CopyProperties(userInfoData_, outUserInfo);
			EOS_UserInfo_Release(outUserInfo2);
			userInfoData_.Dispose();
		}
		return result;
	}

	[MonoPInvokeCallback]
	private static void OnQueryUserInfoByDisplayName(IntPtr messageAddress)
	{
		QueryUserInfoByDisplayNameCallbackInfo_ queryUserInfoByDisplayNameCallbackInfo_ = Marshal.PtrToStructure<QueryUserInfoByDisplayNameCallbackInfo_>(messageAddress);
		QueryUserInfoByDisplayNameCallbackInfo queryUserInfoByDisplayNameCallbackInfo = new QueryUserInfoByDisplayNameCallbackInfo();
		Helper.CopyProperties(queryUserInfoByDisplayNameCallbackInfo_, queryUserInfoByDisplayNameCallbackInfo);
		IntPtr clientDataAddress = queryUserInfoByDisplayNameCallbackInfo_.ClientDataAddress;
		queryUserInfoByDisplayNameCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, queryUserInfoByDisplayNameCallbackInfo) as OnQueryUserInfoByDisplayNameCallback)(queryUserInfoByDisplayNameCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnQueryUserInfo(IntPtr messageAddress)
	{
		QueryUserInfoCallbackInfo_ queryUserInfoCallbackInfo_ = Marshal.PtrToStructure<QueryUserInfoCallbackInfo_>(messageAddress);
		QueryUserInfoCallbackInfo queryUserInfoCallbackInfo = new QueryUserInfoCallbackInfo();
		Helper.CopyProperties(queryUserInfoCallbackInfo_, queryUserInfoCallbackInfo);
		IntPtr clientDataAddress = queryUserInfoCallbackInfo_.ClientDataAddress;
		queryUserInfoCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, queryUserInfoCallbackInfo) as OnQueryUserInfoCallback)(queryUserInfoCallbackInfo);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_UserInfo_Release(IntPtr userInfo);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_UserInfo_CopyUserInfo(IntPtr handle, ref CopyUserInfoOptions_ options, ref IntPtr outUserInfo);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_UserInfo_QueryUserInfoByDisplayName(IntPtr handle, ref QueryUserInfoByDisplayNameOptions_ options, IntPtr clientData, OnQueryUserInfoByDisplayNameCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_UserInfo_QueryUserInfo(IntPtr handle, ref QueryUserInfoOptions_ options, IntPtr clientData, OnQueryUserInfoCallbackInternal completionDelegate);
}
