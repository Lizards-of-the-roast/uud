using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyPresenceOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

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

	public EpicAccountId LocalUserId
	{
		get
		{
			if (!(m_LocalUserId == IntPtr.Zero))
			{
				return new EpicAccountId(m_LocalUserId);
			}
			return null;
		}
		set
		{
			m_LocalUserId = ((value == null) ? IntPtr.Zero : value.InnerHandle);
		}
	}

	public EpicAccountId TargetUserId
	{
		get
		{
			if (!(m_TargetUserId == IntPtr.Zero))
			{
				return new EpicAccountId(m_TargetUserId);
			}
			return null;
		}
		set
		{
			m_TargetUserId = ((value == null) ? IntPtr.Zero : value.InnerHandle);
		}
	}

	public void Dispose()
	{
	}
}
