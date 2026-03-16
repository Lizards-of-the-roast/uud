using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CreateSessionModificationOptions_ : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_BucketId;

	private uint m_MaxPlayers;

	private IntPtr m_LocalUserId;

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

	public string SessionName
	{
		get
		{
			return m_SessionName;
		}
		set
		{
			m_SessionName = value;
		}
	}

	public string BucketId
	{
		get
		{
			return m_BucketId;
		}
		set
		{
			m_BucketId = value;
		}
	}

	public uint MaxPlayers
	{
		get
		{
			return m_MaxPlayers;
		}
		set
		{
			m_MaxPlayers = value;
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

	public void Dispose()
	{
	}
}
