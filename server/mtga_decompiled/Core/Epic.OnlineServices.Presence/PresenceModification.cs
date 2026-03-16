using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Presence;

public sealed class PresenceModification : Handle
{
	public PresenceModification(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SetStatus(PresenceModificationSetStatusOptions options)
	{
		object obj = default(PresenceModificationSetStatusOptions_);
		Helper.CopyProperties(options, obj);
		PresenceModificationSetStatusOptions_ options2 = (PresenceModificationSetStatusOptions_)obj;
		Result result = EOS_PresenceModification_SetStatus(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetRawRichText(PresenceModificationSetRawRichTextOptions options)
	{
		object obj = default(PresenceModificationSetRawRichTextOptions_);
		Helper.CopyProperties(options, obj);
		PresenceModificationSetRawRichTextOptions_ options2 = (PresenceModificationSetRawRichTextOptions_)obj;
		Result result = EOS_PresenceModification_SetRawRichText(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetData(PresenceModificationSetDataOptions options)
	{
		object obj = default(PresenceModificationSetDataOptions_);
		Helper.CopyProperties(options, obj);
		PresenceModificationSetDataOptions_ options2 = (PresenceModificationSetDataOptions_)obj;
		Result result = EOS_PresenceModification_SetData(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result DeleteData(PresenceModificationDeleteDataOptions options)
	{
		object obj = default(PresenceModificationDeleteDataOptions_);
		Helper.CopyProperties(options, obj);
		PresenceModificationDeleteDataOptions_ options2 = (PresenceModificationDeleteDataOptions_)obj;
		Result result = EOS_PresenceModification_DeleteData(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public void Release()
	{
		EOS_PresenceModification_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_PresenceModification_Release(IntPtr presenceModificationHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_PresenceModification_DeleteData(IntPtr handle, ref PresenceModificationDeleteDataOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_PresenceModification_SetData(IntPtr handle, ref PresenceModificationSetDataOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_PresenceModification_SetRawRichText(IntPtr handle, ref PresenceModificationSetRawRichTextOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_PresenceModification_SetStatus(IntPtr handle, ref PresenceModificationSetStatusOptions_ options);
}
