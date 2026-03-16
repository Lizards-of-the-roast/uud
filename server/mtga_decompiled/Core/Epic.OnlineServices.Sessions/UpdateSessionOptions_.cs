using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UpdateSessionOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_SessionModificationHandle;

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

	public SessionModification SessionModificationHandle
	{
		get
		{
			if (!(m_SessionModificationHandle == IntPtr.Zero))
			{
				return new SessionModification(m_SessionModificationHandle);
			}
			return null;
		}
		set
		{
			m_SessionModificationHandle = ((value == null) ? IntPtr.Zero : value.InnerHandle);
		}
	}

	public void Dispose()
	{
	}
}
