using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionModificationSetJoinInProgressAllowedOptions_ : IDisposable
{
	private int m_ApiVersion;

	private int m_AllowJoinInProgress;

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

	public bool AllowJoinInProgress
	{
		get
		{
			return m_AllowJoinInProgress != 0;
		}
		set
		{
			m_AllowJoinInProgress = (value ? 1 : 0);
		}
	}

	public void Dispose()
	{
	}
}
