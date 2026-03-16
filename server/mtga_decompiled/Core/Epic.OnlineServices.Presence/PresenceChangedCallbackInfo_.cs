using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct PresenceChangedCallbackInfo_ : IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_PresenceUserId;

	public object ClientData => Helper.GetAllocation<BoxedClientData>(m_ClientData).ClientData;

	public IntPtr ClientDataAddress => m_ClientData;

	public EpicAccountId LocalUserId
	{
		get
		{
			if (!(m_LocalUserId == IntPtr.Zero))
			{
				return new EpicAccountId(m_LocalUserId);
			}
			return null;
		}
	}

	public EpicAccountId PresenceUserId
	{
		get
		{
			if (!(m_PresenceUserId == IntPtr.Zero))
			{
				return new EpicAccountId(m_PresenceUserId);
			}
			return null;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
