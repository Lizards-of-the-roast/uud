using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionSearchSetTargetUserIdOptions_ : IDisposable
{
	private int m_ApiVersion;

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

	public ProductUserId TargetUserId
	{
		get
		{
			if (!(m_TargetUserId == IntPtr.Zero))
			{
				return new ProductUserId(m_TargetUserId);
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
