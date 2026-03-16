using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyEntitlementByIndexOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private uint m_EntitlementIndex;

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

	public uint EntitlementIndex
	{
		get
		{
			return m_EntitlementIndex;
		}
		set
		{
			m_EntitlementIndex = value;
		}
	}

	public void Dispose()
	{
	}
}
