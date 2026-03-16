using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryExternalAccountMappingsOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private ExternalAccountType m_AccountIdType;

	private IntPtr m_ExternalAccountIds;

	private uint m_ExternalAccountIdCount;

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

	public ExternalAccountType AccountIdType
	{
		get
		{
			return m_AccountIdType;
		}
		set
		{
			m_AccountIdType = value;
		}
	}

	public string[] ExternalAccountIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_ExternalAccountIds, (int)m_ExternalAccountIdCount);
		}
		set
		{
			Helper.RegisterAllocation(ref m_ExternalAccountIds, value);
			m_ExternalAccountIdCount = (uint)value.Length;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ExternalAccountIds);
	}
}
