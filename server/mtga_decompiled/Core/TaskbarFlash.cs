using System;
using System.Runtime.InteropServices;
using System.Text;

public static class TaskbarFlash
{
	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	private struct FLASHWINFO
	{
		public uint cbSize;

		public IntPtr hwnd;

		public uint dwFlags;

		public uint uCount;

		public uint dwTimeout;
	}

	private const uint FLASHW_ALL = 3u;

	private const uint FLASHW_TIMERNOFG = 12u;

	private static IntPtr _windowHandle = IntPtr.Zero;

	[DllImport("kernel32.dll")]
	private static extern uint GetCurrentThreadId();

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool FlashWindowEx(ref FLASHWINFO flashInfo);

	public static void Flash()
	{
		try
		{
			if (_windowHandle != IntPtr.Zero)
			{
				PerformFlashInternal();
			}
			else
			{
				FindWindowHandle(PerformFlashInternal);
			}
		}
		catch (Exception arg)
		{
			Console.WriteLine("[EX] Couldn't flash taskbar icon on Windows: {0}", arg);
		}
	}

	private static void FindWindowHandle(Action actionWhenFound)
	{
		StringBuilder sb = new StringBuilder("UnityWndClass".Length + 1);
		EnumThreadWindows(GetCurrentThreadId(), delegate(IntPtr handle, IntPtr param)
		{
			GetClassName(handle, sb, sb.Capacity);
			if (sb.ToString() == "UnityWndClass")
			{
				_windowHandle = handle;
				actionWhenFound?.Invoke();
				return false;
			}
			return true;
		}, IntPtr.Zero);
	}

	private static void PerformFlashInternal()
	{
		FLASHWINFO flashInfo = default(FLASHWINFO);
		flashInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(flashInfo));
		flashInfo.hwnd = _windowHandle;
		flashInfo.dwFlags = 15u;
		flashInfo.uCount = uint.MaxValue;
		flashInfo.dwTimeout = 0u;
		FlashWindowEx(ref flashInfo);
	}
}
