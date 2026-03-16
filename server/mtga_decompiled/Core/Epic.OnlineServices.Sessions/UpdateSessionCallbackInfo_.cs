using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct UpdateSessionCallbackInfo_ : IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionName;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_SessionId;

	public Result ResultCode => m_ResultCode;

	public object ClientData => Helper.GetAllocation<BoxedClientData>(m_ClientData).ClientData;

	public IntPtr ClientDataAddress => m_ClientData;

	public string SessionName => m_SessionName;

	public string SessionId => m_SessionId;

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
