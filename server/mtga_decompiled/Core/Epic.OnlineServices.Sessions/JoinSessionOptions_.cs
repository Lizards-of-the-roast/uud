using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct JoinSessionOptions_ : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	private IntPtr m_SessionHandle;

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

	public SessionDetails SessionHandle
	{
		get
		{
			if (!(m_SessionHandle == IntPtr.Zero))
			{
				return new SessionDetails(m_SessionHandle);
			}
			return null;
		}
		set
		{
			m_SessionHandle = ((value == null) ? IntPtr.Zero : value.InnerHandle);
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
