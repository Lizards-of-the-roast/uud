using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

public sealed class P2PInterface : Handle
{
	private delegate void OnQueryNATTypeCompleteCallbackInternal(IntPtr messagePtr);

	private delegate void OnRemoteConnectionClosedCallbackInternal(IntPtr messagePtr);

	private delegate void OnIncomingConnectionRequestCallbackInternal(IntPtr messagePtr);

	public P2PInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SendPacket(SendPacketOptions options)
	{
		object obj = default(SendPacketOptions_);
		Helper.CopyProperties(options, obj);
		SendPacketOptions_ options2 = (SendPacketOptions_)obj;
		Result result = EOS_P2P_SendPacket(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result GetNextReceivedPacketSize(GetNextReceivedPacketSizeOptions options, out uint outPacketSizeBytes)
	{
		object obj = default(GetNextReceivedPacketSizeOptions_);
		Helper.CopyProperties(options, obj);
		GetNextReceivedPacketSizeOptions_ options2 = (GetNextReceivedPacketSizeOptions_)obj;
		outPacketSizeBytes = Helper.GetDefault<uint>();
		Result result = EOS_P2P_GetNextReceivedPacketSize(base.InnerHandle, ref options2, ref outPacketSizeBytes);
		options2.Dispose();
		return result;
	}

	public Result ReceivePacket(ReceivePacketOptions options, out ProductUserId outPeerId, out SocketId outSocketId, out byte outChannel, ref byte[] outData, out uint outBytesWritten)
	{
		object obj = default(ReceivePacketOptions_);
		Helper.CopyProperties(options, obj);
		ReceivePacketOptions_ options2 = (ReceivePacketOptions_)obj;
		outPeerId = Helper.GetDefault<ProductUserId>();
		IntPtr outPeerId2 = IntPtr.Zero;
		outSocketId = Helper.GetDefault<SocketId>();
		SocketId_ outSocketId2 = default(SocketId_);
		outChannel = Helper.GetDefault<byte>();
		outBytesWritten = Helper.GetDefault<uint>();
		Result result = EOS_P2P_ReceivePacket(base.InnerHandle, ref options2, ref outPeerId2, ref outSocketId2, ref outChannel, outData, ref outBytesWritten);
		options2.Dispose();
		outPeerId = ((outPeerId2 == IntPtr.Zero) ? null : new ProductUserId(outPeerId2));
		outSocketId = new SocketId();
		Helper.CopyProperties(outSocketId2, outSocketId);
		outSocketId2.Dispose();
		return result;
	}

	public ulong AddNotifyPeerConnectionRequest(AddNotifyPeerConnectionRequestOptions options, object clientData, OnIncomingConnectionRequestCallback connectionRequestHandler)
	{
		object obj = default(AddNotifyPeerConnectionRequestOptions_);
		Helper.CopyProperties(options, obj);
		AddNotifyPeerConnectionRequestOptions_ options2 = (AddNotifyPeerConnectionRequestOptions_)obj;
		OnIncomingConnectionRequestCallbackInternal onIncomingConnectionRequestCallbackInternal = OnIncomingConnectionRequest;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, connectionRequestHandler, onIncomingConnectionRequestCallbackInternal);
		ulong result = EOS_P2P_AddNotifyPeerConnectionRequest(base.InnerHandle, ref options2, clientDataAddress, onIncomingConnectionRequestCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyPeerConnectionRequest(ulong notificationId)
	{
		EOS_P2P_RemoveNotifyPeerConnectionRequest(base.InnerHandle, notificationId);
	}

	public ulong AddNotifyPeerConnectionClosed(AddNotifyPeerConnectionClosedOptions options, object clientData, OnRemoteConnectionClosedCallback connectionClosedHandler)
	{
		object obj = default(AddNotifyPeerConnectionClosedOptions_);
		Helper.CopyProperties(options, obj);
		AddNotifyPeerConnectionClosedOptions_ options2 = (AddNotifyPeerConnectionClosedOptions_)obj;
		OnRemoteConnectionClosedCallbackInternal onRemoteConnectionClosedCallbackInternal = OnRemoteConnectionClosed;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, connectionClosedHandler, onRemoteConnectionClosedCallbackInternal);
		ulong result = EOS_P2P_AddNotifyPeerConnectionClosed(base.InnerHandle, ref options2, clientDataAddress, onRemoteConnectionClosedCallbackInternal);
		options2.Dispose();
		return result;
	}

	public void RemoveNotifyPeerConnectionClosed(ulong notificationId)
	{
		EOS_P2P_RemoveNotifyPeerConnectionClosed(base.InnerHandle, notificationId);
	}

	public Result AcceptConnection(AcceptConnectionOptions options)
	{
		object obj = default(AcceptConnectionOptions_);
		Helper.CopyProperties(options, obj);
		AcceptConnectionOptions_ options2 = (AcceptConnectionOptions_)obj;
		Result result = EOS_P2P_AcceptConnection(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CloseConnection(CloseConnectionOptions options)
	{
		object obj = default(CloseConnectionOptions_);
		Helper.CopyProperties(options, obj);
		CloseConnectionOptions_ options2 = (CloseConnectionOptions_)obj;
		Result result = EOS_P2P_CloseConnection(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CloseConnections(CloseConnectionsOptions options)
	{
		object obj = default(CloseConnectionsOptions_);
		Helper.CopyProperties(options, obj);
		CloseConnectionsOptions_ options2 = (CloseConnectionsOptions_)obj;
		Result result = EOS_P2P_CloseConnections(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public void QueryNATType(QueryNATTypeOptions options, object clientData, OnQueryNATTypeCompleteCallback nATTypeQueriedHandler)
	{
		object obj = default(QueryNATTypeOptions_);
		Helper.CopyProperties(options, obj);
		QueryNATTypeOptions_ options2 = (QueryNATTypeOptions_)obj;
		OnQueryNATTypeCompleteCallbackInternal onQueryNATTypeCompleteCallbackInternal = OnQueryNATTypeComplete;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, nATTypeQueriedHandler, onQueryNATTypeCompleteCallbackInternal);
		EOS_P2P_QueryNATType(base.InnerHandle, ref options2, clientDataAddress, onQueryNATTypeCompleteCallbackInternal);
		options2.Dispose();
	}

	public Result GetNATType(GetNATTypeOptions options, out NATType outNATType)
	{
		object obj = default(GetNATTypeOptions_);
		Helper.CopyProperties(options, obj);
		GetNATTypeOptions_ options2 = (GetNATTypeOptions_)obj;
		outNATType = Helper.GetDefault<NATType>();
		Result result = EOS_P2P_GetNATType(base.InnerHandle, ref options2, ref outNATType);
		options2.Dispose();
		return result;
	}

	[MonoPInvokeCallback]
	private static void OnQueryNATTypeComplete(IntPtr messageAddress)
	{
		OnQueryNATTypeCompleteInfo_ onQueryNATTypeCompleteInfo_ = Marshal.PtrToStructure<OnQueryNATTypeCompleteInfo_>(messageAddress);
		OnQueryNATTypeCompleteInfo onQueryNATTypeCompleteInfo = new OnQueryNATTypeCompleteInfo();
		Helper.CopyProperties(onQueryNATTypeCompleteInfo_, onQueryNATTypeCompleteInfo);
		IntPtr clientDataAddress = onQueryNATTypeCompleteInfo_.ClientDataAddress;
		onQueryNATTypeCompleteInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, onQueryNATTypeCompleteInfo) as OnQueryNATTypeCompleteCallback)(onQueryNATTypeCompleteInfo);
	}

	[MonoPInvokeCallback]
	private static void OnRemoteConnectionClosed(IntPtr messageAddress)
	{
		OnRemoteConnectionClosedInfo_ onRemoteConnectionClosedInfo_ = Marshal.PtrToStructure<OnRemoteConnectionClosedInfo_>(messageAddress);
		OnRemoteConnectionClosedInfo onRemoteConnectionClosedInfo = new OnRemoteConnectionClosedInfo();
		Helper.CopyProperties(onRemoteConnectionClosedInfo_, onRemoteConnectionClosedInfo);
		IntPtr clientDataAddress = onRemoteConnectionClosedInfo_.ClientDataAddress;
		onRemoteConnectionClosedInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, onRemoteConnectionClosedInfo) as OnRemoteConnectionClosedCallback)(onRemoteConnectionClosedInfo);
	}

	[MonoPInvokeCallback]
	private static void OnIncomingConnectionRequest(IntPtr messageAddress)
	{
		OnIncomingConnectionRequestInfo_ onIncomingConnectionRequestInfo_ = Marshal.PtrToStructure<OnIncomingConnectionRequestInfo_>(messageAddress);
		OnIncomingConnectionRequestInfo onIncomingConnectionRequestInfo = new OnIncomingConnectionRequestInfo();
		Helper.CopyProperties(onIncomingConnectionRequestInfo_, onIncomingConnectionRequestInfo);
		IntPtr clientDataAddress = onIncomingConnectionRequestInfo_.ClientDataAddress;
		onIncomingConnectionRequestInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, onIncomingConnectionRequestInfo) as OnIncomingConnectionRequestCallback)(onIncomingConnectionRequestInfo);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_P2P_GetNATType(IntPtr handle, ref GetNATTypeOptions_ options, ref NATType outNATType);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_P2P_QueryNATType(IntPtr handle, ref QueryNATTypeOptions_ options, IntPtr clientData, OnQueryNATTypeCompleteCallbackInternal nATTypeQueriedHandler);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_P2P_CloseConnections(IntPtr handle, ref CloseConnectionsOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_P2P_CloseConnection(IntPtr handle, ref CloseConnectionOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_P2P_AcceptConnection(IntPtr handle, ref AcceptConnectionOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_P2P_RemoveNotifyPeerConnectionClosed(IntPtr handle, ulong notificationId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern ulong EOS_P2P_AddNotifyPeerConnectionClosed(IntPtr handle, ref AddNotifyPeerConnectionClosedOptions_ options, IntPtr clientData, OnRemoteConnectionClosedCallbackInternal connectionClosedHandler);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_P2P_RemoveNotifyPeerConnectionRequest(IntPtr handle, ulong notificationId);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern ulong EOS_P2P_AddNotifyPeerConnectionRequest(IntPtr handle, ref AddNotifyPeerConnectionRequestOptions_ options, IntPtr clientData, OnIncomingConnectionRequestCallbackInternal connectionRequestHandler);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_P2P_ReceivePacket(IntPtr handle, ref ReceivePacketOptions_ options, ref IntPtr outPeerId, ref SocketId_ outSocketId, ref byte outChannel, byte[] outData, ref uint outBytesWritten);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_P2P_GetNextReceivedPacketSize(IntPtr handle, ref GetNextReceivedPacketSizeOptions_ options, ref uint outPacketSizeBytes);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_P2P_SendPacket(IntPtr handle, ref SendPacketOptions_ options);
}
