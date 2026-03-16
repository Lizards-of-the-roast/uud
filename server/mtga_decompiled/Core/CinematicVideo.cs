using System;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class CinematicVideo : PopupBase
{
	public Action OnComplete;

	[SerializeField]
	private Animator _skipCinematicInfo;

	[SerializeField]
	private float _hintDelayTime = 3f;

	[SerializeField]
	private float _hintLingerTime = 3f;

	private VideoPlayer _videoPlayer;

	private AudioSource _audioSource;

	private bool _hintVisible;

	private float _videoStartTime = -1f;

	private float _receivedInputTime = -1f;

	private Vector3 _mousePosition;

	public void Activate(VideoClip clip, Camera targetCamera = null)
	{
		if (_videoPlayer == null)
		{
			_videoPlayer = GetComponent<VideoPlayer>();
			_audioSource = GetComponent<AudioSource>();
		}
		if (targetCamera != null)
		{
			_videoPlayer.targetCamera = targetCamera;
		}
		if (clip != null)
		{
			_videoPlayer.clip = clip;
			_videoPlayer.SetTargetAudioSource(0, _audioSource);
		}
		base.Activate(activate: true);
	}

	protected override void Show()
	{
		if (_videoPlayer == null)
		{
			_videoPlayer = GetComponent<VideoPlayer>();
		}
		if (_videoPlayer.clip == null)
		{
			Hide();
			return;
		}
		base.Show();
		AudioManager.SetFocus(0f);
		if ((_videoPlayer.renderMode == VideoRenderMode.CameraFarPlane || _videoPlayer.renderMode == VideoRenderMode.CameraNearPlane) && _videoPlayer.targetCamera == null)
		{
			_videoPlayer.targetCamera = CurrentCamera.Value;
		}
		_videoPlayer.Play();
		_videoPlayer.loopPointReached -= VideoComplete;
		_videoPlayer.loopPointReached += VideoComplete;
		_videoStartTime = Time.time;
		_receivedInputTime = _videoStartTime - _hintDelayTime;
		_hintVisible = true;
	}

	protected override void Hide()
	{
		if (!(_videoPlayer == null))
		{
			_receivedInputTime = -1f;
			_videoPlayer.Stop();
			_hintVisible = false;
			Cursor.visible = true;
			OnComplete?.Invoke();
			OnComplete = null;
			AudioManager.SetFocus(100f);
			base.Hide();
		}
	}

	private void OnDisable()
	{
		if (_videoPlayer.isPlaying)
		{
			Hide();
		}
	}

	private void VideoComplete(VideoPlayer video)
	{
		video.loopPointReached -= VideoComplete;
		Hide();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Hide();
			return;
		}
		if (_mousePosition != Input.mousePosition || Input.anyKey)
		{
			_mousePosition = Input.mousePosition;
			if (Time.time - _hintDelayTime > _videoStartTime)
			{
				_receivedInputTime = Time.time;
			}
		}
		bool flag = Time.time - _receivedInputTime < _hintLingerTime;
		if (_hintVisible && !flag)
		{
			_hintVisible = false;
			Cursor.visible = false;
			if (_skipCinematicInfo != null)
			{
				_skipCinematicInfo.SetBool("Hint", value: false);
			}
		}
		else if (!_hintVisible && flag)
		{
			_hintVisible = true;
			Cursor.visible = true;
			if (_skipCinematicInfo != null)
			{
				_skipCinematicInfo.SetBool("Hint", value: true);
			}
		}
	}

	public override void OnEnter()
	{
	}

	public override void OnEscape()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
		Hide();
	}
}
