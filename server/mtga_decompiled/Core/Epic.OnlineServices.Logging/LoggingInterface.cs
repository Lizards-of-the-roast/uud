using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Logging;

public static class LoggingInterface
{
	private delegate void LogMessageFuncInternal(IntPtr messagePtr);

	private static LogMessageFuncInternal s_LogMessageFuncInternal;

	private static LogMessageFunc s_LogMessageFunc;

	public static Result SetCallback(LogMessageFunc callback)
	{
		LogMessageFuncInternal callback2 = LogMessageFunc;
		s_LogMessageFunc = callback;
		s_LogMessageFuncInternal = callback2;
		return EOS_Logging_SetCallback(callback2);
	}

	public static Result SetLogLevel(LogCategory logCategory, LogLevel logLevel)
	{
		return EOS_Logging_SetLogLevel(logCategory, logLevel);
	}

	[MonoPInvokeCallback]
	private static void LogMessageFunc(IntPtr messageAddress)
	{
		LogMessage_ logMessage_ = Marshal.PtrToStructure<LogMessage_>(messageAddress);
		LogMessage logMessage = new LogMessage();
		Helper.CopyProperties(logMessage_, logMessage);
		logMessage_.Dispose();
		s_LogMessageFunc(logMessage);
	}

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Logging_SetLogLevel(LogCategory logCategory, LogLevel logLevel);

	[DllImport("EOSSDK-Win64-Shipping.dll")]
	private static extern Result EOS_Logging_SetCallback(LogMessageFuncInternal callback);
}
