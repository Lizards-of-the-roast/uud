using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SendPacketOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_RemoteUserId;

	private IntPtr m_SocketId;

	private byte m_Channel;

	private uint m_DataLengthBytes;

	private IntPtr m_Data;

	private int m_AllowDelayedDelivery;

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
		set
		{
			m_RemoteUserId = ((value == null) ? IntPtr.Zero : value.InnerHandle);
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

	public byte Channel
	{
		get
		{
			return m_Channel;
		}
		set
		{
			m_Channel = value;
		}
	}

	public byte[] Data
	{
		get
		{
			return Helper.GetAllocation<byte[]>(m_Data, (int)m_DataLengthBytes);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Data, value);
			m_DataLengthBytes = (uint)value.Length;
		}
	}

	public bool AllowDelayedDelivery
	{
		get
		{
			return m_AllowDelayedDelivery != 0;
		}
		set
		{
			m_AllowDelayedDelivery = (value ? 1 : 0);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_SocketId);
		Helper.ReleaseAllocation(ref m_Data);
	}
}
