using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class CinematicControls : MonoBehaviour
{
	[SerializeField]
	private PlayableDirector _timelineDirector;

	[SerializeField]
	private bool _returnToLandingAfterCompleteTimeline = true;

	private void Update()
	{
		if (_returnToLandingAfterCompleteTimeline && PAPA.SceneLoading.CurrentScene != PAPA.MdnScene.None && _timelineDirector != null && _timelineDirector.state != PlayState.Playing)
		{
			SimpleLog.LogError("TEST: Returning to wrapper scene");
			PAPA.SceneLoading.LoadWrapperScene(new HomePageContext());
		}
	}
}
