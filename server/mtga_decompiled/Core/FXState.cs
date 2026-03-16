using System;
using UnityEngine;

[Serializable]
public class FXState
{
	public string Name;

	public FXObject[] FXObjects;

	public bool IsLooping;

	private float _durationTimer;

	[NonSerialized]
	private FXSequencer _parent;

	public FXSequencer Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	public float Duration
	{
		get
		{
			float num = 0f;
			FXObject[] fXObjects = FXObjects;
			foreach (FXObject fXObject in fXObjects)
			{
				num = Mathf.Max(num, fXObject.Duration);
			}
			return num;
		}
	}

	public void Init(FXSequencer seq)
	{
		Parent = seq;
		FXObject[] fXObjects = FXObjects;
		for (int i = 0; i < fXObjects.Length; i++)
		{
			fXObjects[i].Init(this);
		}
	}

	public void Play()
	{
		_durationTimer = Duration;
		FXObject[] fXObjects = FXObjects;
		for (int i = 0; i < fXObjects.Length; i++)
		{
			fXObjects[i].Play();
		}
	}

	public void Stop()
	{
		FXObject[] fXObjects = FXObjects;
		for (int i = 0; i < fXObjects.Length; i++)
		{
			fXObjects[i].Stop();
		}
	}

	public void StopHard()
	{
		FXObject[] fXObjects = FXObjects;
		for (int i = 0; i < fXObjects.Length; i++)
		{
			fXObjects[i].StopHard();
		}
	}

	public void FXStateTick(float deltaTime)
	{
		_durationTimer -= deltaTime;
		FXObject[] fXObjects = FXObjects;
		for (int i = 0; i < fXObjects.Length; i++)
		{
			fXObjects[i].Tick(deltaTime);
		}
		if (_durationTimer <= 0f)
		{
			Parent.StateFinish(this);
		}
	}
}
