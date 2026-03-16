using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct Token_ : IDisposable
{
	private int m_ApiVersion;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_App;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ClientId;

	private IntPtr m_AccountId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_AccessToken;

	private double m_ExpiresIn;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ExpiresAt;

	private AuthTokenType m_AuthType;

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

	public string App
	{
		get
		{
			return m_App;
		}
		set
		{
			m_App = value;
		}
	}

	public string ClientId
	{
		get
		{
			return m_ClientId;
		}
		set
		{
			m_ClientId = value;
		}
	}

	public EpicAccountId AccountId
	{
		get
		{
			if (!(m_AccountId == IntPtr.Zero))
			{
				return new EpicAccountId(m_AccountId);
			}
			return null;
		}
		set
		{
			m_AccountId = ((value == null) ? IntPtr.Zero : value.InnerHandle);
		}
	}

	public string AccessToken
	{
		get
		{
			return m_AccessToken;
		}
		set
		{
			m_AccessToken = value;
		}
	}

	public double ExpiresIn
	{
		get
		{
			return m_ExpiresIn;
		}
		set
		{
			m_ExpiresIn = value;
		}
	}

	public string ExpiresAt
	{
		get
		{
			return m_ExpiresAt;
		}
		set
		{
			m_ExpiresAt = value;
		}
	}

	public AuthTokenType AuthType
	{
		get
		{
			return m_AuthType;
		}
		set
		{
			m_AuthType = value;
		}
	}

	public void Dispose()
	{
	}
}
