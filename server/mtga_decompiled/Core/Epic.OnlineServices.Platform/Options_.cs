using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct Options_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_Reserved;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ProductId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SandboxId;

	private ClientCredentials_ m_ClientCredentials;

	private int m_IsServer;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_EncryptionKey;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OverrideCountryCode;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OverrideLocaleCode;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DeploymentId;

	private ulong m_Flags;

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

	public IntPtr Reserved
	{
		get
		{
			return m_Reserved;
		}
		set
		{
			m_Reserved = value;
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

	public string SandboxId
	{
		get
		{
			return m_SandboxId;
		}
		set
		{
			m_SandboxId = value;
		}
	}

	public ClientCredentials_ ClientCredentials
	{
		get
		{
			return m_ClientCredentials;
		}
		set
		{
			m_ClientCredentials = value;
		}
	}

	public bool IsServer
	{
		get
		{
			return m_IsServer != 0;
		}
		set
		{
			m_IsServer = (value ? 1 : 0);
		}
	}

	public string EncryptionKey
	{
		get
		{
			return m_EncryptionKey;
		}
		set
		{
			m_EncryptionKey = value;
		}
	}

	public string OverrideCountryCode
	{
		get
		{
			return m_OverrideCountryCode;
		}
		set
		{
			m_OverrideCountryCode = value;
		}
	}

	public string OverrideLocaleCode
	{
		get
		{
			return m_OverrideLocaleCode;
		}
		set
		{
			m_OverrideLocaleCode = value;
		}
	}

	public string DeploymentId
	{
		get
		{
			return m_DeploymentId;
		}
		set
		{
			m_DeploymentId = value;
		}
	}

	public ulong Flags
	{
		get
		{
			return m_Flags;
		}
		set
		{
			m_Flags = value;
		}
	}

	public void Dispose()
	{
	}
}
