using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace MovementSystem;

public class SplineMovementSystem : ISplineMovementSystem
{
	private class MovementData
	{
		public static float GLOBALSPEED = 4f;

		public static AnimationCurve DefaultEaseOut = new AnimationCurve(new Keyframe(0f, 0f, 3f, 3f), new Keyframe(1f, 1f, 0f, 0f));

		public readonly IdealPoint StartPoint;

		public readonly IdealPoint EndPoint;

		public readonly SplineMovementData SplineMovement;

		public readonly SplineEventData SplineEvents;

		private IdealPoint _currentPoint;

		public float Progress { get; private set; }

		public AllowInteractionType AllowInteractions { get; private set; }

		public SplineData.NodeType PositionNodeType
		{
			get
			{
				if (!(SplineMovement == null))
				{
					return SplineMovement.Spline.PositionStyle;
				}
				return SplineData.NodeType.WorldAbsolute;
			}
		}

		public SplineData.NodeType RotationNodeType
		{
			get
			{
				if (!(SplineMovement == null))
				{
					return SplineMovement.Spline.RotationStyle;
				}
				return SplineData.NodeType.WorldAbsolute;
			}
		}

		public IdealPoint CurrentPoint => _currentPoint;

		public MovementData(IdealPoint start, IdealPoint end, AllowInteractionType allowInteractions = AllowInteractionType.WhenFinished, SplineMovementData data = null, SplineEventData events = null, float startingProgress = 0f, float performedProgress = 0f)
		{
			SplineMovement = data;
			StartPoint = start;
			EndPoint = end;
			Progress = startingProgress;
			SplineEvents = events;
			AllowInteractions = allowInteractions;
			_currentPoint = StartPoint;
			if (SplineEvents != null && startingProgress > 0f && startingProgress > performedProgress)
			{
				SplineEvents.UpdateEvents(performedProgress, startingProgress, _currentPoint);
			}
			if ((object)SplineMovement != null)
			{
				SplineMovement.Spline.DrawDebugLine(start.Position, end.Position);
			}
			else
			{
				Debug.DrawLine(start.Position, end.Position, Color.grey, 1f);
			}
		}

		public bool UpdateProgress(float timeStep)
		{
			if (Progress >= 1f)
			{
				return false;
			}
			timeStep *= EndPoint.Speed;
			timeStep = (((object)SplineMovement == null) ? (timeStep * GLOBALSPEED) : (timeStep * SplineMovement.Speed));
			float progress = Progress;
			Progress = Mathf.Clamp01(Progress + timeStep);
			UpdatePosition();
			UpdateRotation();
			UpdateScale();
			if (SplineEvents != null)
			{
				SplineEvents.UpdateEvents(progress, Progress, CurrentPoint);
			}
			return Progress >= 1f;
		}

		private void UpdatePosition()
		{
			if (Progress >= 1f)
			{
				_currentPoint.Position = EndPoint.Position;
			}
			else if ((object)SplineMovement == null)
			{
				_currentPoint.Position = Vector3.LerpUnclamped(StartPoint.Position, EndPoint.Position, DefaultEaseOut.Evaluate(Progress));
			}
			else if (SplineMovement.Spline.PositionStyle == SplineData.NodeType.Ignored)
			{
				_currentPoint.Position = Vector3.LerpUnclamped(StartPoint.Position, EndPoint.Position, SplineMovement.Easing.Evaluate(Progress));
			}
			else
			{
				_currentPoint.Position = SplineMovement.Spline.GetPositionOnCurve(SplineMovement.Easing.Evaluate(Progress), StartPoint.Position, EndPoint.Position);
			}
		}

		private void UpdateRotation()
		{
			if (Progress >= 1f)
			{
				_currentPoint.Rotation = EndPoint.Rotation;
			}
			else if ((object)SplineMovement == null)
			{
				_currentPoint.Rotation = Quaternion.Lerp(StartPoint.Rotation, EndPoint.Rotation, DefaultEaseOut.Evaluate(Progress));
			}
			else if (SplineMovement.Spline.RotationStyle == SplineData.NodeType.Ignored)
			{
				_currentPoint.Rotation = Quaternion.LerpUnclamped(StartPoint.Rotation, EndPoint.Rotation, SplineMovement.Easing.Evaluate(Progress));
			}
			else
			{
				_currentPoint.Rotation = Quaternion.Euler(SplineMovement.Spline.GetRotationOnCurve(SplineMovement.Easing.Evaluate(Progress), StartPoint.Position, EndPoint.Position, StartPoint.Rotation.eulerAngles, EndPoint.Rotation.eulerAngles));
			}
		}

		private void UpdateScale()
		{
			if (Progress >= 1f)
			{
				_currentPoint.Scale = EndPoint.Scale;
			}
			else if ((object)SplineMovement == null)
			{
				_currentPoint.Scale = Vector3.Lerp(StartPoint.Scale, EndPoint.Scale, DefaultEaseOut.Evaluate(Progress));
			}
			else
			{
				_currentPoint.Scale = Vector3.LerpUnclamped(StartPoint.Scale, EndPoint.Scale, SplineMovement.Easing.Evaluate(Progress));
			}
		}

		public void SetComplete()
		{
			Progress = 1f;
		}
	}

	private class OffsetData
	{
		public Transform Transform { get; private set; }

		public Vector3 Position { get; private set; }

		public Quaternion Rotation { get; private set; }

		public Vector3 Scale { get; private set; }

		public OffsetData(Transform transform, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			Transform = transform;
			Position = position;
			Rotation = rotation;
			Scale = scale;
		}
	}

	private readonly Dictionary<Transform, MovementData> _permanentIdeals = new Dictionary<Transform, MovementData>();

	private readonly Dictionary<Transform, MovementData> _temporaryIdeals = new Dictionary<Transform, MovementData>();

	private readonly Dictionary<Transform, List<OffsetData>> _transformToOffsetData = new Dictionary<Transform, List<OffsetData>>();

	private readonly Dictionary<Transform, Transform> _followingTranformToFollowedTransform = new Dictionary<Transform, Transform>();

	private float _globalSpeedMultiplier = 1f;

	private readonly HashSet<Transform> _allKnownObjects = new HashSet<Transform>();

	private readonly HashSet<Transform> _pendingAdditions = new HashSet<Transform>();

	private readonly HashSet<Transform> _pendingRemovals = new HashSet<Transform>();

	public event Action<Transform, IdealPoint, SplineEventData> MovementStarted;

	public event Action<Transform> MovementCompleted;

	public static SplineMovementSystem Create()
	{
		return new SplineMovementSystem();
	}

	public void UpdateMovement()
	{
		if (_pendingRemovals.Count > 0)
		{
			foreach (Transform pendingRemoval in _pendingRemovals)
			{
				_allKnownObjects.Remove(pendingRemoval);
			}
			_pendingRemovals.Clear();
		}
		if (_pendingAdditions.Count > 0)
		{
			foreach (Transform pendingAddition in _pendingAdditions)
			{
				if (!_allKnownObjects.Contains(pendingAddition))
				{
					_allKnownObjects.Add(pendingAddition);
				}
			}
			_pendingAdditions.Clear();
		}
		foreach (Transform allKnownObject in _allKnownObjects)
		{
			if (!allKnownObject)
			{
				_pendingRemovals.Add(allKnownObject);
			}
			else
			{
				if (_followingTranformToFollowedTransform.ContainsKey(allKnownObject))
				{
					continue;
				}
				MovementData value = null;
				if (_temporaryIdeals.TryGetValue(allKnownObject, out value))
				{
					if (value.Progress < 1f)
					{
						MoveTowards(allKnownObject, value, temporary: true);
					}
				}
				else if (_permanentIdeals.TryGetValue(allKnownObject, out value))
				{
					if (value.Progress < 1f)
					{
						MoveTowards(allKnownObject, value, temporary: false);
					}
				}
				else
				{
					_pendingRemovals.Add(allKnownObject);
				}
			}
		}
	}

	public void MoveInstant(Transform transform, IdealPoint endPoint)
	{
		transform.position = endPoint.Position;
		transform.rotation = endPoint.Rotation;
		transform.localScale = endPoint.Scale;
		_temporaryIdeals.Remove(transform);
		_permanentIdeals[transform] = new MovementData(new IdealPoint(transform), endPoint, AllowInteractionType.All, null, null, 1f);
		_pendingAdditions.Add(transform);
		this.MovementStarted?.Invoke(transform, endPoint, null);
		this.MovementCompleted?.Invoke(transform);
	}

	public void AddPermanentGoal(Transform transform, IdealPoint endPoint, bool allowInteractions = false, SplineMovementData spline = null, SplineEventData events = null)
	{
		AddPermanentGoal(transform, endPoint, (!allowInteractions) ? AllowInteractionType.WhenFinished : AllowInteractionType.All, spline, events);
	}

	public void AddPermanentGoal(Transform transform, IdealPoint endPoint, AllowInteractionType allowInteractions, SplineMovementData spline = null, SplineEventData events = null)
	{
		float startingProgress = 0f;
		IdealPoint start = new IdealPoint(transform);
		if (_permanentIdeals.TryGetValue(transform, out var value) && value.Progress < 1f)
		{
			if (spline == null || spline == value.SplineMovement)
			{
				spline = value.SplineMovement;
				if (!value.StartPoint.Position.Appx(endPoint.Position))
				{
					start = value.StartPoint;
					startingProgress = value.Progress;
				}
			}
			if ((events == null || events.Events.Count == 0) && value.SplineEvents != null)
			{
				events = value.SplineEvents;
				events.Events.RemoveAll((SplineEvent x) => x.Time < startingProgress);
			}
		}
		this.MovementStarted?.Invoke(transform, endPoint, events);
		if (start.Position.Appx(endPoint.Position))
		{
			allowInteractions = AllowInteractionType.All;
			if (start.Scale.Appx(endPoint.Scale) && start.Rotation.AppxRotation(endPoint.Rotation))
			{
				startingProgress = 1f;
				this.MovementCompleted?.Invoke(transform);
			}
		}
		_pendingAdditions.Add(transform);
		_permanentIdeals[transform] = new MovementData(start, endPoint, allowInteractions, spline, events, startingProgress, startingProgress);
	}

	public void RemovePermanentGoal(Transform transform)
	{
		_pendingRemovals.Add(transform);
		_temporaryIdeals.Remove(transform);
		_permanentIdeals.Remove(transform);
	}

	public void AddTemporaryGoal(Transform transform, IdealPoint endPoint, bool allowInteractions = false, SplineMovementData spline = null, SplineEventData events = null)
	{
		AddTemporaryGoal(transform, endPoint, (!allowInteractions) ? AllowInteractionType.WhenFinished : AllowInteractionType.All, spline, events);
	}

	public void AddTemporaryGoal(Transform transform, IdealPoint endPoint, AllowInteractionType allowInteractions, SplineMovementData spline = null, SplineEventData events = null)
	{
		MovementData value;
		IdealPoint start = (_temporaryIdeals.TryGetValue(transform, out value) ? value.CurrentPoint : new IdealPoint(transform));
		this.MovementStarted?.Invoke(transform, endPoint, events);
		_pendingAdditions.Add(transform);
		_temporaryIdeals[transform] = new MovementData(start, endPoint, allowInteractions, spline, events);
	}

	public void RemoveTemporaryGoal(Transform transform)
	{
		if (_temporaryIdeals.Remove(transform))
		{
			if (_permanentIdeals.TryGetValue(transform, out var value))
			{
				SplineMovementData data = ((value.Progress < 1f) ? value.SplineMovement : null);
				SplineEventData events = ((value.Progress < 1f) ? value.SplineEvents : null);
				_permanentIdeals[transform] = new MovementData(new IdealPoint(transform), value.EndPoint, value.AllowInteractions, data, events);
			}
			else
			{
				_pendingRemovals.Add(transform);
			}
		}
	}

	public void AddFollowTransform(Transform transformToFollow, Transform followingTransform, Vector3 positionOffset, Quaternion rotationOffset, Vector3 scaleOffset)
	{
		if (!_followingTranformToFollowedTransform.Exists((KeyValuePair<Transform, Transform> x) => x.Key == followingTransform))
		{
			if (_permanentIdeals.TryGetValue(followingTransform, out var value))
			{
				value.SetComplete();
			}
			OffsetData item = new OffsetData(followingTransform, positionOffset, rotationOffset, scaleOffset);
			_followingTranformToFollowedTransform.Add(followingTransform, transformToFollow);
			List<OffsetData> value2 = null;
			if (!_transformToOffsetData.TryGetValue(transformToFollow, out value2))
			{
				value2 = new List<OffsetData>();
				_transformToOffsetData.Add(transformToFollow, value2);
			}
			value2.Add(item);
		}
	}

	public void RemoveFollowTransform(Transform followingTransform)
	{
		Transform value = null;
		if (!_followingTranformToFollowedTransform.TryGetValue(followingTransform, out value))
		{
			return;
		}
		List<OffsetData> value2 = null;
		if (_transformToOffsetData.TryGetValue(value, out value2))
		{
			value2.RemoveAll((OffsetData x) => x.Transform == followingTransform);
			if (value2.Count == 0)
			{
				_transformToOffsetData.Remove(value);
			}
		}
		_followingTranformToFollowedTransform.Remove(followingTransform);
	}

	public IdealPoint GetGoal(Transform transform)
	{
		if (_temporaryIdeals.TryGetValue(transform, out var value))
		{
			return value.EndPoint;
		}
		if (_permanentIdeals.TryGetValue(transform, out var value2))
		{
			return value2.EndPoint;
		}
		return new IdealPoint(transform);
	}

	public float GetProgress(Transform transform)
	{
		if (_temporaryIdeals.TryGetValue(transform, out var value))
		{
			return value.Progress;
		}
		if (_permanentIdeals.TryGetValue(transform, out var value2))
		{
			return value2.Progress;
		}
		return 1f;
	}

	public bool InteractionsAreAllowed(Transform transform)
	{
		if (_temporaryIdeals.TryGetValue(transform, out var value) || _permanentIdeals.TryGetValue(transform, out value))
		{
			switch (value.AllowInteractions)
			{
			case AllowInteractionType.All:
				return true;
			case AllowInteractionType.WhenFinished:
				return GetProgress(transform) >= 1f;
			case AllowInteractionType.Never:
				return false;
			}
		}
		return true;
	}

	private void MoveTowards(Transform transform, MovementData movementData, bool temporary)
	{
		bool flag = movementData.UpdateProgress(Time.deltaTime * _globalSpeedMultiplier);
		bool flag2 = true;
		if (movementData.SplineEvents != null && movementData.SplineEvents.Events.Count > 0)
		{
			MovementData value2;
			if (temporary)
			{
				if (!_temporaryIdeals.TryGetValue(transform, out var value) || value != movementData)
				{
					flag2 = false;
				}
			}
			else if (!_permanentIdeals.TryGetValue(transform, out value2) || value2 != movementData)
			{
				flag2 = false;
			}
		}
		if (flag2)
		{
			IdealPoint currentPoint = movementData.CurrentPoint;
			SplineData.NodeType positionNodeType = movementData.PositionNodeType;
			if (positionNodeType == SplineData.NodeType.Local || positionNodeType == SplineData.NodeType.LocalAbsolute)
			{
				transform.localPosition = currentPoint.Position;
			}
			else
			{
				transform.position = currentPoint.Position;
			}
			positionNodeType = movementData.RotationNodeType;
			if (positionNodeType == SplineData.NodeType.Local || positionNodeType == SplineData.NodeType.LocalAbsolute)
			{
				transform.localRotation = currentPoint.Rotation;
			}
			else
			{
				transform.rotation = currentPoint.Rotation;
			}
			transform.localScale = currentPoint.Scale;
			List<OffsetData> value3 = null;
			if (_transformToOffsetData.TryGetValue(transform, out value3))
			{
				foreach (OffsetData item in value3)
				{
					Vector3 position = transform.TransformPoint(item.Position);
					item.Transform.position = position;
					item.Transform.rotation = item.Rotation * transform.rotation;
					item.Transform.localScale = new Vector3(item.Scale.x * transform.localScale.x, item.Scale.y * transform.localScale.y, item.Scale.z * transform.localScale.z);
				}
			}
		}
		if (flag)
		{
			this.MovementCompleted?.Invoke(transform);
			if (movementData.SplineEvents != null)
			{
				movementData.SplineEvents.Events.Clear();
			}
		}
	}

	public void SetSpeedModifier(float newModifier = 1f)
	{
		_globalSpeedMultiplier = newModifier;
	}

	public float GetSpeedModifier()
	{
		return _globalSpeedMultiplier;
	}

	public static void AdjustGlobalAnimation(float speed, AnimationCurve curve)
	{
		MovementData.GLOBALSPEED = speed;
		MovementData.DefaultEaseOut = curve;
	}

	public static float GetDefaultSpeed()
	{
		return MovementData.GLOBALSPEED;
	}

	public static AnimationCurve GetDefaultEasingCurve()
	{
		return MovementData.DefaultEaseOut;
	}
}
