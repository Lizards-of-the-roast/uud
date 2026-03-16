using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

public sealed class SessionModification : Handle
{
	public SessionModification(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result SetBucketId(SessionModificationSetBucketIdOptions options)
	{
		object obj = default(SessionModificationSetBucketIdOptions_);
		Helper.CopyProperties(options, obj);
		SessionModificationSetBucketIdOptions_ options2 = (SessionModificationSetBucketIdOptions_)obj;
		Result result = EOS_SessionModification_SetBucketId(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetHostAddress(SessionModificationSetHostAddressOptions options)
	{
		object obj = default(SessionModificationSetHostAddressOptions_);
		Helper.CopyProperties(options, obj);
		SessionModificationSetHostAddressOptions_ options2 = (SessionModificationSetHostAddressOptions_)obj;
		Result result = EOS_SessionModification_SetHostAddress(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetPermissionLevel(SessionModificationSetPermissionLevelOptions options)
	{
		object obj = default(SessionModificationSetPermissionLevelOptions_);
		Helper.CopyProperties(options, obj);
		SessionModificationSetPermissionLevelOptions_ options2 = (SessionModificationSetPermissionLevelOptions_)obj;
		Result result = EOS_SessionModification_SetPermissionLevel(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetJoinInProgressAllowed(SessionModificationSetJoinInProgressAllowedOptions options)
	{
		object obj = default(SessionModificationSetJoinInProgressAllowedOptions_);
		Helper.CopyProperties(options, obj);
		SessionModificationSetJoinInProgressAllowedOptions_ options2 = (SessionModificationSetJoinInProgressAllowedOptions_)obj;
		Result result = EOS_SessionModification_SetJoinInProgressAllowed(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result SetMaxPlayers(SessionModificationSetMaxPlayersOptions options)
	{
		object obj = default(SessionModificationSetMaxPlayersOptions_);
		Helper.CopyProperties(options, obj);
		SessionModificationSetMaxPlayersOptions_ options2 = (SessionModificationSetMaxPlayersOptions_)obj;
		Result result = EOS_SessionModification_SetMaxPlayers(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result AddAttribute(SessionModificationAddAttributeOptions options)
	{
		object obj = default(SessionModificationAddAttributeOptions_);
		Helper.CopyProperties(options, obj);
		SessionModificationAddAttributeOptions_ options2 = (SessionModificationAddAttributeOptions_)obj;
		Result result = EOS_SessionModification_AddAttribute(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public Result RemoveAttribute(SessionModificationRemoveAttributeOptions options)
	{
		object obj = default(SessionModificationRemoveAttributeOptions_);
		Helper.CopyProperties(options, obj);
		SessionModificationRemoveAttributeOptions_ options2 = (SessionModificationRemoveAttributeOptions_)obj;
		Result result = EOS_SessionModification_RemoveAttribute(base.InnerHandle, ref options2);
		options2.Dispose();
		return result;
	}

	public void Release()
	{
		EOS_SessionModification_Release(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_SessionModification_Release(IntPtr sessionModificationHandle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionModification_RemoveAttribute(IntPtr handle, ref SessionModificationRemoveAttributeOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionModification_AddAttribute(IntPtr handle, ref SessionModificationAddAttributeOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionModification_SetMaxPlayers(IntPtr handle, ref SessionModificationSetMaxPlayersOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionModification_SetJoinInProgressAllowed(IntPtr handle, ref SessionModificationSetJoinInProgressAllowedOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionModification_SetPermissionLevel(IntPtr handle, ref SessionModificationSetPermissionLevelOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionModification_SetHostAddress(IntPtr handle, ref SessionModificationSetHostAddressOptions_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_SessionModification_SetBucketId(IntPtr handle, ref SessionModificationSetBucketIdOptions_ options);
}
