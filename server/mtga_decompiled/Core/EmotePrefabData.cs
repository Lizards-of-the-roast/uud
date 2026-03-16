using System;
using Wizards.Arena.Enums.Cosmetic;

[Serializable]
public class EmotePrefabData
{
	public string Id { get; set; }

	public EmotePage Page { get; set; }

	public EmoteView Prefab { get; set; }

	public ClientEmoteEntry emoteEntry { get; set; }
}
