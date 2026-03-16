using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct ReceivePacketOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private uint m_MaxDataSizeBytes;

	private byte m_RequestedChannel;

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

	public uint MaxDataSizeBytes
	{
		get
		{
			return m_MaxDataSizeBytes;
		}
		set
		{
			m_MaxDataSizeBytes = value;
		}
	}

	public byte RequestedChannel
	{
		get
		{
			return m_RequestedChannel;
		}
		set
		{
			m_RequestedChannel = value;
		}
	}

	public void Dispose()
	{
	}
}
