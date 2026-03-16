using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class SessionsInterface : Handle
{
	private delegate void OnSessionInviteReceivedCallbackInternal(IntPtr messagePtr);

	private delegate void OnSendInviteCallbackInternal(IntPtr messagePtr);

	private delegate void OnUnregisterPlayersCallbackInternal(IntPtr messagePtr);

	private delegate void OnRegisterPlayersCallbackInternal(IntPtr messagePtr);

	private delegate void OnEndSessionCallbackInternal(IntPtr messagePtr);

	private delegate void OnStartSessionCallbackInternal(IntPtr messagePtr);

	private delegate void OnJoinSessionCallbackInternal(IntPtr messagePtr);

	private delegate void OnDestroySessionCallbackInternal(IntPtr messagePtr);

	private delegate void OnUpdateSessionCallbackInternal(IntPtr messagePtr);

	public SessionsInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result CreateSessionModification(CreateSessionModificationOptions options, out SessionModification outSessionModificationHandle)
	{
		object obj = default(CreateSessionModificationOptions_);
		Helper.CopyProperties(options, obj);
		CreateSessionModificationOptions_ options2 = (CreateSessionModificationOptions_)obj;
		outSessionModificationHandle = Helper.GetDefault<SessionModification>();
		IntPtr outSessionModificationHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_CreateSessionModification(base.InnerHandle, ref options2, ref outSessionModificationHandle2);
		options2.Dispose();
		outSessionModificationHandle = ((outSessionModificationHandle2 == IntPtr.Zero) ? null : new SessionModification(outSessionModificationHandle2));
		return result;
	}

	public Result UpdateSessionModification(UpdateSessionModificationOptions options, out SessionModification outSessionModificationHandle)
	{
		object obj = default(UpdateSessionModificationOptions_);
		Helper.CopyProperties(options, obj);
		UpdateSessionModificationOptions_ options2 = (UpdateSessionModificationOptions_)obj;
		outSessionModificationHandle = Helper.GetDefault<SessionModification>();
		IntPtr outSessionModificationHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_UpdateSessionModification(base.InnerHandle, ref options2, ref outSessionModificationHandle2);
		options2.Dispose();
		outSessionModificationHandle = ((outSessionModificationHandle2 == IntPtr.Zero) ? null : new SessionModification(outSessionModificationHandle2));
		return result;
	}

	public void UpdateSession(UpdateSessionOptions options, object clientData, OnUpdateSessionCallback completionDelegate)
	{
		object obj = default(UpdateSessionOptions_);
		Helper.CopyProperties(options, obj);
		UpdateSessionOptions_ options2 = (UpdateSessionOptions_)obj;
		OnUpdateSessionCallbackInternal onUpdateSessionCallbackInternal = OnUpdateSession;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onUpdateSessionCallbackInternal);
		EOS_Sessions_UpdateSession(base.InnerHandle, ref options2, clientDataAddress, onUpdateSessionCallbackInternal);
		options2.Dispose();
	}

	public void DestroySession(DestroySessionOptions options, object clientData, OnDestroySessionCallback completionDelegate)
	{
		object obj = default(DestroySessionOptions_);
		Helper.CopyProperties(options, obj);
		DestroySessionOptions_ options2 = (DestroySessionOptions_)obj;
		OnDestroySessionCallbackInternal onDestroySessionCallbackInternal = OnDestroySession;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onDestroySessionCallbackInternal);
		EOS_Sessions_DestroySession(base.InnerHandle, ref options2, clientDataAddress, onDestroySessionCallbackInternal);
		options2.Dispose();
	}

	public void JoinSession(JoinSessionOptions options, object clientData, OnJoinSessionCallback completionDelegate)
	{
		object obj = default(JoinSessionOptions_);
		Helper.CopyProperties(options, obj);
		JoinSessionOptions_ options2 = (JoinSessionOptions_)obj;
		OnJoinSessionCallbackInternal onJoinSessionCallbackInternal = OnJoinSession;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onJoinSessionCallbackInternal);
		EOS_Sessions_JoinSession(base.InnerHandle, ref options2, clientDataAddress, onJoinSessionCallbackInternal);
		options2.Dispose();
	}

	public void StartSession(StartSessionOptions options, object clientData, OnStartSessionCallback completionDelegate)
	{
		object obj = default(StartSessionOptions_);
		Helper.CopyProperties(options, obj);
		StartSessionOptions_ options2 = (StartSessionOptions_)obj;
		OnStartSessionCallbackInternal onStartSessionCallbackInternal = OnStartSession;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onStartSessionCallbackInternal);
		EOS_Sessions_StartSession(base.InnerHandle, ref options2, clientDataAddress, onStartSessionCallbackInternal);
		options2.Dispose();
	}

	public void EndSession(EndSessionOptions options, object clientData, OnEndSessionCallback completionDelegate)
	{
		object obj = default(EndSessionOptions_);
		Helper.CopyProperties(options, obj);
		EndSessionOptions_ options2 = (EndSessionOptions_)obj;
		OnEndSessionCallbackInternal onEndSessionCallbackInternal = OnEndSession;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onEndSessionCallbackInternal);
		EOS_Sessions_EndSession(base.InnerHandle, ref options2, clientDataAddress, onEndSessionCallbackInternal);
		options2.Dispose();
	}

	public void RegisterPlayers(RegisterPlayersOptions options, object clientData, OnRegisterPlayersCallback completionDelegate)
	{
		object obj = default(RegisterPlayersOptions_);
		Helper.CopyProperties(options, obj);
		RegisterPlayersOptions_ options2 = (RegisterPlayersOptions_)obj;
		OnRegisterPlayersCallbackInternal onRegisterPlayersCallbackInternal = OnRegisterPlayers;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onRegisterPlayersCallbackInternal);
		EOS_Sessions_RegisterPlayers(base.InnerHandle, ref options2, clientDataAddress, onRegisterPlayersCallbackInternal);
		options2.Dispose();
	}

	public void UnregisterPlayers(UnregisterPlayersOptions options, object clientData, OnUnregisterPlayersCallback completionDelegate)
	{
		object obj = default(UnregisterPlayersOptions_);
		Helper.CopyProperties(options, obj);
		UnregisterPlayersOptions_ options2 = (UnregisterPlayersOptions_)obj;
		OnUnregisterPlayersCallbackInternal onUnregisterPlayersCallbackInternal = OnUnregisterPlayers;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, onUnregisterPlayersCallbackInternal);
		EOS_Sessions_UnregisterPlayers(base.InnerHandle, ref options2, clientDataAddress, onUnregisterPlayersCallbackInternal);
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
		EOS_Sessions_SendInvite(base.InnerHandle, ref options2, clientDataAddress, onSendInviteCallbackInternal);
		options2.Dispose();
	}

	public Result CreateSessionSearch(CreateSessionSearchOptions options, out SessionSearch outSessionSearchHandle)
	{
		object obj = default(CreateSessionSearchOptions_);
		Helper.CopyProperties(options, obj);
		CreateSessionSearchOptions_ options2 = (CreateSessionSearchOptions_)obj;
		outSessionSearchHandle = Helper.GetDefault<SessionSearch>();
		IntPtr outSessionSearchHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_CreateSessionSearch(base.InnerHandle, ref options2, ref outSessionSearchHandle2);
		options2.Dispose();
		outSessionSearchHandle = ((outSessionSearchHandle2 == IntPtr.Zero) ? null : new SessionSearch(outSessionSearchHandle2));
		return result;
	}

	public Result CopyActiveSessionHandle(CopyActiveSessionHandleOptions options, out ActiveSession outSessionHandle)
	{
		object obj = default(CopyActiveSessionHandleOptions_);
		Helper.CopyProperties(options, obj);
		CopyActiveSessionHandleOptions_ options2 = (CopyActiveSessionHandleOptions_)obj;
		outSessionHandle = Helper.GetDefault<ActiveSession>();
		IntPtr outSessionHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_CopyActiveSessionHandle(base.InnerHandle, ref options2, ref outSessionHandle2);
		options2.Dispose();
		outSessionHandle = ((outSessionHandle2 == IntPtr.Zero) ? null : new ActiveSession(outSessionHandle2));
		return result;
	}

	public ulong AddNotifySessionInviteReceived(AddNotifySessionInviteReceivedOptions options, object clientData, OnSessionInviteReceivedCallback notificationFn)
	{
		object obj = default(AddNotifySessionInviteReceivedOptions_);
		Helper.CopyProperties(options, obj);
		AddNotifySessionInviteReceivedOptions_ options2 = (AddNotifySessionInviteReceivedOptions_)obj;
		OnSessionInviteReceivedCallbackInternal onSessionInviteReceivedCallbackInternal = OnSessionInviteReceived;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, notificationFn, onSessionInviteReceivedCallbackInternal);
		ulong result = EOS_Sessions_AddNotifySessionInviteReceived(base.InnerHandle, ref options2, clientDataAddress, onSessionInviteReceivedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifySessionInviteReceived(ulong inId)
	{
		EOS_Sessions_RemoveNotifySessionInviteReceived(base.InnerHandle, inId);
	}

	public Result CopySessionHandleByInviteId(CopySessionHandleByInviteIdOptions options, out SessionDetails outSessionHandle)
	{
		object obj = default(CopySessionHandleByInviteIdOptions_);
		Helper.CopyProperties(options, obj);
		CopySessionHandleByInviteIdOptions_ options2 = (CopySessionHandleByInviteIdOptions_)obj;
		outSessionHandle = Helper.GetDefault<SessionDetails>();
		IntPtr outSessionHandle2 = IntPtr.Zero;
		Result result = EOS_Sessions_CopySessionHandleByInviteId(base.InnerHandle, ref options2, ref outSessionHandle2);
		options2.Dispose();
		outSessionHandle = ((outSessionHandle2 == IntPtr.Zero) ? null : new SessionDetails(outSessionHandle2));
		return result;
	}

	public Result DumpSessionState(DumpSessionStateOptions options)
	{
		object obj = default(DumpSessionStateOptions_);
		Helper.CopyProperties(options, obj);
		DumpSessionStateOptions_ options2 = (DumpSessionStateOptions_)obj;
		Result result = EOS_Sessions_DumpSessionState(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	[MonoPInvokeCallback]
	private static void OnSessionInviteReceived(IntPtr messageAddress)
	{
		SessionInviteReceivedCallbackInfo_ sessionInviteReceivedCallbackInfo_ = Marshal.PtrToStructure<SessionInviteReceivedCallbackInfo_>(messageAddress);
		SessionInviteReceivedCallbackInfo sessionInviteReceivedCallbackInfo = new SessionInviteReceivedCallbackInfo();
		Helper.CopyProperties(sessionInviteReceivedCallbackInfo_, sessionInviteReceivedCallbackInfo);
		IntPtr clientDataAddress = sessionInviteReceivedCallbackInfo_.ClientDataAddress;
		sessionInviteReceivedCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, sessionInviteReceivedCallbackInfo) as OnSessionInviteReceivedCallback)(sessionInviteReceivedCallbackInfo);
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
	private static void OnUnregisterPlayers(IntPtr messageAddress)
	{
		UnregisterPlayersCallbackInfo_ unregisterPlayersCallbackInfo_ = Marshal.PtrToStructure<UnregisterPlayersCallbackInfo_>(messageAddress);
		UnregisterPlayersCallbackInfo unregisterPlayersCallbackInfo = new UnregisterPlayersCallbackInfo();
		Helper.CopyProperties(unregisterPlayersCallbackInfo_, unregisterPlayersCallbackInfo);
		IntPtr clientDataAddress = unregisterPlayersCallbackInfo_.ClientDataAddress;
		unregisterPlayersCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, unregisterPlayersCallbackInfo) as OnUnregisterPlayersCallback)(unregisterPlayersCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnRegisterPlayers(IntPtr messageAddress)
	{
		RegisterPlayersCallbackInfo_ registerPlayersCallbackInfo_ = Marshal.PtrToStructure<RegisterPlayersCallbackInfo_>(messageAddress);
		RegisterPlayersCallbackInfo registerPlayersCallbackInfo = new RegisterPlayersCallbackInfo();
		Helper.CopyProperties(registerPlayersCallbackInfo_, registerPlayersCallbackInfo);
		IntPtr clientDataAddress = registerPlayersCallbackInfo_.ClientDataAddress;
		registerPlayersCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, registerPlayersCallbackInfo) as OnRegisterPlayersCallback)(registerPlayersCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnEndSession(IntPtr messageAddress)
	{
		EndSessionCallbackInfo_ endSessionCallbackInfo_ = Marshal.PtrToStructure<EndSessionCallbackInfo_>(messageAddress);
		EndSessionCallbackInfo endSessionCallbackInfo = new EndSessionCallbackInfo();
		Helper.CopyProperties(endSessionCallbackInfo_, endSessionCallbackInfo);
		IntPtr clientDataAddress = endSessionCallbackInfo_.ClientDataAddress;
		endSessionCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, endSessionCallbackInfo) as OnEndSessionCallback)(endSessionCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnStartSession(IntPtr messageAddress)
	{
		StartSessionCallbackInfo_ startSessionCallbackInfo_ = Marshal.PtrToStructure<StartSessionCallbackInfo_>(messageAddress);
		StartSessionCallbackInfo startSessionCallbackInfo = new StartSessionCallbackInfo();
		Helper.CopyProperties(startSessionCallbackInfo_, startSessionCallbackInfo);
		IntPtr clientDataAddress = startSessionCallbackInfo_.ClientDataAddress;
		startSessionCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, startSessionCallbackInfo) as OnStartSessionCallback)(startSessionCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnJoinSession(IntPtr messageAddress)
	{
		JoinSessionCallbackInfo_ joinSessionCallbackInfo_ = Marshal.PtrToStructure<JoinSessionCallbackInfo_>(messageAddress);
		JoinSessionCallbackInfo joinSessionCallbackInfo = new JoinSessionCallbackInfo();
		Helper.CopyProperties(joinSessionCallbackInfo_, joinSessionCallbackInfo);
		IntPtr clientDataAddress = joinSessionCallbackInfo_.ClientDataAddress;
		joinSessionCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, joinSessionCallbackInfo) as OnJoinSessionCallback)(joinSessionCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnDestroySession(IntPtr messageAddress)
	{
		DestroySessionCallbackInfo_ destroySessionCallbackInfo_ = Marshal.PtrToStructure<DestroySessionCallbackInfo_>(messageAddress);
		DestroySessionCallbackInfo destroySessionCallbackInfo = new DestroySessionCallbackInfo();
		Helper.CopyProperties(destroySessionCallbackInfo_, destroySessionCallbackInfo);
		IntPtr clientDataAddress = destroySessionCallbackInfo_.ClientDataAddress;
		destroySessionCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, destroySessionCallbackInfo) as OnDestroySessionCallback)(destroySessionCallbackInfo);
	}

	[MonoPInvokeCallback]
	private static void OnUpdateSession(IntPtr messageAddress)
	{
		UpdateSessionCallbackInfo_ updateSessionCallbackInfo_ = Marshal.PtrToStructure<UpdateSessionCallbackInfo_>(messageAddress);
		UpdateSessionCallbackInfo updateSessionCallbackInfo = new UpdateSessionCallbackInfo();
		Helper.CopyProperties(updateSessionCallbackInfo_, updateSessionCallbackInfo);
		IntPtr clientDataAddress = updateSessionCallbackInfo_.ClientDataAddress;
		updateSessionCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, updateSessionCallbackInfo) as OnUpdateSessionCallback)(updateSessionCallbackInfo);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Sessions_DumpSessionState(IntPtr handle, ref DumpSessionStateOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Sessions_CopySessionHandleByInviteId(IntPtr handle, ref CopySessionHandleByInviteIdOptions_ options, ref IntPtr outSessionHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Sessions_RemoveNotifySessionInviteReceived(IntPtr handle, ulong inId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern ulong EOS_Sessions_AddNotifySessionInviteReceived(IntPtr handle, ref AddNotifySessionInviteReceivedOptions_ options, IntPtr clientData, OnSessionInviteReceivedCallbackInternal notificationFn);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Sessions_CopyActiveSessionHandle(IntPtr handle, ref CopyActiveSessionHandleOptions_ options, ref IntPtr outSessionHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Sessions_CreateSessionSearch(IntPtr handle, ref CreateSessionSearchOptions_ options, ref IntPtr outSessionSearchHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Sessions_SendInvite(IntPtr handle, ref SendInviteOptions_ options, IntPtr clientData, OnSendInviteCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Sessions_UnregisterPlayers(IntPtr handle, ref UnregisterPlayersOptions_ options, IntPtr clientData, OnUnregisterPlayersCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Sessions_RegisterPlayers(IntPtr handle, ref RegisterPlayersOptions_ options, IntPtr clientData, OnRegisterPlayersCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Sessions_EndSession(IntPtr handle, ref EndSessionOptions_ options, IntPtr clientData, OnEndSessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Sessions_StartSession(IntPtr handle, ref StartSessionOptions_ options, IntPtr clientData, OnStartSessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Sessions_JoinSession(IntPtr handle, ref JoinSessionOptions_ options, IntPtr clientData, OnJoinSessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Sessions_DestroySession(IntPtr handle, ref DestroySessionOptions_ options, IntPtr clientData, OnDestroySessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Sessions_UpdateSession(IntPtr handle, ref UpdateSessionOptions_ options, IntPtr clientData, OnUpdateSessionCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Sessions_UpdateSessionModification(IntPtr handle, ref UpdateSessionModificationOptions_ options, ref IntPtr outSessionModificationHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Sessions_CreateSessionModification(IntPtr handle, ref CreateSessionModificationOptions_ options, ref IntPtr outSessionModificationHandle);
}
