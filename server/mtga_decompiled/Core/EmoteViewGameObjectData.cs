public readonly struct EmoteViewGameObjectData
{
	public readonly EmoteData EmoteData;

	public readonly EmoteSelectionController.EmoteUISection EmoteUISection;

	public readonly bool IsClickable;

	public readonly bool IsEquipped;

	public readonly bool IsInstantiated;

	public EmoteViewGameObjectData(EmoteData emoteData, EmoteSelectionController.EmoteUISection emoteUISection, bool isClickable, bool isEquipped, bool isInstantiated)
	{
		EmoteData = emoteData;
		EmoteUISection = emoteUISection;
		IsClickable = isClickable;
		IsEquipped = isEquipped;
		IsInstantiated = isInstantiated;
	}
}
