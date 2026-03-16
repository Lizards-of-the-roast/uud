using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UnregisterPlayersOptions_ : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	private IntPtr m_PlayersToUnregister;

	private uint m_PlayersToUnregisterCount;

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

	public string SessionName
	{
		get
		{
			return m_SessionName;
		}
		set
		{
			m_SessionName = value;
		}
	}

	public ProductUserId[] PlayersToUnregister
	{
		get
		{
			return (from address in Helper.GetAllocation<IntPtr[]>(m_PlayersToUnregister, (int)m_PlayersToUnregisterCount)
				select new ProductUserId(address)).ToArray();
		}
		set
		{
			Helper.RegisterAllocation(ref m_PlayersToUnregister, value.Select((ProductUserId handle) => handle.InnerHandle).ToArray());
			m_PlayersToUnregisterCount = (uint)value.Length;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_PlayersToUnregister);
	}
}
