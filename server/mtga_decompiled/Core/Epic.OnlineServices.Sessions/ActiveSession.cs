using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class ActiveSession : Handle
{
	public ActiveSession(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result CopyInfo(ActiveSessionCopyInfoOptions options, out ActiveSessionInfo outActiveSessionInfo)
	{
		object obj = default(ActiveSessionCopyInfoOptions_);
		Helper.CopyProperties(options, obj);
		ActiveSessionCopyInfoOptions_ options2 = (ActiveSessionCopyInfoOptions_)obj;
		outActiveSessionInfo = Helper.GetDefault<ActiveSessionInfo>();
		IntPtr outActiveSessionInfo2 = IntPtr.Zero;
		Result result = EOS_ActiveSession_CopyInfo(base.InnerHandle, ref options2, ref outActiveSessionInfo2);
		options2.Dispose();
		if (outActiveSessionInfo2 != IntPtr.Zero)
		{
			ActiveSessionInfo_ activeSessionInfo_ = Marshal.PtrToStructure<ActiveSessionInfo_>(outActiveSessionInfo2);
			outActiveSessionInfo = new ActiveSessionInfo();
			Helper.CopyProperties(activeSessionInfo_, outActiveSessionInfo);
			EOS_ActiveSession_Info_Release(outActiveSessionInfo2);
			activeSessionInfo_.Dispose();
		}
		return result;
	}

	public void Release()
	{
		EOS_ActiveSession_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_ActiveSession_Info_Release(IntPtr activeSessionInfo);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_ActiveSession_Release(IntPtr activeSessionHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_ActiveSession_CopyInfo(IntPtr handle, ref ActiveSessionCopyInfoOptions_ options, ref IntPtr outActiveSessionInfo);
}
