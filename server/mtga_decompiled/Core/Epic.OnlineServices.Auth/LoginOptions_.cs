using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LoginOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_Credentials;

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

	public Credentials_ Credentials
	{
		get
		{
			return Helper.GetAllocation<Credentials_>(m_Credentials);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Credentials, value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Credentials);
	}
}
