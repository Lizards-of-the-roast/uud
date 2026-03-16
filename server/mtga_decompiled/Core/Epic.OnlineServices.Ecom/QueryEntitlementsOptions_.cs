using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryEntitlementsOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private IntPtr m_EntitlementIds;

	private uint m_EntitlementIdCount;

	private int m_IncludeRedeemed;

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

	public string[] EntitlementIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_EntitlementIds, (int)m_EntitlementIdCount);
		}
		set
		{
			Helper.RegisterAllocation(ref m_EntitlementIds, value);
			m_EntitlementIdCount = (uint)value.Length;
		}
	}

	public bool IncludeRedeemed
	{
		get
		{
			return m_IncludeRedeemed != 0;
		}
		set
		{
			m_IncludeRedeemed = (value ? 1 : 0);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_EntitlementIds);
	}
}
