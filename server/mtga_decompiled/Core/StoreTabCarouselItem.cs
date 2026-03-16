using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableObject/Carousel/StoreTab", fileName = "StoreTabCarouselItem")]
public class StoreTabCarouselItem : CarouselItemBase
{
	[Space(20f)]
	public StoreTabType SelectedTab = StoreTabType.Featured;

	protected override bool OnIsVisibleToPlayer()
	{
		return true;
	}

	public override void OnClick()
	{
		SceneLoader.GetSceneLoader().GoToStore(SelectedTab, "Carousel Item");
	}
}
