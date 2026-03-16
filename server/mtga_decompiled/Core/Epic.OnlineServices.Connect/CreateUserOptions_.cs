using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CreateUserOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_ContinuanceToken;

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

	public ContinuanceToken ContinuanceToken
	{
		get
		{
			if (!(m_ContinuanceToken == IntPtr.Zero))
			{
				return new ContinuanceToken(m_ContinuanceToken);
			}
			return null;
		}
		set
		{
			m_ContinuanceToken = ((value == null) ? IntPtr.Zero : value.InnerHandle);
		}
	}

	public void Dispose()
	{
	}
}
