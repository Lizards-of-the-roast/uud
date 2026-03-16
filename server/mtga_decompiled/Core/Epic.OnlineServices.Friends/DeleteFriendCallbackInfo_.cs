using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Friends;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct DeleteFriendCallbackInfo_ : IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

	public Result ResultCode => m_ResultCode;

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

	public EpicAccountId TargetUserId
	{
		get
		{
			if (!(m_TargetUserId == IntPtr.Zero))
			{
				return new EpicAccountId(m_TargetUserId);
			}
			return null;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
