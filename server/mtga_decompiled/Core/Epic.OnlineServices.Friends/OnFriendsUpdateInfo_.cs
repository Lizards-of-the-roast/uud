using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Friends;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct OnFriendsUpdateInfo_ : IDisposable
{
	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

	private FriendsStatus m_PreviousStatus;

	private FriendsStatus m_CurrentStatus;

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

	public FriendsStatus PreviousStatus => m_PreviousStatus;

	public FriendsStatus CurrentStatus => m_CurrentStatus;

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
