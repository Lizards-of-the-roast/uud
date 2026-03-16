using System;
using UnityEngine;

namespace Core.Shared.Code.DebugTools;

public class AndroidThermalStatusProvider : IThermalStatusProvider, IDisposable
{
	private int _lastThermalStatus;

	private readonly InvokeAtInterval _thermalStatusUpdater;

	private readonly AndroidJavaObject _nativeObject;

	public AndroidThermalStatusProvider()
	{
		AndroidJNI.AttachCurrentThread();
		_nativeObject = new AndroidJavaObject("com.wizards.mtga.android.ThermalStatusProvider");
		_thermalStatusUpdater = new InvokeAtInterval(120, delegate
		{
			AndroidJNI.AttachCurrentThread();
			_lastThermalStatus = _nativeObject.Call<int>("getThermalStatus", Array.Empty<object>());
		});
	}

	public int GetThermalStatus()
	{
		_thermalStatusUpdater.Increment();
		return _lastThermalStatus;
	}

	public void Dispose()
	{
		AndroidJNI.AttachCurrentThread();
		_nativeObject.Dispose();
	}
}
