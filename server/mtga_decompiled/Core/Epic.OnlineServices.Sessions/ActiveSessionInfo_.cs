using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct ActiveSessionInfo_ : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	private IntPtr m_LocalUserId;

	private OnlineSessionState m_State;

	private IntPtr m_SessionDetails;

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

	public OnlineSessionState State
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	public SessionDetailsInfo_ SessionDetails
	{
		get
		{
			return Helper.GetAllocation<SessionDetailsInfo_>(m_SessionDetails);
		}
		set
		{
			Helper.RegisterAllocation(ref m_SessionDetails, value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_SessionDetails);
	}
}
