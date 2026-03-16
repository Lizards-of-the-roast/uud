using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SetPresenceOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_PresenceModificationHandle;

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

	public PresenceModification PresenceModificationHandle
	{
		get
		{
			if (!(m_PresenceModificationHandle == IntPtr.Zero))
			{
				return new PresenceModification(m_PresenceModificationHandle);
			}
			return null;
		}
		set
		{
			m_PresenceModificationHandle = ((value == null) ? IntPtr.Zero : value.InnerHandle);
		}
	}

	public void Dispose()
	{
	}
}
