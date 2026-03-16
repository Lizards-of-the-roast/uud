using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class SessionSearch : Handle
{
	private delegate void SessionSearchOnFindCallbackInternal(IntPtr messagePtr);

	public SessionSearch(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SetSessionId(SessionSearchSetSessionIdOptions options)
	{
		object obj = default(SessionSearchSetSessionIdOptions_);
		Helper.CopyProperties(options, obj);
		SessionSearchSetSessionIdOptions_ options2 = (SessionSearchSetSessionIdOptions_)obj;
		Result result = EOS_SessionSearch_SetSessionId(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetTargetUserId(SessionSearchSetTargetUserIdOptions options)
	{
		object obj = default(SessionSearchSetTargetUserIdOptions_);
		Helper.CopyProperties(options, obj);
		SessionSearchSetTargetUserIdOptions_ options2 = (SessionSearchSetTargetUserIdOptions_)obj;
		Result result = EOS_SessionSearch_SetTargetUserId(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetParameter(SessionSearchSetParameterOptions options)
	{
		object obj = default(SessionSearchSetParameterOptions_);
		Helper.CopyProperties(options, obj);
		SessionSearchSetParameterOptions_ options2 = (SessionSearchSetParameterOptions_)obj;
		Result result = EOS_SessionSearch_SetParameter(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result RemoveParameter(SessionSearchRemoveParameterOptions options)
	{
		object obj = default(SessionSearchRemoveParameterOptions_);
		Helper.CopyProperties(options, obj);
		SessionSearchRemoveParameterOptions_ options2 = (SessionSearchRemoveParameterOptions_)obj;
		Result result = EOS_SessionSearch_RemoveParameter(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetMaxResults(SessionSearchSetMaxResultsOptions options)
	{
		object obj = default(SessionSearchSetMaxResultsOptions_);
		Helper.CopyProperties(options, obj);
		SessionSearchSetMaxResultsOptions_ options2 = (SessionSearchSetMaxResultsOptions_)obj;
		Result result = EOS_SessionSearch_SetMaxResults(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public void Find(SessionSearchFindOptions options, object clientData, SessionSearchOnFindCallback completionDelegate)
	{
		object obj = default(SessionSearchFindOptions_);
		Helper.CopyProperties(options, obj);
		SessionSearchFindOptions_ options2 = (SessionSearchFindOptions_)obj;
		SessionSearchOnFindCallbackInternal sessionSearchOnFindCallbackInternal = SessionSearchOnFind;
		BoxedClientData boxedClientData = new BoxedClientData(clientData);
		IntPtr clientDataAddress = IntPtr.Zero;
		Helper.RegisterCall(ref clientDataAddress, boxedClientData, completionDelegate, sessionSearchOnFindCallbackInternal);
		EOS_SessionSearch_Find(base.InnerHandle, ref options2, clientDataAddress, sessionSearchOnFindCallbackInternal);
		options2.Dispose();
	}

	public uint GetSearchResultCount(SessionSearchGetSearchResultCountOptions options)
	{
		object obj = default(SessionSearchGetSearchResultCountOptions_);
		Helper.CopyProperties(options, obj);
		SessionSearchGetSearchResultCountOptions_ options2 = (SessionSearchGetSearchResultCountOptions_)obj;
		uint result = EOS_SessionSearch_GetSearchResultCount(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result CopySearchResultByIndex(SessionSearchCopySearchResultByIndexOptions options, out SessionDetails outSessionHandle)
	{
		object obj = default(SessionSearchCopySearchResultByIndexOptions_);
		Helper.CopyProperties(options, obj);
		SessionSearchCopySearchResultByIndexOptions_ options2 = (SessionSearchCopySearchResultByIndexOptions_)obj;
		outSessionHandle = Helper.GetDefault<SessionDetails>();
		IntPtr outSessionHandle2 = IntPtr.Zero;
		Result result = EOS_SessionSearch_CopySearchResultByIndex(base.InnerHandle, ref options2, ref outSessionHandle2);
		options2.Dispose();
		outSessionHandle = ((outSessionHandle2 == IntPtr.Zero) ? null : new SessionDetails(outSessionHandle2));
		return result;
	}

	public void Release()
	{
		EOS_SessionSearch_Release(base.InnerHandle);
	}

	[MonoPInvokeCallback]
	private static void SessionSearchOnFind(IntPtr messageAddress)
	{
		SessionSearchFindCallbackInfo_ sessionSearchFindCallbackInfo_ = Marshal.PtrToStructure<SessionSearchFindCallbackInfo_>(messageAddress);
		SessionSearchFindCallbackInfo sessionSearchFindCallbackInfo = new SessionSearchFindCallbackInfo();
		Helper.CopyProperties(sessionSearchFindCallbackInfo_, sessionSearchFindCallbackInfo);
		IntPtr clientDataAddress = sessionSearchFindCallbackInfo_.ClientDataAddress;
		sessionSearchFindCallbackInfo_.Dispose();
		(Helper.GetAndTryRemoveCallDelegate(clientDataAddress, sessionSearchFindCallbackInfo) as SessionSearchOnFindCallback)(sessionSearchFindCallbackInfo);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_SessionSearch_Release(IntPtr sessionSearchHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionSearch_CopySearchResultByIndex(IntPtr handle, ref SessionSearchCopySearchResultByIndexOptions_ options, ref IntPtr outSessionHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern uint EOS_SessionSearch_GetSearchResultCount(IntPtr handle, ref SessionSearchGetSearchResultCountOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_SessionSearch_Find(IntPtr handle, ref SessionSearchFindOptions_ options, IntPtr clientData, SessionSearchOnFindCallbackInternal completionDelegate);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionSearch_SetMaxResults(IntPtr handle, ref SessionSearchSetMaxResultsOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionSearch_RemoveParameter(IntPtr handle, ref SessionSearchRemoveParameterOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionSearch_SetParameter(IntPtr handle, ref SessionSearchSetParameterOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionSearch_SetTargetUserId(IntPtr handle, ref SessionSearchSetTargetUserIdOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionSearch_SetSessionId(IntPtr handle, ref SessionSearchSetSessionIdOptions_ options);
}
