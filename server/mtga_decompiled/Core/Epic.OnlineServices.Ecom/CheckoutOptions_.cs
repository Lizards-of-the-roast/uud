using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CheckoutOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OverrideCatalogNamespace;

	private uint m_EntryCount;

	private IntPtr m_Entries;

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

	public string OverrideCatalogNamespace
	{
		get
		{
			return m_OverrideCatalogNamespace;
		}
		set
		{
			m_OverrideCatalogNamespace = value;
		}
	}

	public CheckoutEntry_[] Entries
	{
		get
		{
			return Helper.GetAllocation<CheckoutEntry_[]>(m_Entries, (int)m_EntryCount);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Entries, value);
			m_EntryCount = (uint)value.Length;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Entries);
	}
}
