using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

public sealed class PresenceInterface : Handle
{
	private delegate void OnPresenceChangedCallbackInternal(IntPtr messagePtr);

	private delegate void SetPresenceCompleteCallbackInternal(IntPtr messagePtr);

	private delegate void OnQueryPresenceCompleteCallbackInternal(IntPtr messagePtr);

	public PresenceInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public void QueryPresence(QueryPresenceOptions options, object clientData, OnQueryPresenceCompleteCallback completionDelegate)
	{
		object obj = default(QueryPresenceOptions_);
		Helper.CopyProperties(options, obj);
		QueryPresenceOptions_ options2 = (QueryPresenceOptions_)obj;
		OnQueryPresenceCompleteCallbackInternal onQueryPresenceCompleteCallbackInternal = OnQueryPresenceComplete;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onQueryPresenceCompleteCallbackInternal);
		EOS_Presence_QueryPresence(base.InnerHandle, ref options2, clientDataAddress, onQueryPresenceCompleteCallbackInternal);
		options2.Dispose();
	}

	public int HasPresence(HasPresenceOptions options)
	{
		object obj = default(HasPresenceOptions_);
		Helper.CopyProperties(options, obj);
		HasPresenceOptions_ options2 = (HasPresenceOptions_)obj;
		int result = EOS_Presence_HasPresence(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopyPresence(CopyPresenceOptions options, out Info outPresence)
	{
		object obj = default(CopyPresenceOptions_);
		Helper.CopyProperties(options, obj);
		CopyPresenceOptions_ options2 = (CopyPresenceOptions_)obj;
		outPresence = Helper.GetDefault<Info>();
		IntPtr outPresence2 = IntPtr.Zero;
		Result result = EOS_Presence_CopyPresence(base.InnerHandle, ref options2, ref outPresence2);
		options2.Dispose();
		if (outPresence2 != IntPtr.Zero)
		{
			Info_ info_ = Marshal.PtrToStructure<Info_>(outPresence2);
			outPresence = new Info();
			Helper.CopyProperties(info_, outPresence);
			EOS_Presence_Info_Release(outPresence2);
			info_.Dispose();
		}
		return result;
	}

	public Result CreatePresenceModification(CreatePresenceModificationOptions options, out PresenceModification outPresenceModificationHandle)
	{
		object obj = default(CreatePresenceModificationOptions_);
		Helper.CopyProperties(options, obj);
		CreatePresenceModificationOptions_ options2 = (CreatePresenceModificationOptions_)obj;
		outPresenceModificationHandle = Helper.GetDefault<PresenceModification>();
		IntPtr outPresenceModificationHandle2 = IntPtr.Zero;
		Result result = EOS_Presence_CreatePresenceModification(base.InnerHandle, ref options2, ref outPresenceModificationHandle2);
		options2.Dispose();
		outPresenceModificationHandle = ((outPresenceModificationHandle2 == IntPtr.Zero) ? null : new PresenceModification(outPresenceModificationHandle2));
		return result;
	}

	public void SetPresence(SetPresenceOptions options, object clientData, SetPresenceCompleteCallback completionDelegate)
	{
		object obj = default(SetPresenceOptions_);
		Helper.CopyProperties(options, obj);
		SetPresenceOptions_ options2 = (SetPresenceOptions_)obj;
		SetPresenceCompleteCallbackInternal setPresenceCompleteCallbackInternal = SetPresenceComplete;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, setPresenceCompleteCallbackInternal);
		EOS_Presence_SetPresence(base.InnerHandle, ref options2, clientDataAddress, setPresenceCompleteCallbackInternal);
		options2.Dispose();
	}

	public ulong AddNotifyOnPresenceChanged(AddNotifyOnPresenceChangedOptions options, object clientData, OnPresenceChangedCallback notificationHandler)
	{
		object obj = default(AddNotifyOnPresenceChangedOptions_);
		Helper.CopyProperties(options, obj);
		AddNotifyOnPresenceChangedOptions_ options2 = (AddNotifyOnPresenceChangedOptions_)obj;
		OnPresenceChangedCallbackInternal onPresenceChangedCallbackInternal = OnPresenceChanged;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, notificationHandler, onPresenceChangedCallbackInternal);
		ulong result = EOS_Presence_AddNotifyOnPresenceChanged(base.InnerHandle, ref options2, clientDataAddress, onPresenceChangedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyOnPresenceChanged(ulong notificationId)
	{
		EOS_Presence_RemoveNotifyOnPresenceChanged(base.InnerHandle, notificationId);
	}

	[MonoPInvokeCallback]
	private static void OnPresenceChanged(IntPtr messageAddress)
	{
		PresenceChangedCallbackInfo_ presenceChangedCallbackInfo_ = Marshal.PtrToStructure<PresenceChangedCallbackInfo_>(messageAddress);
		PresenceChangedCallbackInfo presenceChangedCallbackInfo = new PresenceChangedCallbackInfo();
		Helper.CopyProperties(presenceChangedCallbackInfo_, presenceChangedCallbackInfo);
		IntPtr clientDataAddress = presenceChangedCallbackInfo_.ClientDataAddress;
		presenceChangedCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, presenceChangedCallbackInfo) as OnPresenceChangedCallback)(presenceChangedCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void SetPresenceComplete(IntPtr messageAddress)
	{
		SetPresenceCallbackInfo_ setPresenceCallbackInfo_ = Marshal.PtrToStructure<SetPresenceCallbackInfo_>(messageAddress);
		SetPresenceCallbackInfo setPresenceCallbackInfo = new SetPresenceCallbackInfo();
		Helper.CopyProperties(setPresenceCallbackInfo_, setPresenceCallbackInfo);
		IntPtr clientDataAddress = setPresenceCallbackInfo_.ClientDataAddress;
		setPresenceCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, setPresenceCallbackInfo) as SetPresenceCompleteCallback)(setPresenceCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnQueryPresenceComplete(IntPtr messageAddress)
	{
		QueryPresenceCallbackInfo_ queryPresenceCallbackInfo_ = Marshal.PtrToStructure<QueryPresenceCallbackInfo_>(messageAddress);
		QueryPresenceCallbackInfo queryPresenceCallbackInfo = new QueryPresenceCallbackInfo();
		Helper.CopyProperties(queryPresenceCallbackInfo_, queryPresenceCallbackInfo);
		IntPtr clientDataAddress = queryPresenceCallbackInfo_.ClientDataAddress;
		queryPresenceCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, queryPresenceCallbackInfo) as OnQueryPresenceCompleteCallback)(queryPresenceCallbackInfo);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Presence_Info_Release(IntPtr presenceInfo);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Presence_RemoveNotifyOnPresenceChanged(IntPtr handle, ulong notificationId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern ulong EOS_Presence_AddNotifyOnPresenceChanged(IntPtr handle, ref AddNotifyOnPresenceChangedOptions_ options, IntPtr clientData, OnPresenceChangedCallbackInternal notificationHandler);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Presence_SetPresence(IntPtr handle, ref SetPresenceOptions_ options, IntPtr clientData, SetPresenceCompleteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Presence_CreatePresenceModification(IntPtr handle, ref CreatePresenceModificationOptions_ options, ref IntPtr outPresenceModificationHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Presence_CopyPresence(IntPtr handle, ref CopyPresenceOptions_ options, ref IntPtr outPresence);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern int EOS_Presence_HasPresence(IntPtr handle, ref HasPresenceOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Presence_QueryPresence(IntPtr handle, ref QueryPresenceOptions_ options, IntPtr clientData, OnQueryPresenceCompleteCallbackInternal completionDelegate);
}
