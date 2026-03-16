using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryOwnershipTokenOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_CatalogItemIds;

	private uint m_CatalogItemIdCount;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_CatalogNamespace;

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

	public string[] CatalogItemIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_CatalogItemIds, (int)m_CatalogItemIdCount);
		}
		set
		{
			Helper.RegisterAllocation(ref m_CatalogItemIds, value);
			m_CatalogItemIdCount = (uint)value.Length;
		}
	}

	public string CatalogNamespace
	{
		get
		{
			return m_CatalogNamespace;
		}
		set
		{
			m_CatalogNamespace = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_CatalogItemIds);
	}
}
