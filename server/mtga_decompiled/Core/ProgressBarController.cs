using UnityEngine;

public class ProgressBarController : MonoBehaviour
{
	[SerializeField]
	private float _fillSpeed = 1f;

	[SerializeField]
	private QuestProgressBar QuestStyleProgressBar;

	private float _fillProgress;

	private float _fillGoal;

	private float _fillMax;

	private bool _isComplete;

	private bool _paused;

	public bool IsComplete()
	{
		return _isComplete;
	}

	private void Start()
	{
	}

	public void SetStart(float fillStart)
	{
		_fillProgress = fillStart;
	}

	public void SetGoal(float fillGoal)
	{
		_fillGoal = fillGoal;
	}

	public void SetFillMax(float fillMax)
	{
		_fillMax = fillMax;
	}

	public void SetFillTime(float time)
	{
		if (Mathf.Abs(_fillGoal - _fillProgress) <= float.Epsilon)
		{
			_fillSpeed = 1f;
		}
		_fillSpeed = Mathf.Abs(_fillGoal - _fillProgress) / time;
		SetProgress(_fillProgress, _fillMax);
	}

	public void Activate(bool active)
	{
		if (active)
		{
			_isComplete = false;
		}
		base.gameObject.SetActive(active);
	}

	public void SetPaused(bool pause)
	{
		_paused = pause;
	}

	private void Update()
	{
		if (_paused || _isComplete)
		{
			return;
		}
		if (_fillProgress == _fillGoal)
		{
			_isComplete = true;
		}
		else if (_fillProgress < _fillGoal)
		{
			_fillProgress += _fillSpeed * Time.deltaTime;
			if (_fillProgress > _fillGoal)
			{
				_isComplete = true;
				_fillProgress = _fillGoal;
			}
		}
		else
		{
			_fillProgress -= _fillSpeed * Time.deltaTime;
			if (_fillProgress < _fillGoal)
			{
				_isComplete = true;
				_fillProgress = _fillGoal;
			}
		}
		if (_fillMax > 0f)
		{
			SetProgress(_fillProgress, _fillMax);
		}
	}

	private void SetProgress(float progress, float max)
	{
		float percent = progress / max;
		if (QuestStyleProgressBar != null)
		{
			QuestStyleProgressBar.Percent = percent;
		}
	}
}
