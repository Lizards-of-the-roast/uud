using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct OnRemoteConnectionClosedInfo_ : IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_RemoteUserId;

	private IntPtr m_SocketId;

	private ConnectionClosedReason m_Reason;

	public object ClientData => Helper.GetAllocation<BoxedClientData>(m_ClientData).ClientData;

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId LocalUserId
	{
		get
		{
			if (!(m_LocalUserId == IntPtr.Zero))
			{
				return new ProductUserId(m_LocalUserId);
			}
			return null;
		}
	}

	public ProductUserId RemoteUserId
	{
		get
		{
			if (!(m_RemoteUserId == IntPtr.Zero))
			{
				return new ProductUserId(m_RemoteUserId);
			}
			return null;
		}
	}

	public SocketId_ SocketId => Helper.GetAllocation<SocketId_>(m_SocketId);

	public ConnectionClosedReason Reason => m_Reason;

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
