using System;
using UnityEngine;

namespace MovementSystem;

public interface ISplineMovementSystem
{
	event Action<Transform, IdealPoint, SplineEventData> MovementStarted;

	event Action<Transform> MovementCompleted;

	void UpdateMovement();

	void MoveInstant(Transform transform, IdealPoint endPoint);

	void AddPermanentGoal(Transform transform, IdealPoint endPoint, bool allowInteractions = false, SplineMovementData spline = null, SplineEventData events = null);

	void AddPermanentGoal(Transform transform, IdealPoint endPoint, AllowInteractionType allowInteractions, SplineMovementData spline = null, SplineEventData events = null);

	void RemovePermanentGoal(Transform transform);

	void AddTemporaryGoal(Transform transform, IdealPoint endPoint, bool allowInteractions = false, SplineMovementData spline = null, SplineEventData events = null);

	void AddTemporaryGoal(Transform transform, IdealPoint endPoint, AllowInteractionType allowInteractions, SplineMovementData spline = null, SplineEventData events = null);

	void RemoveTemporaryGoal(Transform transform);

	void AddFollowTransform(Transform transformToFollow, Transform followingTransform, Vector3 positionOffset, Quaternion rotationOffset, Vector3 scaleOffset);

	void RemoveFollowTransform(Transform followingTransform);

	IdealPoint GetGoal(Transform transform);

	float GetProgress(Transform transform);

	bool InteractionsAreAllowed(Transform transform);

	void SetSpeedModifier(float newSpeed);

	float GetSpeedModifier();
}
