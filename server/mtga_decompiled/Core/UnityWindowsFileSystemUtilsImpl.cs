using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;

public class UnityWindowsFileSystemUtilsImpl : IFileSystemUtilsImpl
{
	private class External
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct WIN32_FIND_DATA
		{
			public FileAttributes dwFileAttributes;

			public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;

			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;

			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;

			public uint nFileSizeHigh;

			public uint nFileSizeLow;

			public uint dwReserved0;

			public uint dwReserved1;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string cFileName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			public string cAlternate;
		}

		internal const int MAX_ALTERNATE = 14;

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CreateDirectory(string directoryPath, IntPtr securityAttributes);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CopyFile(string existingFileName, string newFileName, bool failIfExists);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetFileAttributes(string filePath, uint attributes);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern uint GetFileAttributes(string filePath);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool RemoveDirectory(string directoryPath);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteFile(string filePath);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool FindClose(IntPtr hFindFile);
	}

	private class PathSafeFileStream : FileStream
	{
		public PathSafeFileStream(SafeFileHandle handle, FileAccess access)
			: base(handle, access)
		{
		}

		protected override void Dispose(bool disposing)
		{
			SafeFileHandle?.Dispose();
			base.Dispose(disposing);
		}

		public override void Close()
		{
			SafeFileHandle?.Close();
			base.Close();
		}
	}

	private const int MAX_PATH = 260;

	public void CreateDirectory(DirectoryInfo directoryInfo)
	{
		List<string> list = new List<string>();
		while (directoryInfo != null && directoryInfo.FullName != directoryInfo.Root.FullName)
		{
			list.Insert(0, directoryInfo.FullName);
			directoryInfo = directoryInfo.Parent;
		}
		foreach (string item in list)
		{
			if (!External.CreateDirectory(GetLongFilePathFormat(item), IntPtr.Zero))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 183)
				{
					throw new Exception($"Failed to create directory {item} with error code {lastWin32Error:X8}");
				}
			}
		}
	}

	public void CopyFile(string sourceFilePath, string targetFilePath, bool failIfExists = false)
	{
		string longFilePathFormat = GetLongFilePathFormat(sourceFilePath);
		string longFilePathFormat2 = GetLongFilePathFormat(targetFilePath);
		CreateDirectory(new DirectoryInfo(new FileInfo(targetFilePath).Directory.FullName));
		if (!External.CopyFile(longFilePathFormat, longFilePathFormat2, failIfExists))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			throw new Exception($"File copy failed with error code {lastWin32Error:X8}");
		}
	}

	public void NormalizeFileAttributes(string filePath)
	{
		string longFilePathFormat = GetLongFilePathFormat(filePath);
		uint attributes = 128u;
		if (!External.SetFileAttributes(longFilePathFormat, attributes))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			throw new Exception($"NoramlizeFileAttributes failed on path {longFilePathFormat} with error {lastWin32Error:X8}");
		}
	}

	public void DeleteFile(string filePath)
	{
		if (filePath.Length < 260)
		{
			File.Delete(filePath);
			return;
		}
		string longFilePathFormat = GetLongFilePathFormat(filePath);
		if (External.DeleteFile(longFilePathFormat))
		{
			return;
		}
		int lastWin32Error = Marshal.GetLastWin32Error();
		throw new Exception($"FileDelete failed on path {longFilePathFormat} with error {lastWin32Error:X8}");
	}

	public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
	{
		searchPattern = searchPattern ?? "*";
		List<string> dirs = new List<string>();
		InternalGetDirectories(path, searchPattern, searchOption, ref dirs);
		return dirs.ToArray();
	}

	public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
	{
		searchPattern = searchPattern ?? "*";
		List<string> list = new List<string>();
		List<string> list2 = new List<string> { path };
		if (searchOption == SearchOption.AllDirectories)
		{
			list2.AddRange(GetDirectories(path, null, SearchOption.AllDirectories));
		}
		foreach (string item in list2)
		{
			External.WIN32_FIND_DATA lpFindFileData;
			IntPtr intPtr = External.FindFirstFile(Path.Combine(GetLongFilePathFormat(item), searchPattern), out lpFindFileData);
			try
			{
				if (!(intPtr != new IntPtr(-1)))
				{
					continue;
				}
				do
				{
					if ((lpFindFileData.dwFileAttributes & FileAttributes.Directory) == 0)
					{
						string path2 = Path.Combine(item, lpFindFileData.cFileName);
						list.Add(GetCleanPath(path2));
					}
				}
				while (External.FindNextFile(intPtr, out lpFindFileData));
				External.FindClose(intPtr);
			}
			catch (Exception)
			{
				External.FindClose(intPtr);
			}
		}
		return list.ToArray();
	}

	public void Move(string sourceFilePath, string targetFilePath)
	{
		if (sourceFilePath.Length < 260)
		{
			File.Move(sourceFilePath, targetFilePath);
			return;
		}
		CopyFile(sourceFilePath, targetFilePath);
		DeleteFile(sourceFilePath);
	}

	public bool FileExists(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		if (path.Length < 260)
		{
			return File.Exists(path);
		}
		uint fileAttributes = External.GetFileAttributes(GetLongFilePathFormat(path));
		if (fileAttributes != uint.MaxValue)
		{
			return (fileAttributes & 0x20) == 32;
		}
		return false;
	}

	public FileStream OpenRead(string path)
	{
		if (path.Length < 260)
		{
			return File.OpenRead(path);
		}
		return new PathSafeFileStream(OpenFile(path), FileAccess.Read);
	}

	public FileStream Create(string path)
	{
		if (path.Length < 260)
		{
			return File.Create(path);
		}
		return new PathSafeFileStream(CreateFile(path), FileAccess.ReadWrite);
	}

	public StreamReader OpenText(string path)
	{
		if (path.Length < 260)
		{
			return File.OpenText(path);
		}
		return new StreamReader(new PathSafeFileStream(OpenFile(path), FileAccess.Read));
	}

	public StreamWriter CreateText(string path)
	{
		if (path.Length < 260)
		{
			return File.CreateText(path);
		}
		return new StreamWriter(new PathSafeFileStream(CreateFile(path), FileAccess.ReadWrite));
	}

	public long FileLength(string path)
	{
		if (path.Length < 260)
		{
			return new FileInfo(path).Length;
		}
		using SafeFileHandle handle = OpenFile(path);
		using FileStream fileStream = new FileStream(handle, FileAccess.Read);
		return fileStream.Length;
	}

	public void DeleteDirectory(string directoryInfo)
	{
		string longDirectoryPathFormat = GetLongDirectoryPathFormat(directoryInfo);
		if (!External.RemoveDirectory(longDirectoryPathFormat))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			throw new Exception($"RemoveDirectory failed on path {longDirectoryPathFormat} with error {lastWin32Error:X8}");
		}
	}

	public string GetHash(string filePath, Func<Stream, string> hasher)
	{
		using SafeFileHandle handle = OpenFile(filePath);
		using FileStream arg = new FileStream(handle, FileAccess.Read);
		return hasher(arg);
	}

	private static string GetLongFilePathFormat(string path)
	{
		string arg = "\\\\?\\";
		if (path.StartsWith("\\\\"))
		{
			arg = "\\\\?\\UNC\\";
			path = path.TrimStart(new char[1] { '\\' });
		}
		else
		{
			path = new FileInfo(path).FullName;
		}
		return string.Format("{0}{1}", arg, path.Replace("/", "\\"));
	}

	private static string GetLongDirectoryPathFormat(string path)
	{
		string arg = "\\\\?\\";
		if (path.StartsWith("\\\\"))
		{
			arg = "\\\\?\\UNC\\";
			path = path.TrimStart(new char[1] { '\\' });
		}
		else
		{
			path = new DirectoryInfo(path).FullName;
		}
		return string.Format("{0}{1}", arg, path.Replace("/", "\\"));
	}

	private static void InternalGetDirectories(string path, string searchPattern, SearchOption searchOption, ref List<string> dirs)
	{
		External.WIN32_FIND_DATA lpFindFileData;
		IntPtr intPtr = External.FindFirstFile(Path.Combine(GetLongFilePathFormat(path), searchPattern), out lpFindFileData);
		try
		{
			if (!(intPtr != new IntPtr(-1)))
			{
				return;
			}
			do
			{
				if ((lpFindFileData.dwFileAttributes & FileAttributes.Directory) != 0 && lpFindFileData.cFileName != "." && lpFindFileData.cFileName != "..")
				{
					string path2 = Path.Combine(path, lpFindFileData.cFileName);
					dirs.Add(GetCleanPath(path2));
					if (searchOption == SearchOption.AllDirectories)
					{
						InternalGetDirectories(path2, searchPattern, searchOption, ref dirs);
					}
				}
			}
			while (External.FindNextFile(intPtr, out lpFindFileData));
			External.FindClose(intPtr);
		}
		catch (Exception)
		{
			External.FindClose(intPtr);
			throw;
		}
	}

	private static string GetCleanPath(string path)
	{
		if (path.StartsWith("\\\\?\\UNC\\"))
		{
			return "\\\\" + path.Substring(8);
		}
		if (path.StartsWith("\\\\?\\"))
		{
			return path.Substring(4);
		}
		return path;
	}

	private static SafeFileHandle OpenFile(string filePath, bool allowWrite = false)
	{
		uint num = 2147483648u;
		if (allowWrite)
		{
			num |= 0x40000000;
		}
		uint dwShareMode = 0u;
		if (!allowWrite)
		{
			dwShareMode = 1u;
		}
		string longFilePathFormat = GetLongFilePathFormat(filePath);
		SafeFileHandle safeFileHandle = External.CreateFile(longFilePathFormat, num, dwShareMode, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
		if (safeFileHandle.IsInvalid)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			throw new Exception($"OpenFile failed on path {longFilePathFormat} with error {lastWin32Error:X8}");
		}
		return safeFileHandle;
	}

	private static SafeFileHandle CreateFile(string filePath)
	{
		string longFilePathFormat = GetLongFilePathFormat(filePath);
		SafeFileHandle safeFileHandle = External.CreateFile(longFilePathFormat, 1073741824u, 0u, IntPtr.Zero, 2u, 0u, IntPtr.Zero);
		if (safeFileHandle.IsInvalid)
		{
			throw new Exception(string.Format("OpenFile failed on path {0} with error {1}", longFilePathFormat, Marshal.GetLastWin32Error().ToString("X8")));
		}
		return safeFileHandle;
	}
}
