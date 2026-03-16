using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct Entitlement_ : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Id;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_InstanceId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_CatalogItemId;

	private int m_ServerIndex;

	private int m_Redeemed;

	private long m_EndTimestamp;

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

	public string Id
	{
		get
		{
			return m_Id;
		}
		set
		{
			m_Id = value;
		}
	}

	public string InstanceId
	{
		get
		{
			return m_InstanceId;
		}
		set
		{
			m_InstanceId = value;
		}
	}

	public string CatalogItemId
	{
		get
		{
			return m_CatalogItemId;
		}
		set
		{
			m_CatalogItemId = value;
		}
	}

	public int ServerIndex
	{
		get
		{
			return m_ServerIndex;
		}
		set
		{
			m_ServerIndex = value;
		}
	}

	public bool Redeemed
	{
		get
		{
			return m_Redeemed != 0;
		}
		set
		{
			m_Redeemed = (value ? 1 : 0);
		}
	}

	public long EndTimestamp
	{
		get
		{
			return m_EndTimestamp;
		}
		set
		{
			m_EndTimestamp = value;
		}
	}

	public void Dispose()
	{
	}
}
