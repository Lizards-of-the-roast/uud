using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct QueryOwnershipTokenCallbackInfo_ : IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OwnershipToken;

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

	public string OwnershipToken => m_OwnershipToken;

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
