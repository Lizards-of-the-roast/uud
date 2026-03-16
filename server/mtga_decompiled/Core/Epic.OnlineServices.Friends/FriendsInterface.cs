using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Friends;

public sealed class FriendsInterface : Handle
{
	private delegate void OnFriendsUpdateCallbackInternal(IntPtr messagePtr);

	private delegate void OnRejectInviteCallbackInternal(IntPtr messagePtr);

	private delegate void OnAcceptInviteCallbackInternal(IntPtr messagePtr);

	private delegate void OnSendInviteCallbackInternal(IntPtr messagePtr);

	private delegate void OnQueryFriendsCallbackInternal(IntPtr messagePtr);

	public FriendsInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryFriends(QueryFriendsOptions options, object clientData, OnQueryFriendsCallback completionDelegate)
	{
		object obj = default(QueryFriendsOptions_);
		Helper.CopyProperties(options, obj);
		QueryFriendsOptions_ options2 = (QueryFriendsOptions_)obj;
		OnQueryFriendsCallbackInternal onQueryFriendsCallbackInternal = OnQueryFriends;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onQueryFriendsCallbackInternal);
		EOS_Friends_QueryFriends(base.InnerHandle, ref options2, clientDataAddress, onQueryFriendsCallbackInternal);
		options2.Dispose();
	}

	public void SendInvite(SendInviteOptions options, object clientData, OnSendInviteCallback completionDelegate)
	{
		object obj = default(SendInviteOptions_);
		Helper.CopyProperties(options, obj);
		SendInviteOptions_ options2 = (SendInviteOptions_)obj;
		OnSendInviteCallbackInternal onSendInviteCallbackInternal = OnSendInvite;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onSendInviteCallbackInternal);
		EOS_Friends_SendInvite(base.InnerHandle, ref options2, clientDataAddress, onSendInviteCallbackInternal);
		options2.Dispose();
	}

	public void AcceptInvite(AcceptInviteOptions options, object clientData, OnAcceptInviteCallback completionDelegate)
	{
		object obj = default(AcceptInviteOptions_);
		Helper.CopyProperties(options, obj);
		AcceptInviteOptions_ options2 = (AcceptInviteOptions_)obj;
		OnAcceptInviteCallbackInternal onAcceptInviteCallbackInternal = OnAcceptInvite;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onAcceptInviteCallbackInternal);
		EOS_Friends_AcceptInvite(base.InnerHandle, ref options2, clientDataAddress, onAcceptInviteCallbackInternal);
		options2.Dispose();
	}

	public void RejectInvite(RejectInviteOptions options, object clientData, OnRejectInviteCallback completionDelegate)
	{
		object obj = default(RejectInviteOptions_);
		Helper.CopyProperties(options, obj);
		RejectInviteOptions_ options2 = (RejectInviteOptions_)obj;
		OnRejectInviteCallbackInternal onRejectInviteCallbackInternal = OnRejectInvite;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onRejectInviteCallbackInternal);
		EOS_Friends_RejectInvite(base.InnerHandle, ref options2, clientDataAddress, onRejectInviteCallbackInternal);
		options2.Dispose();
	}

	public int GetFriendsCount(GetFriendsCountOptions options)
	{
		object obj = default(GetFriendsCountOptions_);
		Helper.CopyProperties(options, obj);
		GetFriendsCountOptions_ options2 = (GetFriendsCountOptions_)obj;
		int result = EOS_Friends_GetFriendsCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public EpicAccountId GetFriendAtIndex(GetFriendAtIndexOptions options)
	{
		object obj = default(GetFriendAtIndexOptions_);
		Helper.CopyProperties(options, obj);
		GetFriendAtIndexOptions_ options2 = (GetFriendAtIndexOptions_)obj;
		IntPtr intPtr = EOS_Friends_GetFriendAtIndex(base.InnerHandle, ref options2);
		options2.Dispose();
		if (!(intPtr == IntPtr.Zero))
		{
			return new EpicAccountId(intPtr);
		}
		return null;
	}

	public FriendsStatus GetStatus(GetStatusOptions options)
	{
		object obj = default(GetStatusOptions_);
		Helper.CopyProperties(options, obj);
		GetStatusOptions_ options2 = (GetStatusOptions_)obj;
		FriendsStatus result = EOS_Friends_GetStatus(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public ulong AddNotifyFriendsUpdate(AddNotifyFriendsUpdateOptions options, object clientData, OnFriendsUpdateCallback friendsUpdateHandler)
	{
		object obj = default(AddNotifyFriendsUpdateOptions_);
		Helper.CopyProperties(options, obj);
		AddNotifyFriendsUpdateOptions_ options2 = (AddNotifyFriendsUpdateOptions_)obj;
		OnFriendsUpdateCallbackInternal onFriendsUpdateCallbackInternal = OnFriendsUpdate;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, friendsUpdateHandler, onFriendsUpdateCallbackInternal);
		ulong result = EOS_Friends_AddNotifyFriendsUpdate(base.InnerHandle, ref options2, clientDataAddress, onFriendsUpdateCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyFriendsUpdate(ulong notificationId)
	{
		EOS_Friends_RemoveNotifyFriendsUpdate(base.InnerHandle, notificationId);
	}

	[MonoPInvokeCallback]
	private static void OnFriendsUpdate(IntPtr messageAddress)
	{
		OnFriendsUpdateInfo_ onFriendsUpdateInfo_ = Marshal.PtrToStructure<OnFriendsUpdateInfo_>(messageAddress);
		OnFriendsUpdateInfo onFriendsUpdateInfo = new OnFriendsUpdateInfo();
		Helper.CopyProperties(onFriendsUpdateInfo_, onFriendsUpdateInfo);
		IntPtr clientDataAddress = onFriendsUpdateInfo_.ClientDataAddress;
		onFriendsUpdateInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, onFriendsUpdateInfo) as OnFriendsUpdateCallback)(onFriendsUpdateInfo);
	}

	[MonoPInvokeCallback]
	private static void OnRejectInvite(IntPtr messageAddress)
	{
		RejectInviteCallbackInfo_ rejectInviteCallbackInfo_ = Marshal.PtrToStructure<RejectInviteCallbackInfo_>(messageAddress);
		RejectInviteCallbackInfo rejectInviteCallbackInfo = new RejectInviteCallbackInfo();
		Helper.CopyProperties(rejectInviteCallbackInfo_, rejectInviteCallbackInfo);
		IntPtr clientDataAddress = rejectInviteCallbackInfo_.ClientDataAddress;
		rejectInviteCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, rejectInviteCallbackInfo) as OnRejectInviteCallback)(rejectInviteCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnAcceptInvite(IntPtr messageAddress)
	{
		AcceptInviteCallbackInfo_ acceptInviteCallbackInfo_ = Marshal.PtrToStructure<AcceptInviteCallbackInfo_>(messageAddress);
		AcceptInviteCallbackInfo acceptInviteCallbackInfo = new AcceptInviteCallbackInfo();
		Helper.CopyProperties(acceptInviteCallbackInfo_, acceptInviteCallbackInfo);
		IntPtr clientDataAddress = acceptInviteCallbackInfo_.ClientDataAddress;
		acceptInviteCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, acceptInviteCallbackInfo) as OnAcceptInviteCallback)(acceptInviteCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnSendInvite(IntPtr messageAddress)
	{
		SendInviteCallbackInfo_ sendInviteCallbackInfo_ = Marshal.PtrToStructure<SendInviteCallbackInfo_>(messageAddress);
		SendInviteCallbackInfo sendInviteCallbackInfo = new SendInviteCallbackInfo();
		Helper.CopyProperties(sendInviteCallbackInfo_, sendInviteCallbackInfo);
		IntPtr clientDataAddress = sendInviteCallbackInfo_.ClientDataAddress;
		sendInviteCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, sendInviteCallbackInfo) as OnSendInviteCallback)(sendInviteCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnQueryFriends(IntPtr messageAddress)
	{
		QueryFriendsCallbackInfo_ queryFriendsCallbackInfo_ = Marshal.PtrToStructure<QueryFriendsCallbackInfo_>(messageAddress);
		QueryFriendsCallbackInfo queryFriendsCallbackInfo = new QueryFriendsCallbackInfo();
		Helper.CopyProperties(queryFriendsCallbackInfo_, queryFriendsCallbackInfo);
		IntPtr clientDataAddress = queryFriendsCallbackInfo_.ClientDataAddress;
		queryFriendsCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, queryFriendsCallbackInfo) as OnQueryFriendsCallback)(queryFriendsCallbackInfo);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Friends_RemoveNotifyFriendsUpdate(IntPtr handle, ulong notificationId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern ulong EOS_Friends_AddNotifyFriendsUpdate(IntPtr handle, ref AddNotifyFriendsUpdateOptions_ options, IntPtr clientData, OnFriendsUpdateCallbackInternal friendsUpdateHandler);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern FriendsStatus EOS_Friends_GetStatus(IntPtr handle, ref GetStatusOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Friends_GetFriendAtIndex(IntPtr handle, ref GetFriendAtIndexOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern int EOS_Friends_GetFriendsCount(IntPtr handle, ref GetFriendsCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Friends_RejectInvite(IntPtr handle, ref RejectInviteOptions_ options, IntPtr clientData, OnRejectInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Friends_AcceptInvite(IntPtr handle, ref AcceptInviteOptions_ options, IntPtr clientData, OnAcceptInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Friends_SendInvite(IntPtr handle, ref SendInviteOptions_ options, IntPtr clientData, OnSendInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Friends_QueryFriends(IntPtr handle, ref QueryFriendsOptions_ options, IntPtr clientData, OnQueryFriendsCallbackInternal completionDelegate);
}
