using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionInviteReceivedCallbackInfo_ : IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_InviteId;

	public object ClientData => Helper.GetAllocation<BoxedClientData>(m_ClientData).ClientData;

	public IntPtr ClientDataAddress => m_ClientData;

	public ProductUserId LocalUserId
	{
		get
		{
			if (!(m_LocalUserId == IntPtr.Zero))
			{
				return new ProductUserId(m_LocalUserId);
			}
			return null;
		}
	}

	public ProductUserId TargetUserId
	{
		get
		{
			if (!(m_TargetUserId == IntPtr.Zero))
			{
				return new ProductUserId(m_TargetUserId);
			}
			return null;
		}
	}

	public string InviteId => m_InviteId;

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
