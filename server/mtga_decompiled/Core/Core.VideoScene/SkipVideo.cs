using System;
using System.Diagnostics;
using Core.Shared.Code;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;

namespace Core.VideoScene;

public class SkipVideo : MonoBehaviour
{
	[SerializeField]
	public float holdTime = 3f;

	[SerializeField]
	public GameObject progressBar;

	private float _holdTimer;

	private bool _isHolding;

	private Image _progressBarImage;

	private readonly Stopwatch _stopwatch = new Stopwatch();

	public void StopVideoScene()
	{
		SceneChange payload = new SceneChange
		{
			EventTime = DateTime.UtcNow,
			fromSceneName = base.gameObject.scene.name,
			toSceneName = "",
			initiator = "SkipVideoButton",
			context = "Video",
			duration = _stopwatch.Elapsed,
			transitionTimeSeconds = TimeSpan.Zero
		};
		Pantry.Get<IBILogger>().Send(ClientBusinessEventType.SceneChange, payload);
		Pantry.Get<GlobalCoroutineExecutor>().StartCoroutine(CinematicScripting.UncoverHomePage(WrapperController.Instance, base.gameObject.scene));
	}

	public void StartSkip()
	{
		EventSystem.current.pixelDragThreshold = 200;
		_isHolding = true;
		if (progressBar != null)
		{
			_progressBarImage = progressBar.GetComponent<Image>();
		}
	}

	public void StopSkip()
	{
		_isHolding = false;
		_holdTimer = 0f;
		if (_progressBarImage != null)
		{
			_progressBarImage.fillAmount = 0f;
		}
	}

	private void Start()
	{
		_stopwatch.Start();
	}

	private void OnDisable()
	{
		if (EventSystem.current != null)
		{
			EventSystem.current.pixelDragThreshold = 5;
		}
	}

	private void Update()
	{
		bool flag = false;
		if (_isHolding)
		{
			_holdTimer += Time.deltaTime;
			if (_progressBarImage != null)
			{
				_progressBarImage.fillAmount = _holdTimer / holdTime;
			}
			if (_holdTimer >= holdTime)
			{
				flag = true;
			}
		}
		if (flag)
		{
			StopSkip();
			StopVideoScene();
		}
	}
}
