using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct VerifyUserAuthOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_AuthToken;

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

	public Token_ AuthToken
	{
		get
		{
			return Helper.GetAllocation<Token_>(m_AuthToken);
		}
		set
		{
			Helper.RegisterAllocation(ref m_AuthToken, value);
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_AuthToken);
	}
}
