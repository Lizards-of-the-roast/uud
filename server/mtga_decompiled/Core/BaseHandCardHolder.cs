using System;
using System.Collections;
using System.Collections.Generic;
using MovementSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public abstract class BaseHandCardHolder : ZoneCardHolderBase
{
	private float _impactTimer;

	private const float ImpactTotalTime = 0.2f;

	private const float LateralDisplacementMin = 0.1f;

	private const float LateralDisplacementMax = 1f;

	private const float VerticalUniformDisplacement = 0f;

	private const float VerticalCenterWeightedDisplacement = 0.1f;

	private const float RotationMin = 1f;

	private const float RotationMax = -2f;

	private readonly SortedList<ZoneType, List<DuelScene_CDC>> _sortedCardSections = new SortedList<ZoneType, List<DuelScene_CDC>>(new LambdaComparer<ZoneType>(delegate(ZoneType lhs, ZoneType rhs)
	{
		return GetPriority(lhs).CompareTo(GetPriority(rhs));
		static int GetPriority(ZoneType zt)
		{
			return zt switch
			{
				ZoneType.Hand => -1, 
				ZoneType.Library => 1, 
				ZoneType.Graveyard => 2, 
				ZoneType.Exile => 3, 
				_ => int.MaxValue, 
			};
		}
	}));

	protected CardLayout_Hand _handLayout;

	protected virtual void Awake()
	{
		CardLayout_Hand handLayout = new CardLayout_Hand();
		base.Layout = (_handLayout = handLayout);
	}

	private void OnDrawGizmosSelected()
	{
		if (_handLayout != null)
		{
			float getLeftmostAngle = _handLayout.GetLeftmostAngle;
			float getRightmostAngle = _handLayout.GetRightmostAngle;
			Transform transform = base.transform;
			Vector3 up = transform.up;
			Gizmos.color = UnityEngine.Color.blue;
			Gizmos.DrawRay(transform.TransformPoint(_handLayout.GetLeftmostX, 0f, 0f), up * 5f);
			Gizmos.color = UnityEngine.Color.cyan;
			Gizmos.DrawRay(transform.TransformPoint(_handLayout.GetRightmostmostX, 0f, 0f), up * 5f);
			Func<float, Vector2> func = (float angleDeg) => new Vector3(Mathf.Cos(angleDeg * (MathF.PI / 180f)) * _handLayout.Radius, Mathf.Sin(angleDeg * (MathF.PI / 180f)) * _handLayout.Radius - _handLayout.GetArcBaseY);
			float num = _handLayout.FitAngle / 10f;
			Vector3 position = func(getLeftmostAngle);
			for (float num2 = getLeftmostAngle - num; num2 >= getRightmostAngle; num2 -= num)
			{
				Vector3 vector = func(num2);
				Gizmos.color = UnityEngine.Color.white;
				Gizmos.DrawLine(transform.TransformPoint(position), transform.TransformPoint(vector));
				position = vector;
			}
		}
	}

	protected virtual void Update()
	{
		SetFocusPosition();
	}

	protected virtual void SetFocusPosition()
	{
		if (!(CurrentCamera.Value == null) && _handLayout != null)
		{
			Vector3? focusPosition = null;
			if (CardDragController.DraggedCard == null)
			{
				Vector3 mousePosition = Input.mousePosition;
				mousePosition.z = Vector3.Distance(CurrentCamera.Value.transform.position, base.transform.position);
				Vector3 position = ((mousePosition.IsNaN() || mousePosition.IsInfinity() || mousePosition.IsFloatMinMax()) ? Vector3.zero : CurrentCamera.Value.ScreenToWorldPoint(mousePosition));
				focusPosition = base.transform.InverseTransformPoint(position);
			}
			if (_handLayout.SetFocusPosition(focusPosition))
			{
				_isDirty = true;
			}
		}
	}

	protected override void OnPreLayout()
	{
		base.OnPreLayout();
		SortCardViews();
	}

	protected void SortCardViews()
	{
		_handLayout.ArtificialSpacers.Clear();
		bool flag = false;
		for (int i = 0; i < _cardViews.Count - 1; i++)
		{
			if (_cardViews[i].Model.Zone.Type != _cardViews[i + 1].Model.Zone.Type)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < _cardViews.Count; j++)
		{
			ZoneType type = _cardViews[j].Model.Zone.Type;
			if (!_sortedCardSections.ContainsKey(type))
			{
				_sortedCardSections.Add(type, new List<DuelScene_CDC>());
			}
			_sortedCardSections[type].Add(_cardViews[j]);
		}
		_cardViews.Clear();
		for (int k = 0; k < _sortedCardSections.Count; k++)
		{
			ZoneType key = _sortedCardSections.Keys[k];
			_cardViews.AddRange(_sortedCardSections[key]);
			if (k + 1 < _sortedCardSections.Count)
			{
				_handLayout.ArtificialSpacers.Add(_cardViews.Count + k);
			}
		}
		_sortedCardSections.Clear();
	}

	public override IdealPoint GetLayoutEndpoint(CardLayoutData data)
	{
		Transform parent = data.Card.Root.parent;
		return new IdealPoint(parent.TransformPoint(data.Position), parent.rotation * data.Rotation, _overrideCdcSize ? (Vector3.one * _cdcSize) : data.Scale);
	}

	public void PlayImpactShake()
	{
		StartCoroutine(ImpactShake());
	}

	private IEnumerator ImpactShake()
	{
		_impactTimer = 0f;
		DuelScene_CDC[] localCardViews = _cardViews.ToArray();
		Vector3[] offsetPositions = new Vector3[localCardViews.Length];
		float[] offsetRotations = new float[localCardViews.Length];
		for (int i = 0; i < localCardViews.Length; i++)
		{
			Vector3 position = base.transform.position;
			Vector3 value = localCardViews[i].Root.position - position;
			Vector3 vector = Vector3.Normalize(value) * UnityEngine.Random.Range(0.1f, 1f);
			Vector3 vector2 = new Vector3(0f, 0.1f * (10f - value.magnitude) + 0f, 0f);
			offsetPositions[i] = localCardViews[i].PartsRoot.localPosition - (vector + vector2);
			offsetRotations[i] = (position.x - localCardViews[i].Root.position.x) * UnityEngine.Random.Range(1f, -2f);
		}
		while (_impactTimer < 1.2f)
		{
			for (int j = 0; j < localCardViews.Length; j++)
			{
				DuelScene_CDC duelScene_CDC = localCardViews[j];
				if (!(duelScene_CDC == null) && !(duelScene_CDC.PartsRoot == null))
				{
					Transform partsRoot = duelScene_CDC.PartsRoot;
					partsRoot.localPosition = Vector3.Lerp(offsetPositions[j], Vector3.zero, _impactTimer / 0.2f);
					Vector3 zero = Vector3.zero;
					zero.x = 0f;
					zero.y = (duelScene_CDC.VisualModel.IsDisplayedFaceDown ? 180 : 0);
					zero.z = Mathf.LerpAngle(offsetRotations[j], 0f, _impactTimer / 0.2f);
					partsRoot.localEulerAngles = zero;
				}
			}
			_impactTimer += Time.deltaTime;
			yield return null;
		}
	}

	public virtual void HandleClick(PointerEventData eventData, CardInput cardInput)
	{
		if (cardInput != null && CardHandlesInput())
		{
			cardInput.HandleClick(eventData);
		}
	}

	public virtual bool CardHandlesInput()
	{
		return true;
	}
}
