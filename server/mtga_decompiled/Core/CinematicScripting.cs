using System.Collections;
using Core.Shared.Code;
using Core.VideoScene;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Wizards.Mtga;
using Wizards.Mtga.Assets;

[RequireComponent(typeof(PlayableDirector))]
public class CinematicScripting : MonoBehaviour
{
	[SerializeField]
	private PlayableDirector _timelineDirector;

	[SerializeField]
	private bool _closeCinematicAfterStopped = true;

	private WaitUntil _waitUntilTimelineComplete;

	public void Awake()
	{
		_waitUntilTimelineComplete = new WaitUntil(() => _timelineDirector != null && _timelineDirector.state != PlayState.Playing);
	}

	public void OnEnable()
	{
		Pantry.Get<GlobalCoroutineExecutor>().StartGlobalCoroutine(PlayCinematicOnEnable());
	}

	private IEnumerator PlayCinematicOnEnable()
	{
		_timelineDirector.Play();
		yield return _waitUntilTimelineComplete;
		if (_closeCinematicAfterStopped)
		{
			yield return UncoverHomePage(WrapperController.Instance, base.gameObject.scene);
		}
	}

	public static IEnumerator GoToCinematicCoroutine(WrapperController wrapper, string cinematicSceneName, string videoUrl = "", string videoPlayLookupMode = "", string videoPlayAudioMode = "")
	{
		SceneLoader sceneLoader = wrapper.SceneLoader;
		yield return sceneLoader.Coroutine_ShowTransitionBlocker();
		wrapper.TemporarilyHideAllUI(cinematicSceneName);
		AudioManager.StopAmbiance();
		AudioManager.StopMusic();
		if (!string.IsNullOrWhiteSpace(videoUrl))
		{
			CinematicVideoFilePlayer.VideoToPlay = videoUrl;
		}
		if (!string.IsNullOrWhiteSpace(videoPlayLookupMode))
		{
			CinematicVideoFilePlayer.VideoPlayLookupMode = videoPlayLookupMode;
		}
		if (!string.IsNullOrWhiteSpace(videoPlayAudioMode))
		{
			CinematicVideoFilePlayer.VideoPlayAudioMode = videoPlayAudioMode;
		}
		yield return Scenes.LoadSceneAsync(cinematicSceneName, LoadSceneMode.Additive);
		yield return sceneLoader.Coroutine_HideTransitionBlocker();
	}

	public static IEnumerator UncoverHomePage(WrapperController wrapper, Scene sceneToUnload)
	{
		if (wrapper != null)
		{
			yield return wrapper.SceneLoader.Coroutine_ShowTransitionBlocker();
			yield return SceneManager.UnloadSceneAsync(sceneToUnload);
			yield return Resources.UnloadUnusedAssets();
			wrapper.RestoreAllTemporarilyHiddenUI();
			AudioManager.PlayMusic(wrapper.CurrentContentType.ToString());
			AudioManager.PlayAmbiance(wrapper.CurrentContentType.ToString());
			yield return wrapper.SceneLoader.Coroutine_HideTransitionBlocker();
		}
	}
}
