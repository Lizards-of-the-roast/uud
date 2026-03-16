using System;
using Core.MainNavigation.RewardTrack;
using UnityEngine;
using Wizards.Mtga;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableObject/Carousel/MasteryTab", fileName = "MasteryTabCarouselItem")]
public class MasteryTabCarouselItem : CarouselItemBase
{
	public override void OnClick()
	{
		if (SceneLoader.GetSceneLoader().CurrentContentType == NavContentType.Home)
		{
			ProgressionTrackPageContext trackPageContext = new ProgressionTrackPageContext(Pantry.Get<SetMasteryDataProvider>().CurrentBpName, NavContentType.Home, NavContentType.Home);
			SceneLoader.GetSceneLoader().GoToProgressionTrackScene(trackPageContext, "From Carousel");
		}
	}
}
