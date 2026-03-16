using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct Info_ : IDisposable
{
	private int m_ApiVersion;

	private Status m_Status;

	private IntPtr m_UserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ProductId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ProductVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_Platform;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_RichText;

	private int m_RecordsCount;

	private IntPtr m_Records;

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

	public Status Status
	{
		get
		{
			return m_Status;
		}
		set
		{
			m_Status = value;
		}
	}

	public EpicAccountId UserId
	{
		get
		{
			if (!(m_UserId == IntPtr.Zero))
			{
				return new EpicAccountId(m_UserId);
			}
			return null;
		}
		set
		{
			m_UserId = ((value == null) ? IntPtr.Zero : value.InnerHandle);
		}
	}

	public string ProductId
	{
		get
		{
			return m_ProductId;
		}
		set
		{
			m_ProductId = value;
		}
	}

	public string ProductVersion
	{
		get
		{
			return m_ProductVersion;
		}
		set
		{
			m_ProductVersion = value;
		}
	}

	public string Platform
	{
		get
		{
			return m_Platform;
		}
		set
		{
			m_Platform = value;
		}
	}

	public string RichText
	{
		get
		{
			return m_RichText;
		}
		set
		{
			m_RichText = value;
		}
	}

	public DataRecord_[] Records
	{
		get
		{
			return Helper.GetAllocation<DataRecord_[]>(m_Records, m_RecordsCount);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Records, value);
			m_RecordsCount = value.Length;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Records);
	}
}
