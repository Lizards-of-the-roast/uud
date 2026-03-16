using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct RedeemEntitlementsOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	private uint m_EntitlementInstanceIdCount;

	private IntPtr m_EntitlementInstanceIds;

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

	public string[] EntitlementInstanceIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_EntitlementInstanceIds, (int)m_EntitlementInstanceIdCount);
		}
		set
		{
			Helper.RegisterAllocation(ref m_EntitlementInstanceIds, value);
			m_EntitlementInstanceIdCount = (uint)value.Length;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_EntitlementInstanceIds);
	}
}
