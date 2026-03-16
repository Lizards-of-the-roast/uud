using UnityEngine;
using UnityEngine.Animations;

[DisallowMultipleComponent]
public class SkeletonControlRig : MonoBehaviour
{
	[Header("Rig Settings")]
	public string controlRigObjectName = "ControlRig";

	public string controlsContainerName = "Controls";

	public string zeroSuffix = "_ZERO";

	public string controlSuffix = "_CTRL";

	[SerializeField]
	[HideInInspector]
	public Transform _controlsRoot;

	public void BuildControlsInto(Transform controlsRoot)
	{
		if (!(controlsRoot == null))
		{
			_controlsRoot = controlsRoot;
			BuildControlsRecursive(base.transform, controlsRoot);
		}
	}

	public void ResetControls()
	{
		if (_controlsRoot == null)
		{
			return;
		}
		ControlNode[] componentsInChildren = _controlsRoot.GetComponentsInChildren<ControlNode>(includeInactive: true);
		foreach (ControlNode controlNode in componentsInChildren)
		{
			if ((bool)controlNode)
			{
				Transform obj = controlNode.transform;
				obj.localPosition = Vector3.zero;
				obj.localRotation = Quaternion.identity;
				obj.localScale = Vector3.one;
			}
		}
	}

	private void BuildControlsRecursive(Transform bone, Transform parentControl)
	{
		Transform transform = new GameObject(bone.name + zeroSuffix).transform;
		transform.SetParent(parentControl, worldPositionStays: false);
		AlignTransformWorld(transform, bone);
		GameObject obj = new GameObject(bone.name + controlSuffix);
		Transform transform2 = obj.transform;
		transform2.SetParent(transform, worldPositionStays: false);
		transform2.localPosition = Vector3.zero;
		transform2.localRotation = Quaternion.identity;
		transform2.localScale = Vector3.one;
		obj.AddComponent<ControlNode>().targetBone = bone;
		ParentConstraint parentConstraint = bone.GetComponent<ParentConstraint>();
		if (!parentConstraint)
		{
			parentConstraint = bone.gameObject.AddComponent<ParentConstraint>();
		}
		for (int num = parentConstraint.sourceCount - 1; num >= 0; num--)
		{
			parentConstraint.RemoveSource(num);
		}
		int index = parentConstraint.AddSource(new ConstraintSource
		{
			sourceTransform = transform2,
			weight = 1f
		});
		parentConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
		parentConstraint.rotationAxis = Axis.X | Axis.Y | Axis.Z;
		parentConstraint.translationAtRest = bone.localPosition;
		parentConstraint.rotationAtRest = bone.localEulerAngles;
		parentConstraint.locked = false;
		parentConstraint.constraintActive = false;
		parentConstraint.SetTranslationOffset(index, Vector3.zero);
		parentConstraint.SetRotationOffset(index, Vector3.zero);
		parentConstraint.constraintActive = true;
		parentConstraint.locked = true;
		for (int i = 0; i < bone.childCount; i++)
		{
			BuildControlsRecursive(bone.GetChild(i), transform2);
		}
	}

	private static void AlignTransformWorld(Transform dst, Transform src)
	{
		dst.position = src.position;
		dst.rotation = src.rotation;
		dst.localScale = src.lossyScale;
	}
}
