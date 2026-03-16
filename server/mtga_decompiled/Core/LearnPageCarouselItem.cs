using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableObject/Carousel/LearnPage", fileName = "LearnPageCarouselItem")]
public class LearnPageCarouselItem : CarouselItemBase
{
	public override void OnClick()
	{
		if (SceneLoader.GetSceneLoader().CurrentContentType == NavContentType.Home)
		{
			SceneLoader.GetSceneLoader().GoToLearnToPlay("Carousel");
		}
	}
}
