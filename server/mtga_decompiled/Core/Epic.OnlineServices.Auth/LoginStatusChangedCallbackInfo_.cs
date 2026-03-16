using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LoginStatusChangedCallbackInfo_ : IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private LoginStatus m_PrevStatus;

	private LoginStatus m_CurrentStatus;

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

	public LoginStatus PrevStatus => m_PrevStatus;

	public LoginStatus CurrentStatus => m_CurrentStatus;

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
