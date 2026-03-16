using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct PresenceModificationSetDataOptions_ : IDisposable
{
	private int m_ApiVersion;

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
