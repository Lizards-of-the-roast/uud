using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

public class ProfilingLogger : MonoBehaviour
{
	public string logFilePath;

	public bool profileInEditor;

	private string screencapDirPath = "";

	private int screenCaptureIntervalInSec;

	private int frameCount;

	private Recorder[] recorders;

	private float lastScreenCaptureTime;

	private int postIntervalInSec = 10;

	private float lastLogTime;

	private List<string> newLines = new List<string>();

	private string sessionID;

	private bool inProfilingMode;

	private readonly string[] recorderNames = new string[7] { "Camera.Render", "Render.TransparentGeometry", "Render.OpaqueGeometry", "Camera.ImageEffects", "ParticleSystem.Draw", "ParticleSystem.Update", "FixedUpdate.PhysicsFixedUpdate" };

	private void Start()
	{
		inProfilingMode = ParseCommandLineForProfilingMode();
		if (inProfilingMode)
		{
			sessionID = DateTime.Now.ToString("yyyyMMddHHmmssf");
			recorders = new Recorder[recorderNames.Length];
			for (int i = 0; i < recorderNames.Length; i++)
			{
				recorders[i] = Recorder.Get(recorderNames[i]);
				recorders[i].enabled = true;
			}
		}
	}

	private void Update()
	{
		if (!inProfilingMode)
		{
			return;
		}
		frameCount++;
		newLines.Add(csvFormat());
		if (Time.time - lastLogTime > (float)postIntervalInSec)
		{
			if (!string.IsNullOrWhiteSpace(logFilePath))
			{
				WriteLinesToFile(Path.Combine(logFilePath, sessionID + ".json"), newLines);
			}
			newLines.Clear();
			lastLogTime = Time.time;
		}
		if (screenCaptureIntervalInSec > 0 && Time.time - lastScreenCaptureTime > (float)screenCaptureIntervalInSec)
		{
			string filePath = $"{screencapDirPath}{(int)Time.time}.png";
			StartCoroutine(SaveScreenshot_ReadPixelsAsynch(filePath));
			lastScreenCaptureTime = Time.time;
		}
	}

	private string csvFormat()
	{
		StringBuilder stringBuilder = new StringBuilder("", 50);
		stringBuilder.Append(Time.deltaTime * 1000f + ",");
		for (int i = 0; i < recorders.Length; i++)
		{
			if (recorders[i].isValid)
			{
				stringBuilder.Append((double)recorders[i].elapsedNanoseconds * 1E-06 + ",");
			}
		}
		return stringBuilder.ToString();
	}

	private string jsonFormat()
	{
		StringBuilder stringBuilder = new StringBuilder("{", 200);
		stringBuilder.Append("\"machineName\":\"" + Environment.MachineName + "\",");
		stringBuilder.Append("\"graphicsDeviceName\":\"" + SystemInfo.graphicsDeviceName + "\",");
		stringBuilder.Append("\"graphicsDeviceType\":\"" + SystemInfo.graphicsDeviceType.ToString() + "\",");
		stringBuilder.Append("\"graphicsDeviceVendor\":\"" + SystemInfo.graphicsDeviceVendor + "\",");
		stringBuilder.Append("\"graphicsDeviceVersion\":\"" + SystemInfo.graphicsDeviceVersion + "\",");
		stringBuilder.Append("\"graphicsMemorySize\":\"" + SystemInfo.graphicsMemorySize + "\",");
		stringBuilder.Append("\"graphicsMultiThreaded\":\"" + SystemInfo.graphicsMultiThreaded + "\",");
		stringBuilder.Append("\"graphicsShaderLevel\":\"" + SystemInfo.graphicsShaderLevel + "\",");
		stringBuilder.Append("\"deviceUniqueIdentifier\":\"" + SystemInfo.deviceUniqueIdentifier + "\",");
		stringBuilder.Append("\"deviceModel\":\"" + SystemInfo.deviceModel + "\",");
		stringBuilder.Append("\"deviceType\":\"" + SystemInfo.deviceType.ToString() + "\",");
		stringBuilder.Append("\"operatingSystem\":\"" + SystemInfo.operatingSystem + "\",");
		stringBuilder.Append("\"operatingSystemFamily\":\"" + SystemInfo.operatingSystemFamily.ToString() + "\",");
		stringBuilder.Append("\"processorCount\":\"" + SystemInfo.processorCount + "\",");
		stringBuilder.Append("\"processorFrequency\":\"" + SystemInfo.processorFrequency + "\",");
		stringBuilder.Append("\"processorType\":\"" + SystemInfo.processorType + "\",");
		stringBuilder.Append("\"systemMemorySize\":\"" + SystemInfo.systemMemorySize + "\",");
		stringBuilder.Append("\"date\":\"" + DateTime.Today.ToString("d") + "\",");
		stringBuilder.Append("\"time\":\"" + DateTime.Now.TimeOfDay.ToString() + "\",");
		stringBuilder.Append("\"totalFrameTime\":\"" + Time.deltaTime * 1000f + "\",");
		stringBuilder.Append("\"sessionId\":\"" + sessionID + "\",");
		for (int i = 0; i < recorders.Length; i++)
		{
			if (recorders[i].isValid)
			{
				stringBuilder.Append("\"" + recorderNames[i].Replace(".", "_") + "\":\"" + (double)recorders[i].elapsedNanoseconds * 1E-06 + "\"");
				if (i < recorders.Length - 1)
				{
					stringBuilder.Append(",");
				}
			}
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	private void WriteLinesToFile(string logFilePath, List<string> lines)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
		using StreamWriter streamWriter = new StreamWriter(logFilePath, append: true);
		foreach (string line in lines)
		{
			streamWriter.WriteLine(line);
		}
	}

	private IEnumerator SaveScreenshot_ReadPixelsAsynch(string filePath)
	{
		yield return new WaitForEndOfFrame();
		Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
		texture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		yield return null;
		byte[] bytes = texture.EncodeToPNG();
		try
		{
			File.WriteAllBytes(filePath, bytes);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		UnityEngine.Object.Destroy(texture);
	}

	private IEnumerator SaveScreenshot_RenderToTexAsynch(string filePath)
	{
		yield return new WaitForEndOfFrame();
		RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
		Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera obj in allCameras)
		{
			obj.targetTexture = renderTexture;
			obj.Render();
			obj.targetTexture = null;
		}
		RenderTexture.active = renderTexture;
		screenShot.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		Camera.main.targetTexture = null;
		RenderTexture.active = null;
		UnityEngine.Object.Destroy(renderTexture);
		yield return null;
		byte[] bytes = screenShot.EncodeToPNG();
		File.WriteAllBytes(filePath, bytes);
	}

	private bool ParseCommandLineForProfilingMode()
	{
		bool result = Environment.GetCommandLineArgs().Contains("-profiling");
		if (Application.isPlaying && !Application.isEditor)
		{
			return result;
		}
		return profileInEditor;
	}
}
