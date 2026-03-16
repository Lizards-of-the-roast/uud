using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LoginCallbackInfo_ : IDisposable
{
	private Result m_ResultCode;

	private IntPtr m_ClientData;

	private IntPtr m_LocalUserId;

	private IntPtr m_ContinuanceToken;

	public Result ResultCode => m_ResultCode;

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

	public ContinuanceToken ContinuanceToken
	{
		get
		{
			if (!(m_ContinuanceToken == IntPtr.Zero))
			{
				return new ContinuanceToken(m_ContinuanceToken);
			}
			return null;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_ClientData);
	}
}
