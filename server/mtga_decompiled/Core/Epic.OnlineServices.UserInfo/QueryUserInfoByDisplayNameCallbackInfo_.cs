using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryUserInfoByDisplayNameCallbackInfo_ : IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_TargetUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_DisplayName;

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

	public string DisplayName => m_DisplayName;

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
