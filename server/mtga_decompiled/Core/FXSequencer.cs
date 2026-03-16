using UnityEngine;

public class FXSequencer : MonoBehaviour
{
	public bool StartOnEnable = true;

	public bool LoopOnComplete;

	public bool KillOnComplete = true;

	public float KillDelayAfterComplete = 5f;

	public FXState[] FXStates;

	private int _currentStateInt;

	private FXState _currentState;

	public float Duration
	{
		get
		{
			float num = 0f;
			FXState[] fXStates = FXStates;
			foreach (FXState fXState in fXStates)
			{
				num += fXState.Duration;
			}
			return num;
		}
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Stop();
	}

	private void Init()
	{
		FXState[] fXStates = FXStates;
		for (int i = 0; i < fXStates.Length; i++)
		{
			fXStates[i].Init(this);
		}
		if (StartOnEnable)
		{
			PlayStateInt(0);
		}
	}

	public void ProceedOnNextLoop()
	{
		_currentStateInt++;
		if (_currentStateInt > FXStates.Length - 1)
		{
			_currentStateInt = 0;
			_currentState = FXStates[0];
			if (!LoopOnComplete)
			{
				Debug.Log("Reached the end (proceedOnNextLooop).");
				if (KillOnComplete)
				{
					StartKillSelfDelay();
					base.enabled = false;
				}
				else
				{
					base.enabled = false;
				}
			}
			else
			{
				Init();
			}
		}
		else
		{
			Init();
		}
	}

	public void ProceedImmediate()
	{
		_currentStateInt++;
		if (_currentStateInt > FXStates.Length - 1)
		{
			_currentStateInt = 0;
			_currentState = FXStates[0];
			if (!LoopOnComplete)
			{
				Debug.Log("Reached the end (proceedImmediate).");
				if (KillOnComplete)
				{
					StartKillSelfDelay();
					base.enabled = false;
				}
				else
				{
					base.enabled = false;
				}
			}
			else
			{
				Init();
			}
		}
		else
		{
			PlayStateInt(_currentStateInt);
		}
	}

	public void PlayStateInt(int instate)
	{
		if (instate > FXStates.Length - 1)
		{
			_currentStateInt = 0;
			_currentState = FXStates[0];
			Debug.LogWarning($"Warning: You tried to move to FX State '{instate}' that does not exist.");
			if (!LoopOnComplete)
			{
				Debug.Log("Reached the end (PlayStateInt).");
				if (KillOnComplete)
				{
					StartKillSelfDelay();
					base.enabled = false;
				}
				else
				{
					base.enabled = false;
				}
			}
			else
			{
				Init();
			}
		}
		else
		{
			StopHard();
			_currentState = FXStates[instate];
			_currentState.Init(this);
			_currentState.Play();
			_currentStateInt = instate;
		}
	}

	public void PlayStateString(string instate)
	{
		for (int i = 0; i < FXStates.Length; i++)
		{
			FXState fXState = FXStates[i];
			if (fXState.Name == instate)
			{
				StopHard();
				fXState.Init(this);
				fXState.Play();
				_currentState = fXState;
				_currentStateInt = i;
				return;
			}
		}
		Debug.LogWarning($"Warning: You tried to call a FX State '{instate}' that does not exist.");
	}

	public void Stop()
	{
		if (_currentState != null)
		{
			_currentState.Stop();
			_currentState = null;
		}
	}

	public void StopHard()
	{
		if (_currentState != null)
		{
			_currentState.StopHard();
			_currentState = null;
		}
	}

	public void Reinitialize()
	{
		StopHard();
		Init();
	}

	private void Update()
	{
		if (_currentState != null)
		{
			_currentState.FXStateTick(Time.deltaTime);
		}
	}

	private void StartKillSelfDelay()
	{
		Object.Destroy(base.gameObject, KillDelayAfterComplete);
	}

	public void StateFinish(FXState state)
	{
		if (state.IsLooping)
		{
			if (_currentStateInt > FXStates.Length - 1)
			{
				_currentStateInt = 0;
				_currentState = FXStates[0];
				if (!LoopOnComplete)
				{
					if (KillOnComplete)
					{
						StartKillSelfDelay();
						base.enabled = false;
					}
					else
					{
						base.enabled = false;
					}
				}
				else
				{
					Init();
				}
			}
			else
			{
				state.Init(this);
				_currentState.Init(this);
				_currentState = FXStates[_currentStateInt];
				_currentState.Play();
			}
			return;
		}
		state.Stop();
		_currentStateInt++;
		if (_currentStateInt > FXStates.Length - 1)
		{
			_currentStateInt = 0;
			if (!LoopOnComplete)
			{
				if (KillOnComplete)
				{
					StartKillSelfDelay();
					base.enabled = false;
				}
				else
				{
					base.enabled = false;
				}
			}
			else
			{
				Init();
			}
		}
		else
		{
			_currentState.Init(this);
			_currentState = FXStates[_currentStateInt];
			_currentState.Play();
		}
	}
}
