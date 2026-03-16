using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class SessionDetails : Handle
{
	public SessionDetails(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result CopyInfo(SessionDetailsCopyInfoOptions options, out SessionDetailsInfo outSessionInfo)
	{
		object obj = default(SessionDetailsCopyInfoOptions_);
		Helper.CopyProperties(options, obj);
		SessionDetailsCopyInfoOptions_ options2 = (SessionDetailsCopyInfoOptions_)obj;
		outSessionInfo = Helper.GetDefault<SessionDetailsInfo>();
		IntPtr outSessionInfo2 = IntPtr.Zero;
		Result result = EOS_SessionDetails_CopyInfo(base.InnerHandle, ref options2, ref outSessionInfo2);
		options2.Dispose();
		if (outSessionInfo2 != IntPtr.Zero)
		{
			SessionDetailsInfo_ sessionDetailsInfo_ = Marshal.PtrToStructure<SessionDetailsInfo_>(outSessionInfo2);
			outSessionInfo = new SessionDetailsInfo();
			Helper.CopyProperties(sessionDetailsInfo_, outSessionInfo);
			EOS_SessionDetails_Info_Release(outSessionInfo2);
			sessionDetailsInfo_.Dispose();
		}
		return result;
	}

	public uint GetSessionAttributeCount(SessionDetailsGetSessionAttributeCountOptions options)
	{
		object obj = default(SessionDetailsGetSessionAttributeCountOptions_);
		Helper.CopyProperties(options, obj);
		SessionDetailsGetSessionAttributeCountOptions_ options2 = (SessionDetailsGetSessionAttributeCountOptions_)obj;
		uint result = EOS_SessionDetails_GetSessionAttributeCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopySessionAttributeByIndex(SessionDetailsCopySessionAttributeByIndexOptions options, out SessionDetailsAttribute outSessionAttribute)
	{
		object obj = default(SessionDetailsCopySessionAttributeByIndexOptions_);
		Helper.CopyProperties(options, obj);
		SessionDetailsCopySessionAttributeByIndexOptions_ options2 = (SessionDetailsCopySessionAttributeByIndexOptions_)obj;
		outSessionAttribute = Helper.GetDefault<SessionDetailsAttribute>();
		IntPtr outSessionAttribute2 = IntPtr.Zero;
		Result result = EOS_SessionDetails_CopySessionAttributeByIndex(base.InnerHandle, ref options2, ref outSessionAttribute2);
		options2.Dispose();
		if (outSessionAttribute2 != IntPtr.Zero)
		{
			SessionDetailsAttribute_ sessionDetailsAttribute_ = Marshal.PtrToStructure<SessionDetailsAttribute_>(outSessionAttribute2);
			outSessionAttribute = new SessionDetailsAttribute();
			Helper.CopyProperties(sessionDetailsAttribute_, outSessionAttribute);
			EOS_SessionDetails_Attribute_Release(outSessionAttribute2);
			sessionDetailsAttribute_.Dispose();
		}
		return result;
	}

	public void Release()
	{
		EOS_SessionDetails_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_SessionDetails_Info_Release(IntPtr sessionInfo);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_SessionDetails_Attribute_Release(IntPtr sessionAttribute);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_SessionDetails_Release(IntPtr sessionHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionDetails_CopySessionAttributeByIndex(IntPtr handle, ref SessionDetailsCopySessionAttributeByIndexOptions_ options, ref IntPtr outSessionAttribute);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern uint EOS_SessionDetails_GetSessionAttributeCount(IntPtr handle, ref SessionDetailsGetSessionAttributeCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionDetails_CopyInfo(IntPtr handle, ref SessionDetailsCopyInfoOptions_ options, ref IntPtr outSessionInfo);
}
