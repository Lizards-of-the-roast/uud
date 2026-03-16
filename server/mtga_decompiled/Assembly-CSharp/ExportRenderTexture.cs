using UnityEngine;

public class ExportRenderTexture : MonoBehaviour
{
	public RenderTexture renderTexture;

	public string outputFileName = "ExportedRenderTexture.png";

	public void Export()
	{
		Debug.LogWarning("Export can only be used in the editor.");
	}
}
