using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CloseConnectionsOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_SocketId;

	public int ApiVersion
	{
		get
		{
			return m_ApiVersion;
		}
		set
		{
			m_ApiVersion = value;
		}
	}

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
		set
		{
			m_LocalUserId = ((value == null) ? IntPtr.Zero : value.InnerHandle);
		}
	}

	public SocketId_ SocketId
	{
		get
		{
			return Helper.GetAllocation<SocketId_>(m_SocketId);
		}
		set
		{
			Helper.RegisterAllocation(ref m_SocketId, value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_SocketId);
	}
}
