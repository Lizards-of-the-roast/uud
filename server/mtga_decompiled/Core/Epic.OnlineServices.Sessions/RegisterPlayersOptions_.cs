using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct RegisterPlayersOptions_ : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	private IntPtr m_PlayersToRegister;

	private uint m_PlayersToRegisterCount;

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

	public ProductUserId[] PlayersToRegister
	{
		get
		{
			return (from address in Helper.GetAllocation<IntPtr[]>(m_PlayersToRegister, (int)m_PlayersToRegisterCount)
				select new ProductUserId(address)).ToArray();
		}
		set
		{
			Helper.RegisterAllocation(ref m_PlayersToRegister, value.Select((ProductUserId handle) => handle.InnerHandle).ToArray());
			m_PlayersToRegisterCount = (uint)value.Length;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_PlayersToRegister);
	}
}
