using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Video;

namespace AssetLookupTree.Payloads;

public class VideoClipPayload : IPayload
{
	public AltAssetReference<VideoClip> VideoClip = new AltAssetReference<VideoClip>();

	[JsonIgnore]
	public string VideoClipPath => VideoClip.RelativePath;

	public IEnumerable<string> GetFilePaths()
	{
		if (VideoClip != null)
		{
			yield return VideoClip.RelativePath;
		}
	}
}
