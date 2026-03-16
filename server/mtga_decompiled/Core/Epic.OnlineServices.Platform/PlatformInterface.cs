using System;
using System.Runtime.InteropServices;
using System.Text;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Ecom;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Metrics;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.Sessions;
using Epic.OnlineServices.UserInfo;

namespace Epic.OnlineServices.Platform;

public sealed class PlatformInterface : Handle
{
	public PlatformInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public static Result Initialize(InitializeOptions options)
	{
		object obj = default(InitializeOptions_);
		Helper.CopyProperties(options, obj);
		InitializeOptions_ options2 = (InitializeOptions_)obj;
		int[] memory = new int[2] { 1, 1 };
		IntPtr address = IntPtr.Zero;
		Helper.RegisterAllocation(ref address, memory);
		options2.Reserved = address;
		Result result = EOS_Initialize(ref options2);
		options2.Dispose();
		Helper.ReleaseAllocation(ref address);
		return result;
	}

	public static Result Shutdown()
	{
		return Result.Success;
	}

	public static PlatformInterface Create(Options options)
	{
		object obj = default(Options_);
		Helper.CopyProperties(options, obj);
		Options_ options2 = (Options_)obj;
		IntPtr intPtr = EOS_Platform_Create(ref options2);
		options2.Dispose();
		if (!(intPtr == IntPtr.Zero))
		{
			return new PlatformInterface(intPtr);
		}
		return null;
	}

	public void Release()
	{
		EOS_Platform_Release(base.InnerHandle);
	}

	public void Tick()
	{
		EOS_Platform_Tick(base.InnerHandle);
	}

	public MetricsInterface GetMetricsInterface()
	{
		IntPtr intPtr = EOS_Platform_GetMetricsInterface(base.InnerHandle);
		if (!(intPtr == IntPtr.Zero))
		{
			return new MetricsInterface(intPtr);
		}
		return null;
	}

	public AuthInterface GetAuthInterface()
	{
		IntPtr intPtr = EOS_Platform_GetAuthInterface(base.InnerHandle);
		if (!(intPtr == IntPtr.Zero))
		{
			return new AuthInterface(intPtr);
		}
		return null;
	}

	public ConnectInterface GetConnectInterface()
	{
		IntPtr intPtr = EOS_Platform_GetConnectInterface(base.InnerHandle);
		if (!(intPtr == IntPtr.Zero))
		{
			return new ConnectInterface(intPtr);
		}
		return null;
	}

	public EcomInterface GetEcomInterface()
	{
		IntPtr intPtr = EOS_Platform_GetEcomInterface(base.InnerHandle);
		if (!(intPtr == IntPtr.Zero))
		{
			return new EcomInterface(intPtr);
		}
		return null;
	}

	public FriendsInterface GetFriendsInterface()
	{
		IntPtr intPtr = EOS_Platform_GetFriendsInterface(base.InnerHandle);
		if (!(intPtr == IntPtr.Zero))
		{
			return new FriendsInterface(intPtr);
		}
		return null;
	}

	public PresenceInterface GetPresenceInterface()
	{
		IntPtr intPtr = EOS_Platform_GetPresenceInterface(base.InnerHandle);
		if (!(intPtr == IntPtr.Zero))
		{
			return new PresenceInterface(intPtr);
		}
		return null;
	}

	public SessionsInterface GetSessionsInterface()
	{
		IntPtr intPtr = EOS_Platform_GetSessionsInterface(base.InnerHandle);
		if (!(intPtr == IntPtr.Zero))
		{
			return new SessionsInterface(intPtr);
		}
		return null;
	}

	public UserInfoInterface GetUserInfoInterface()
	{
		IntPtr intPtr = EOS_Platform_GetUserInfoInterface(base.InnerHandle);
		if (!(intPtr == IntPtr.Zero))
		{
			return new UserInfoInterface(intPtr);
		}
		return null;
	}

	public P2PInterface GetP2PInterface()
	{
		IntPtr intPtr = EOS_Platform_GetP2PInterface(base.InnerHandle);
		if (!(intPtr == IntPtr.Zero))
		{
			return new P2PInterface(intPtr);
		}
		return null;
	}

	public Result GetActiveCountryCode(EpicAccountId localUserId, StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Platform_GetActiveCountryCode(base.InnerHandle, localUserId.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public Result GetActiveLocaleCode(EpicAccountId localUserId, StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Platform_GetActiveLocaleCode(base.InnerHandle, localUserId.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public Result GetOverrideCountryCode(StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Platform_GetOverrideCountryCode(base.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public Result GetOverrideLocaleCode(StringBuilder outBuffer, ref int inOutBufferLength)
	{
		return EOS_Platform_GetOverrideLocaleCode(base.InnerHandle, outBuffer, ref inOutBufferLength);
	}

	public Result SetOverrideCountryCode(string newCountryCode)
	{
		return EOS_Platform_SetOverrideCountryCode(base.InnerHandle, newCountryCode);
	}

	public Result SetOverrideLocaleCode(string newLocaleCode)
	{
		return EOS_Platform_SetOverrideLocaleCode(base.InnerHandle, newLocaleCode);
	}

	public Result CheckForLauncherAndRestart()
	{
		return EOS_Platform_CheckForLauncherAndRestart(base.InnerHandle);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Platform_CheckForLauncherAndRestart(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Platform_SetOverrideLocaleCode(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string newLocaleCode);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Platform_SetOverrideCountryCode(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string newCountryCode);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Platform_GetOverrideLocaleCode(IntPtr handle, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Platform_GetOverrideCountryCode(IntPtr handle, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Platform_GetActiveLocaleCode(IntPtr handle, IntPtr localUserId, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Platform_GetActiveCountryCode(IntPtr handle, IntPtr localUserId, StringBuilder outBuffer, ref int inOutBufferLength);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_GetP2PInterface(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_GetUserInfoInterface(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_GetSessionsInterface(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_GetPresenceInterface(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_GetFriendsInterface(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_GetEcomInterface(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_GetConnectInterface(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_GetAuthInterface(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_GetMetricsInterface(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Platform_Tick(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern void EOS_Platform_Release(IntPtr handle);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern IntPtr EOS_Platform_Create(ref Options_ options);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Shutdown();

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Initialize(ref InitializeOptions_ options);
}
