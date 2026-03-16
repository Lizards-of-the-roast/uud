using System;
using UnityEngine;

public class UnitySimpleLogImpl : ISimpleLogImpl
{
	public void Log(string msg)
	{
		Debug.Log(msg);
	}

	public void LogForRelease(string msg)
	{
		Debug.Log(msg);
	}

	public void LogFormat(string format, params object[] args)
	{
		Debug.Log(string.Format(format, args));
	}

	public void LogFormatForRelease(string format, params object[] args)
	{
		Debug.Log(string.Format(format, args));
	}

	public void LogWarning(string msg)
	{
		Debug.LogWarning(msg);
	}

	public void LogWarningForRelease(string msg)
	{
		Debug.LogWarning(msg);
	}

	public void LogError(string msg)
	{
		Debug.LogError(msg);
	}

	public void LogPreProdError(string msg)
	{
		LogWarning(msg);
	}

	public void LogErrorFormat(string format, params object[] args)
	{
		Debug.LogError(string.Format(format, args));
	}

	public void LogException(Exception e)
	{
		Debug.LogException(e);
	}
}
