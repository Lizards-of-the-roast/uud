using UnityEngine;
using Wotc.Mtga.Cards.Database;

public class CardArtTextureLoader
{
	private const string MISSING_ART_PATH = "Assets/Core/CardArt/SetAgnostic/MissingArt.png";

	public string GetCardArtPath(string originalAssetPath, bool returnMissingArt = true)
	{
		if (string.IsNullOrWhiteSpace(originalAssetPath))
		{
			if (!returnMissingArt)
			{
				return string.Empty;
			}
			return "Assets/Core/CardArt/SetAgnostic/MissingArt.png";
		}
		if (originalAssetPath.Equals("Assets/Core/CardArt/000000/000000_AIF"))
		{
			if (!returnMissingArt)
			{
				return string.Empty;
			}
			return "Assets/Core/CardArt/SetAgnostic/MissingArt.png";
		}
		if (originalAssetPath.Contains("."))
		{
			originalAssetPath = originalAssetPath.Remove(originalAssetPath.LastIndexOf('.'));
		}
		foreach (string item in CardArtUtil.ArtFileTypesInSearchOrder)
		{
			string text = originalAssetPath + item;
			if (AssetLoader.HaveAsset(text))
			{
				return text;
			}
		}
		if (returnMissingArt)
		{
			SimpleLog.LogError("No card art found for " + originalAssetPath);
			return "Assets/Core/CardArt/SetAgnostic/MissingArt.png";
		}
		return string.Empty;
	}

	public Texture2D AcquireCardArt(AssetTracker cardArtTracker, string assetTrackerKey, string assetPath, bool returnMissingArt = true)
	{
		string cardArtPath = GetCardArtPath(assetPath, returnMissingArt);
		Texture2D texture2D = cardArtTracker.AcquireAndTrack<Texture2D>(assetTrackerKey, cardArtPath);
		if (texture2D == null)
		{
			Debug.LogError("CardArtTextureLoader tried to load " + assetPath + " and failed");
		}
		return texture2D;
	}
}
