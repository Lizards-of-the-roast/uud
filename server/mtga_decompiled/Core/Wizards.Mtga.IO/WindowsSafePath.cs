using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Wizards.Mtga.IO;

public static class WindowsSafePath
{
	private static class kernel32
	{
		[DllImport("kernel32", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		public static extern bool CreateDirectoryW(string directoryPath, IntPtr securityAttributes);

		[DllImport("kernel32", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		public static extern bool CopyFileW(string existingFileName, string newFileName, bool failIfExists);

		[DllImport("kernel32", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		public static extern bool GetFileAttributesExW(string lpFileName, GET_FILEEX_INFO_LEVELS fInfoLevelId, out WIN32_FILE_ATTRIBUTE_DATA fileData);

		[DllImport("kernel32", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		public static extern bool DeleteFileW(string lpFileName);
	}

	[Flags]
	private enum FileAttributes : uint
	{
		FILE_ATTRIBUTE_ARCHIVE = 0x20u,
		FILE_ATTRIBUTE_DIRECTORY = 0x10u,
		FILE_ATTRIBUTE_NORMAL = 0x80u,
		FILE_ATTRIBUTE_INVALID = uint.MaxValue
	}

	private enum SystemError
	{
		ERROR_FILE_NOT_FOUND = 2,
		ERROR_PATH_NOT_FOUND = 3,
		ERROR_ACCESS_DENIED = 5,
		ERROR_NO_MORE_FILES = 18,
		ERROR_FILE_EXISTS = 80,
		ERROR_CANNOT_MAKE = 82,
		ERROR_ALREADY_EXISTS = 183
	}

	public enum GET_FILEEX_INFO_LEVELS
	{
		GetFileExInfoStandard,
		GetFileExMaxInfoLevel
	}

	private struct WIN32_FILE_ATTRIBUTE_DATA
	{
		public FileAttributes dwFileAttributes;

		public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;

		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;

		public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;

		public uint nFileSizeHigh;

		public uint nFileSizeLow;
	}

	public static void CopyFile(string sourceFileName, string destFileName)
	{
		CopyFile(sourceFileName, destFileName, overwrite: false);
	}

	public static void CopyFile(string sourceFileName, string destFileName, bool overwrite)
	{
		if ((0u | (NeedsExtendedPath(sourceFileName, out sourceFileName) ? 1u : 0u) | (NeedsExtendedPath(destFileName, out destFileName) ? 1u : 0u)) != 0)
		{
			if (!kernel32.CopyFileW(sourceFileName, destFileName, !overwrite))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				IOException ex = new IOException("Error calling CopyFileW", lastWin32Error);
				switch ((SystemError)lastWin32Error)
				{
				case SystemError.ERROR_FILE_NOT_FOUND:
					throw new FileNotFoundException("File not found", sourceFileName, ex);
				case SystemError.ERROR_FILE_EXISTS:
					throw new IOException("Destination file exists", lastWin32Error);
				default:
					throw ex;
				}
			}
		}
		else
		{
			File.Copy(sourceFileName, destFileName, overwrite);
		}
	}

	public static DirectoryInfo CreateDirectory(string path)
	{
		if (NeedsExtendedPath(path, out path))
		{
			if (Directory.Exists(path))
			{
				return new DirectoryInfo(TrimLengthExtension(path));
			}
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName))
			{
				CreateDirectory(directoryName);
			}
			if (!kernel32.CreateDirectoryW(path, IntPtr.Zero))
			{
				throw new IOException("Could not create directory", Marshal.GetLastWin32Error());
			}
			return new DirectoryInfo(TrimLengthExtension(path));
		}
		return Directory.CreateDirectory(path);
	}

	public static void DeleteFile(string path)
	{
		if (NeedsExtendedPath(path, out path))
		{
			if (!kernel32.DeleteFileW(path))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				IOException ex = new IOException("Error calling DeleteFile", lastWin32Error);
				if (lastWin32Error == 2)
				{
					throw new FileNotFoundException("File not found", path, ex);
				}
				throw ex;
			}
		}
		else
		{
			File.Delete(path);
		}
	}

	public static bool DirectoryExists(string path)
	{
		NeedsExtendedPath(path, out path);
		return Directory.Exists(path);
	}

	public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
	{
		if (NeedsExtendedPath(path, out path))
		{
			return from p in Directory.EnumerateFiles(path, searchPattern, searchOption)
				select TrimLengthExtension(p);
		}
		return Directory.EnumerateDirectories(path, searchPattern, searchOption);
	}

	public static void DeleteDirectory(string path, bool recursive)
	{
		NeedsExtendedPath(path, out path);
		Directory.Delete(path, recursive);
	}

	public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
	{
		if (NeedsExtendedPath(path, out path))
		{
			return from p in Directory.EnumerateFiles(path, searchPattern, searchOption)
				select TrimLengthExtension(p);
		}
		return Directory.EnumerateFiles(path, searchPattern, searchOption);
	}

	public static bool FileExists(string path)
	{
		NeedsExtendedPath(path, out path);
		return File.Exists(path);
	}

	public static long GetFileLength(FileInfo fileInfo)
	{
		string extendedPath = fileInfo.FullName;
		if (NeedsExtendedPath(extendedPath, out extendedPath))
		{
			int num = 0;
			try
			{
				if (kernel32.GetFileAttributesExW(extendedPath, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out var fileData))
				{
					return (long)(((ulong)fileData.nFileSizeHigh << 32) | fileData.nFileSizeLow);
				}
			}
			catch (Exception ex)
			{
				if (!(ex is Win32Exception ex2))
				{
					throw ex;
				}
				num = ex2.NativeErrorCode;
			}
			if (num != 0)
			{
				num = Marshal.GetLastWin32Error();
			}
			IOException ex3 = new IOException("Error calling GetFileAttributesEx", num);
			if (num == 2)
			{
				throw new FileNotFoundException("File not found", extendedPath, ex3);
			}
			throw ex3;
		}
		return fileInfo.Length;
	}

	public static FileStream OpenFile(string path, FileMode mode)
	{
		return OpenFile(path, mode, FileAccess.ReadWrite, FileShare.None);
	}

	public static FileStream OpenFile(string path, FileMode mode, FileAccess access)
	{
		return OpenFile(path, mode, access, FileShare.None);
	}

	public static FileStream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
	{
		NeedsExtendedPath(path, out path);
		return File.Open(path, mode, access, share);
	}

	public static void SafeCreate(this DirectoryInfo directory)
	{
		if (NeedsExtendedPath(directory.FullName, out var extendedPath))
		{
			CreateDirectory(extendedPath);
		}
		else
		{
			directory.Create();
		}
	}

	public static IEnumerable<DirectoryInfo> SafeEnumerateDirectories(this DirectoryInfo directory)
	{
		return directory.SafeEnumerateDirectories("*", SearchOption.TopDirectoryOnly);
	}

	public static IEnumerable<DirectoryInfo> SafeEnumerateDirectories(this DirectoryInfo directory, string searchPattern)
	{
		return directory.SafeEnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
	}

	public static IEnumerable<DirectoryInfo> SafeEnumerateDirectories(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
	{
		return from p in EnumerateDirectories(directory.FullName, searchPattern, searchOption)
			select new DirectoryInfo(TrimLengthExtension(p));
	}

	public static IEnumerable<FileInfo> SafeEnumerateFiles(this DirectoryInfo directory)
	{
		return directory.SafeEnumerateFiles("*", SearchOption.TopDirectoryOnly);
	}

	public static IEnumerable<FileInfo> SafeEnumerateFiles(this DirectoryInfo directory, string searchPattern)
	{
		return directory.SafeEnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
	}

	public static IEnumerable<FileInfo> SafeEnumerateFiles(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
	{
		return from p in EnumerateFiles(directory.FullName, searchPattern, searchOption)
			select new FileInfo(TrimLengthExtension(p));
	}

	public static bool SafeExists(this DirectoryInfo directory)
	{
		return DirectoryExists(directory.FullName);
	}

	public static void SafeDelete(this FileInfo file)
	{
		DeleteFile(file.FullName);
	}

	public static bool SafeExists(this FileInfo file)
	{
		return FileExists(file.FullName);
	}

	public static long SafeGetLength(this FileInfo file)
	{
		return GetFileLength(file);
	}

	public static FileStream SafeOpen(this FileInfo file, FileMode mode)
	{
		return file.SafeOpen(mode, FileAccess.ReadWrite, FileShare.None);
	}

	public static FileStream SafeOpen(this FileInfo file, FileMode mode, FileAccess access)
	{
		return file.SafeOpen(mode, FileAccess.ReadWrite, FileShare.None);
	}

	public static FileStream SafeOpen(this FileInfo file, FileMode mode, FileAccess access, FileShare share)
	{
		return OpenFile(file.FullName, mode, access, share);
	}

	public static FileStream SafeOpenRead(this FileInfo file)
	{
		return file.SafeOpen(FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	public static StreamReader SafeOpenText(this FileInfo file)
	{
		return new StreamReader(file.SafeOpenRead(), Encoding.UTF8);
	}

	public static FileStream SafeOpenWrite(this FileInfo file)
	{
		return file.SafeOpen(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
	}

	public static bool NeedsExtendedPath(string path, out string extendedPath)
	{
		if (path.StartsWith("\\\\?\\"))
		{
			if (path.Length >= 252)
			{
				extendedPath = path;
				return true;
			}
			extendedPath = path.Substring(4);
			return false;
		}
		extendedPath = Path.GetFullPath(path).Replace('/', '\\');
		if (extendedPath.Length < 248)
		{
			extendedPath = path;
			return false;
		}
		if (extendedPath.StartsWith("\\\\"))
		{
			extendedPath = "\\\\?\\UNC\\" + extendedPath.Substring(2);
		}
		else
		{
			extendedPath = "\\\\?\\" + extendedPath;
		}
		return true;
	}

	public static string TrimLengthExtension(string path)
	{
		if (path.StartsWith("\\\\?\\"))
		{
			if (!path.StartsWith("\\\\?\\UNC\\"))
			{
				return path.Substring(4);
			}
			return path.Substring(8);
		}
		return path;
	}
}
