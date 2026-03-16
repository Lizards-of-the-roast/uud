using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class BattlefieldPhenomenon : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[Serializable]
	public class Condition
	{
		public enum Type
		{
			Click,
			Timed
		}

		[SerializeField]
		public Type _type;

		public int minFrequency = 1;

		public int maxFrequency = 1;
	}

	public string _description;

	[SerializeField]
	private Condition[] _conditions;

	public UnityEvent _thingsToDo;

	private Collider _collider;

	private int _clickCounter;

	private int _nextClickOccurance;

	private float _timeCounter;

	private int _nextTimedOccurance;

	public void OnEnable()
	{
		_collider = base.gameObject.GetComponent<Collider>();
		for (int i = 0; i < _conditions.Length; i++)
		{
			if (_conditions[i]._type == Condition.Type.Click)
			{
				_nextClickOccurance = UnityEngine.Random.Range(_conditions[i].minFrequency, _conditions[i].maxFrequency);
			}
		}
		for (int j = 0; j < _conditions.Length; j++)
		{
			if (_conditions[j]._type == Condition.Type.Timed)
			{
				_nextTimedOccurance = UnityEngine.Random.Range(_conditions[j].minFrequency, _conditions[j].maxFrequency);
			}
		}
	}

	public void Update()
	{
		_timeCounter += Time.deltaTime;
		for (int i = 0; i < _conditions.Length; i++)
		{
			if (_conditions[i]._type == Condition.Type.Timed && _timeCounter >= (float)_nextTimedOccurance)
			{
				_thingsToDo.Invoke();
				_nextTimedOccurance = UnityEngine.Random.Range(_conditions[i].minFrequency, _conditions[i].maxFrequency + 1);
				_timeCounter = 0f;
			}
		}
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		_clickCounter++;
		for (int i = 0; i < _conditions.Length; i++)
		{
			if (_conditions[i]._type == Condition.Type.Click && _clickCounter % _nextClickOccurance == 0)
			{
				_thingsToDo.Invoke();
				_nextClickOccurance = UnityEngine.Random.Range(_conditions[i].minFrequency, _conditions[i].maxFrequency + 1);
			}
		}
	}
}
