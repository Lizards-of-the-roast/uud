using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CatalogRelease_ : IDisposable
{
	private int m_ApiVersion;

	private uint m_CompatibleAppIdCount;

	private IntPtr m_CompatibleAppIds;

	private uint m_CompatiblePlatformCount;

	private IntPtr m_CompatiblePlatforms;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ReleaseNote;

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

	public string[] CompatibleAppIds
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_CompatibleAppIds, (int)m_CompatibleAppIdCount);
		}
		set
		{
			Helper.RegisterAllocation(ref m_CompatibleAppIds, value);
			m_CompatibleAppIdCount = (uint)value.Length;
		}
	}

	public string[] CompatiblePlatforms
	{
		get
		{
			return Helper.GetAllocation<string[]>(m_CompatiblePlatforms, (int)m_CompatiblePlatformCount);
		}
		set
		{
			Helper.RegisterAllocation(ref m_CompatiblePlatforms, value);
			m_CompatiblePlatformCount = (uint)value.Length;
		}
	}

	public string ReleaseNote
	{
		get
		{
			return m_ReleaseNote;
		}
		set
		{
			m_ReleaseNote = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_CompatibleAppIds);
		Helper.ReleaseAllocation(ref m_CompatiblePlatforms);
	}
}
