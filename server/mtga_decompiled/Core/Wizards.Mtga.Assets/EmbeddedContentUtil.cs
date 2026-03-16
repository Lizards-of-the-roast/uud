using System.IO;
using System.Threading;
using UnityEngine.Networking;

namespace Wizards.Mtga.Assets;

public static class EmbeddedContentUtil
{
	public static Stream? LoadEmbeddedContent(string contentPath)
	{
		UnityWebRequest unityWebRequest = UnityWebRequest.Get(contentPath);
		unityWebRequest.timeout = 5;
		UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = unityWebRequest.SendWebRequest();
		for (int i = 0; i < 1000; i++)
		{
			if (unityWebRequestAsyncOperation.isDone)
			{
				break;
			}
			Thread.Sleep(5);
		}
		if (unityWebRequest.responseCode == 404)
		{
			SimpleLog.LogError("Could not find embeded content at path: " + contentPath);
			return null;
		}
		return new MemoryStream(unityWebRequest.downloadHandler.data);
	}
}
