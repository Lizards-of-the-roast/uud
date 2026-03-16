using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using Wizards.DynamicTimelineBinding;
using Wizards.GeneralUtilities;
using Wizards.GeneralUtilities.ObjectCommunication;
using Wizards.Mtga.Npe;

namespace Unity.VisualScripting.Generated.Aot;

[Preserve]
public class AotStubs
{
	[Preserve]
	public static void string_op_Equality()
	{
		string text = null;
		string text2 = null;
		_ = text == text2;
		StaticFunctionInvoker<string, string, bool> staticFunctionInvoker = new StaticFunctionInvoker<string, string, bool>(null);
		staticFunctionInvoker.Invoke(null, text, text2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void string_op_Inequality()
	{
		string text = null;
		string text2 = null;
		_ = text != text2;
		StaticFunctionInvoker<string, string, bool> staticFunctionInvoker = new StaticFunctionInvoker<string, string, bool>(null);
		staticFunctionInvoker.Invoke(null, text, text2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void float_op_Equality()
	{
		float num = 0f;
		float num2 = 0f;
		StaticFunctionInvoker<float, float, bool> staticFunctionInvoker = new StaticFunctionInvoker<float, float, bool>(null);
		staticFunctionInvoker.Invoke(null, num, num2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void float_op_Inequality()
	{
		float num = 0f;
		float num2 = 0f;
		StaticFunctionInvoker<float, float, bool> staticFunctionInvoker = new StaticFunctionInvoker<float, float, bool>(null);
		staticFunctionInvoker.Invoke(null, num, num2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void float_op_LessThan()
	{
		float num = 0f;
		float num2 = 0f;
		StaticFunctionInvoker<float, float, bool> staticFunctionInvoker = new StaticFunctionInvoker<float, float, bool>(null);
		staticFunctionInvoker.Invoke(null, num, num2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void float_op_GreaterThan()
	{
		float num = 0f;
		float num2 = 0f;
		StaticFunctionInvoker<float, float, bool> staticFunctionInvoker = new StaticFunctionInvoker<float, float, bool>(null);
		staticFunctionInvoker.Invoke(null, num, num2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void float_op_LessThanOrEqual()
	{
		float num = 0f;
		float num2 = 0f;
		StaticFunctionInvoker<float, float, bool> staticFunctionInvoker = new StaticFunctionInvoker<float, float, bool>(null);
		staticFunctionInvoker.Invoke(null, num, num2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void float_op_GreaterThanOrEqual()
	{
		float num = 0f;
		float num2 = 0f;
		StaticFunctionInvoker<float, float, bool> staticFunctionInvoker = new StaticFunctionInvoker<float, float, bool>(null);
		staticFunctionInvoker.Invoke(null, num, num2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animator_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animator_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animator_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_StateMachineBehaviour_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_StateMachineBehaviour_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_StateMachineBehaviour_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animation_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animation_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animation_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_AnimationClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_AnimationClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_AnimationClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_AnimatorOverrideController_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_AnimatorOverrideController_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_AnimatorOverrideController_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Avatar_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Avatar_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Avatar_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_AvatarMask_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_AvatarMask_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_AvatarMask_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Motion_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Motion_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Motion_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RuntimeAnimatorController_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RuntimeAnimatorController_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RuntimeAnimatorController_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_AimConstraint_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_AimConstraint_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_AimConstraint_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_PositionConstraint_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_PositionConstraint_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_PositionConstraint_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_RotationConstraint_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_RotationConstraint_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_RotationConstraint_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_ScaleConstraint_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_ScaleConstraint_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_ScaleConstraint_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_LookAtConstraint_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_LookAtConstraint_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_LookAtConstraint_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_ParentConstraint_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_ParentConstraint_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Animations_ParentConstraint_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Camera_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Camera_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Camera_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_FlareLayer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_FlareLayer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_FlareLayer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ReflectionProbe_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ReflectionProbe_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ReflectionProbe_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Bounds_op_Equality()
	{
		Bounds bounds = default(Bounds);
		Bounds bounds2 = default(Bounds);
		_ = bounds == bounds2;
		StaticFunctionInvoker<Bounds, Bounds, bool> staticFunctionInvoker = new StaticFunctionInvoker<Bounds, Bounds, bool>(null);
		staticFunctionInvoker.Invoke(null, bounds, bounds2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Bounds_op_Inequality()
	{
		Bounds bounds = default(Bounds);
		Bounds bounds2 = default(Bounds);
		_ = bounds != bounds2;
		StaticFunctionInvoker<Bounds, Bounds, bool> staticFunctionInvoker = new StaticFunctionInvoker<Bounds, Bounds, bool>(null);
		staticFunctionInvoker.Invoke(null, bounds, bounds2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rect_op_Inequality()
	{
		Rect rect = default(Rect);
		Rect rect2 = default(Rect);
		_ = rect != rect2;
		StaticFunctionInvoker<Rect, Rect, bool> staticFunctionInvoker = new StaticFunctionInvoker<Rect, Rect, bool>(null);
		staticFunctionInvoker.Invoke(null, rect, rect2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rect_op_Equality()
	{
		Rect rect = default(Rect);
		Rect rect2 = default(Rect);
		_ = rect == rect2;
		StaticFunctionInvoker<Rect, Rect, bool> staticFunctionInvoker = new StaticFunctionInvoker<Rect, Rect, bool>(null);
		staticFunctionInvoker.Invoke(null, rect, rect2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightingSettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightingSettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightingSettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_BillboardAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_BillboardAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_BillboardAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_BillboardRenderer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_BillboardRenderer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_BillboardRenderer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightmapSettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightmapSettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightmapSettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightProbes_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightProbes_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightProbes_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_QualitySettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_QualitySettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_QualitySettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Mesh_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Mesh_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Mesh_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Renderer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Renderer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Renderer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Projector_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Projector_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Projector_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Shader_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Shader_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Shader_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_TrailRenderer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_TrailRenderer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_TrailRenderer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LineRenderer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LineRenderer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LineRenderer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RenderSettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RenderSettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RenderSettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Material_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Material_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Material_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_OcclusionPortal_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_OcclusionPortal_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_OcclusionPortal_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_OcclusionArea_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_OcclusionArea_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_OcclusionArea_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Flare_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Flare_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Flare_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LensFlare_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LensFlare_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LensFlare_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Light_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Light_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Light_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Skybox_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Skybox_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Skybox_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_MeshFilter_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_MeshFilter_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_MeshFilter_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightProbeProxyVolume_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightProbeProxyVolume_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightProbeProxyVolume_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SkinnedMeshRenderer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SkinnedMeshRenderer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SkinnedMeshRenderer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_MeshRenderer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_MeshRenderer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_MeshRenderer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightProbeGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightProbeGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LightProbeGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LODGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LODGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LODGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture2D_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture2D_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture2D_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Cubemap_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Cubemap_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Cubemap_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture3D_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture3D_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture3D_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture2DArray_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture2DArray_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Texture2DArray_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CubemapArray_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CubemapArray_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CubemapArray_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SparseTexture_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SparseTexture_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SparseTexture_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RenderTexture_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RenderTexture_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RenderTexture_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CustomRenderTexture_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CustomRenderTexture_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CustomRenderTexture_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Addition()
	{
		Color color = default(Color);
		Color color2 = default(Color);
		_ = color + color2;
		StaticFunctionInvoker<Color, Color, Color> staticFunctionInvoker = new StaticFunctionInvoker<Color, Color, Color>(null);
		staticFunctionInvoker.Invoke(null, color, color2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Subtraction()
	{
		Color color = default(Color);
		Color color2 = default(Color);
		_ = color - color2;
		StaticFunctionInvoker<Color, Color, Color> staticFunctionInvoker = new StaticFunctionInvoker<Color, Color, Color>(null);
		staticFunctionInvoker.Invoke(null, color, color2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Multiply()
	{
		Color color = default(Color);
		Color color2 = default(Color);
		_ = color * color2;
		StaticFunctionInvoker<Color, Color, Color> staticFunctionInvoker = new StaticFunctionInvoker<Color, Color, Color>(null);
		staticFunctionInvoker.Invoke(null, color, color2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Multiply_0()
	{
		Color color = default(Color);
		float num = 0f;
		_ = color * num;
		StaticFunctionInvoker<Color, float, Color> staticFunctionInvoker = new StaticFunctionInvoker<Color, float, Color>(null);
		staticFunctionInvoker.Invoke(null, color, num);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Multiply_1()
	{
		float num = 0f;
		Color color = default(Color);
		_ = num * color;
		StaticFunctionInvoker<float, Color, Color> staticFunctionInvoker = new StaticFunctionInvoker<float, Color, Color>(null);
		staticFunctionInvoker.Invoke(null, num, color);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Division()
	{
		Color color = default(Color);
		float num = 0f;
		_ = color / num;
		StaticFunctionInvoker<Color, float, Color> staticFunctionInvoker = new StaticFunctionInvoker<Color, float, Color>(null);
		staticFunctionInvoker.Invoke(null, color, num);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Equality()
	{
		Color color = default(Color);
		Color color2 = default(Color);
		_ = color == color2;
		StaticFunctionInvoker<Color, Color, bool> staticFunctionInvoker = new StaticFunctionInvoker<Color, Color, bool>(null);
		staticFunctionInvoker.Invoke(null, color, color2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Inequality()
	{
		Color color = default(Color);
		Color color2 = default(Color);
		_ = color != color2;
		StaticFunctionInvoker<Color, Color, bool> staticFunctionInvoker = new StaticFunctionInvoker<Color, Color, bool>(null);
		staticFunctionInvoker.Invoke(null, color, color2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Implicit()
	{
		Color color = default(Color);
		_ = (Vector4)color;
		StaticFunctionInvoker<Color, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<Color, Vector4>(null);
		staticFunctionInvoker.Invoke(null, color);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Color_op_Implicit_0()
	{
		Vector4 vector = default(Vector4);
		_ = (Color)vector;
		StaticFunctionInvoker<Vector4, Color> staticFunctionInvoker = new StaticFunctionInvoker<Vector4, Color>(null);
		staticFunctionInvoker.Invoke(null, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Matrix4x4_op_Multiply()
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		Matrix4x4 matrix4x2 = default(Matrix4x4);
		_ = matrix4x * matrix4x2;
		StaticFunctionInvoker<Matrix4x4, Matrix4x4, Matrix4x4> staticFunctionInvoker = new StaticFunctionInvoker<Matrix4x4, Matrix4x4, Matrix4x4>(null);
		staticFunctionInvoker.Invoke(null, matrix4x, matrix4x2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Matrix4x4_op_Multiply_0()
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		Vector4 vector = default(Vector4);
		_ = matrix4x * vector;
		StaticFunctionInvoker<Matrix4x4, Vector4, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<Matrix4x4, Vector4, Vector4>(null);
		staticFunctionInvoker.Invoke(null, matrix4x, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Matrix4x4_op_Equality()
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		Matrix4x4 matrix4x2 = default(Matrix4x4);
		_ = matrix4x == matrix4x2;
		StaticFunctionInvoker<Matrix4x4, Matrix4x4, bool> staticFunctionInvoker = new StaticFunctionInvoker<Matrix4x4, Matrix4x4, bool>(null);
		staticFunctionInvoker.Invoke(null, matrix4x, matrix4x2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Matrix4x4_op_Inequality()
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		Matrix4x4 matrix4x2 = default(Matrix4x4);
		_ = matrix4x != matrix4x2;
		StaticFunctionInvoker<Matrix4x4, Matrix4x4, bool> staticFunctionInvoker = new StaticFunctionInvoker<Matrix4x4, Matrix4x4, bool>(null);
		staticFunctionInvoker.Invoke(null, matrix4x, matrix4x2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector3_op_Addition()
	{
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		_ = vector + vector2;
		StaticFunctionInvoker<Vector3, Vector3, Vector3> staticFunctionInvoker = new StaticFunctionInvoker<Vector3, Vector3, Vector3>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector3_op_Subtraction()
	{
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		_ = vector - vector2;
		StaticFunctionInvoker<Vector3, Vector3, Vector3> staticFunctionInvoker = new StaticFunctionInvoker<Vector3, Vector3, Vector3>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector3_op_UnaryNegation()
	{
		Vector3 vector = default(Vector3);
		StaticFunctionInvoker<Vector3, Vector3> staticFunctionInvoker = new StaticFunctionInvoker<Vector3, Vector3>(null);
		staticFunctionInvoker.Invoke(null, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector3_op_Multiply()
	{
		Vector3 vector = default(Vector3);
		float num = 0f;
		_ = vector * num;
		StaticFunctionInvoker<Vector3, float, Vector3> staticFunctionInvoker = new StaticFunctionInvoker<Vector3, float, Vector3>(null);
		staticFunctionInvoker.Invoke(null, vector, num);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector3_op_Multiply_0()
	{
		float num = 0f;
		Vector3 vector = default(Vector3);
		_ = num * vector;
		StaticFunctionInvoker<float, Vector3, Vector3> staticFunctionInvoker = new StaticFunctionInvoker<float, Vector3, Vector3>(null);
		staticFunctionInvoker.Invoke(null, num, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector3_op_Division()
	{
		Vector3 vector = default(Vector3);
		float num = 0f;
		_ = vector / num;
		StaticFunctionInvoker<Vector3, float, Vector3> staticFunctionInvoker = new StaticFunctionInvoker<Vector3, float, Vector3>(null);
		staticFunctionInvoker.Invoke(null, vector, num);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector3_op_Equality()
	{
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		_ = vector == vector2;
		StaticFunctionInvoker<Vector3, Vector3, bool> staticFunctionInvoker = new StaticFunctionInvoker<Vector3, Vector3, bool>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector3_op_Inequality()
	{
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		_ = vector != vector2;
		StaticFunctionInvoker<Vector3, Vector3, bool> staticFunctionInvoker = new StaticFunctionInvoker<Vector3, Vector3, bool>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Quaternion_op_Multiply()
	{
		Quaternion quaternion = default(Quaternion);
		Quaternion quaternion2 = default(Quaternion);
		_ = quaternion * quaternion2;
		StaticFunctionInvoker<Quaternion, Quaternion, Quaternion> staticFunctionInvoker = new StaticFunctionInvoker<Quaternion, Quaternion, Quaternion>(null);
		staticFunctionInvoker.Invoke(null, quaternion, quaternion2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Quaternion_op_Multiply_0()
	{
		Quaternion quaternion = default(Quaternion);
		Vector3 vector = default(Vector3);
		_ = quaternion * vector;
		StaticFunctionInvoker<Quaternion, Vector3, Vector3> staticFunctionInvoker = new StaticFunctionInvoker<Quaternion, Vector3, Vector3>(null);
		staticFunctionInvoker.Invoke(null, quaternion, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Quaternion_op_Equality()
	{
		Quaternion quaternion = default(Quaternion);
		Quaternion quaternion2 = default(Quaternion);
		_ = quaternion == quaternion2;
		StaticFunctionInvoker<Quaternion, Quaternion, bool> staticFunctionInvoker = new StaticFunctionInvoker<Quaternion, Quaternion, bool>(null);
		staticFunctionInvoker.Invoke(null, quaternion, quaternion2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Quaternion_op_Inequality()
	{
		Quaternion quaternion = default(Quaternion);
		Quaternion quaternion2 = default(Quaternion);
		_ = quaternion != quaternion2;
		StaticFunctionInvoker<Quaternion, Quaternion, bool> staticFunctionInvoker = new StaticFunctionInvoker<Quaternion, Quaternion, bool>(null);
		staticFunctionInvoker.Invoke(null, quaternion, quaternion2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Addition()
	{
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		_ = vector + vector2;
		StaticFunctionInvoker<Vector2, Vector2, Vector2> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, Vector2, Vector2>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Subtraction()
	{
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		_ = vector - vector2;
		StaticFunctionInvoker<Vector2, Vector2, Vector2> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, Vector2, Vector2>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Multiply()
	{
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		_ = vector * vector2;
		StaticFunctionInvoker<Vector2, Vector2, Vector2> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, Vector2, Vector2>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Division()
	{
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		_ = vector / vector2;
		StaticFunctionInvoker<Vector2, Vector2, Vector2> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, Vector2, Vector2>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_UnaryNegation()
	{
		Vector2 vector = default(Vector2);
		StaticFunctionInvoker<Vector2, Vector2> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, Vector2>(null);
		staticFunctionInvoker.Invoke(null, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Multiply_0()
	{
		Vector2 vector = default(Vector2);
		float num = 0f;
		_ = vector * num;
		StaticFunctionInvoker<Vector2, float, Vector2> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, float, Vector2>(null);
		staticFunctionInvoker.Invoke(null, vector, num);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Multiply_1()
	{
		float num = 0f;
		Vector2 vector = default(Vector2);
		_ = num * vector;
		StaticFunctionInvoker<float, Vector2, Vector2> staticFunctionInvoker = new StaticFunctionInvoker<float, Vector2, Vector2>(null);
		staticFunctionInvoker.Invoke(null, num, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Division_0()
	{
		Vector2 vector = default(Vector2);
		float num = 0f;
		_ = vector / num;
		StaticFunctionInvoker<Vector2, float, Vector2> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, float, Vector2>(null);
		staticFunctionInvoker.Invoke(null, vector, num);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Equality()
	{
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		_ = vector == vector2;
		StaticFunctionInvoker<Vector2, Vector2, bool> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, Vector2, bool>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Inequality()
	{
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		_ = vector != vector2;
		StaticFunctionInvoker<Vector2, Vector2, bool> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, Vector2, bool>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Implicit()
	{
		Vector3 vector = default(Vector3);
		_ = (Vector2)vector;
		StaticFunctionInvoker<Vector3, Vector2> staticFunctionInvoker = new StaticFunctionInvoker<Vector3, Vector2>(null);
		staticFunctionInvoker.Invoke(null, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector2_op_Implicit_0()
	{
		Vector2 vector = default(Vector2);
		_ = (Vector3)vector;
		StaticFunctionInvoker<Vector2, Vector3> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, Vector3>(null);
		staticFunctionInvoker.Invoke(null, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Addition()
	{
		Vector4 vector = default(Vector4);
		Vector4 vector2 = default(Vector4);
		_ = vector + vector2;
		StaticFunctionInvoker<Vector4, Vector4, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<Vector4, Vector4, Vector4>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Subtraction()
	{
		Vector4 vector = default(Vector4);
		Vector4 vector2 = default(Vector4);
		_ = vector - vector2;
		StaticFunctionInvoker<Vector4, Vector4, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<Vector4, Vector4, Vector4>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_UnaryNegation()
	{
		Vector4 vector = default(Vector4);
		StaticFunctionInvoker<Vector4, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<Vector4, Vector4>(null);
		staticFunctionInvoker.Invoke(null, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Multiply()
	{
		Vector4 vector = default(Vector4);
		float num = 0f;
		_ = vector * num;
		StaticFunctionInvoker<Vector4, float, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<Vector4, float, Vector4>(null);
		staticFunctionInvoker.Invoke(null, vector, num);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Multiply_0()
	{
		float num = 0f;
		Vector4 vector = default(Vector4);
		_ = num * vector;
		StaticFunctionInvoker<float, Vector4, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<float, Vector4, Vector4>(null);
		staticFunctionInvoker.Invoke(null, num, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Division()
	{
		Vector4 vector = default(Vector4);
		float num = 0f;
		_ = vector / num;
		StaticFunctionInvoker<Vector4, float, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<Vector4, float, Vector4>(null);
		staticFunctionInvoker.Invoke(null, vector, num);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Equality()
	{
		Vector4 vector = default(Vector4);
		Vector4 vector2 = default(Vector4);
		_ = vector == vector2;
		StaticFunctionInvoker<Vector4, Vector4, bool> staticFunctionInvoker = new StaticFunctionInvoker<Vector4, Vector4, bool>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Inequality()
	{
		Vector4 vector = default(Vector4);
		Vector4 vector2 = default(Vector4);
		_ = vector != vector2;
		StaticFunctionInvoker<Vector4, Vector4, bool> staticFunctionInvoker = new StaticFunctionInvoker<Vector4, Vector4, bool>(null);
		staticFunctionInvoker.Invoke(null, vector, vector2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Implicit()
	{
		Vector3 vector = default(Vector3);
		_ = (Vector4)vector;
		StaticFunctionInvoker<Vector3, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<Vector3, Vector4>(null);
		staticFunctionInvoker.Invoke(null, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Implicit_0()
	{
		Vector4 vector = default(Vector4);
		_ = (Vector3)vector;
		StaticFunctionInvoker<Vector4, Vector3> staticFunctionInvoker = new StaticFunctionInvoker<Vector4, Vector3>(null);
		staticFunctionInvoker.Invoke(null, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Vector4_op_Implicit_1()
	{
		Vector2 vector = default(Vector2);
		_ = (Vector4)vector;
		StaticFunctionInvoker<Vector2, Vector4> staticFunctionInvoker = new StaticFunctionInvoker<Vector2, Vector4>(null);
		staticFunctionInvoker.Invoke(null, vector);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Behaviour_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Behaviour_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Behaviour_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Component_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Component_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Component_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_GameObject_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_GameObject_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_GameObject_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LayerMask_op_Implicit()
	{
		LayerMask layerMask = default(LayerMask);
		_ = (int)layerMask;
		StaticFunctionInvoker<LayerMask, int> staticFunctionInvoker = new StaticFunctionInvoker<LayerMask, int>(null);
		staticFunctionInvoker.Invoke(null, layerMask);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_LayerMask_op_Implicit_0()
	{
		int num = 0;
		_ = (LayerMask)num;
		StaticFunctionInvoker<int, LayerMask> staticFunctionInvoker = new StaticFunctionInvoker<int, LayerMask>(null);
		staticFunctionInvoker.Invoke(null, num);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_MonoBehaviour_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_MonoBehaviour_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_MonoBehaviour_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ScriptableObject_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ScriptableObject_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ScriptableObject_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_TextAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_TextAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_TextAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Object_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Object_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Object_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ComputeShader_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ComputeShader_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ComputeShader_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ShaderVariantCollection_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ShaderVariantCollection_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_ShaderVariantCollection_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RectTransform_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RectTransform_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_RectTransform_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Transform_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Transform_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Transform_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SpriteRenderer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SpriteRenderer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SpriteRenderer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Sprite_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Sprite_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Sprite_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_U2D_Light2DBase_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_U2D_Light2DBase_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_U2D_Light2DBase_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_U2D_SpriteAtlas_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_U2D_SpriteAtlas_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_U2D_SpriteAtlas_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SceneManagement_Scene_op_Equality()
	{
		Scene scene = default(Scene);
		Scene scene2 = default(Scene);
		_ = scene == scene2;
		StaticFunctionInvoker<Scene, Scene, bool> staticFunctionInvoker = new StaticFunctionInvoker<Scene, Scene, bool>(null);
		staticFunctionInvoker.Invoke(null, scene, scene2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SceneManagement_Scene_op_Inequality()
	{
		Scene scene = default(Scene);
		Scene scene2 = default(Scene);
		_ = scene != scene2;
		StaticFunctionInvoker<Scene, Scene, bool> staticFunctionInvoker = new StaticFunctionInvoker<Scene, Scene, bool>(null);
		staticFunctionInvoker.Invoke(null, scene, scene2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Networking_PlayerConnection_PlayerConnection_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Networking_PlayerConnection_PlayerConnection_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Networking_PlayerConnection_PlayerConnection_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_GraphicsSettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_GraphicsSettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_GraphicsSettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_RenderPipelineAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_RenderPipelineAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_RenderPipelineAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_RenderPipelineGlobalSettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_RenderPipelineGlobalSettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_RenderPipelineGlobalSettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_SortingGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_SortingGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Rendering_SortingGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Playables_PlayableAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Playables_PlayableAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Playables_PlayableAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Experimental_Rendering_RayTracingShader_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Experimental_Rendering_RayTracingShader_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Experimental_Rendering_RayTracingShader_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Playables_PlayableDirector_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Playables_PlayableDirector_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Playables_PlayableDirector_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SpriteMask_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SpriteMask_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_SpriteMask_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CanvasGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CanvasGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CanvasGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CanvasRenderer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CanvasRenderer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_CanvasRenderer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Canvas_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Canvas_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Canvas_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelSettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelSettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelSettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_UIDocument_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_UIDocument_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_UIDocument_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_StyleSheet_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_StyleSheet_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_StyleSheet_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_ThemeStyleSheet_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_ThemeStyleSheet_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_ThemeStyleSheet_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelTextSettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelTextSettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelTextSettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_VisualTreeAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_VisualTreeAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_VisualTreeAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_VectorImage_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_VectorImage_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_VectorImage_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnimationFramerateLimiter_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnimationFramerateLimiter_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnimationFramerateLimiter_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ParticleCounter_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ParticleCounter_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ParticleCounter_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ParticleSystemFramerate_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ParticleSystemFramerate_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ParticleSystemFramerate_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void test_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void test_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void test_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ExportRenderTexture_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ExportRenderTexture_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ExportRenderTexture_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void SplitPointController_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void SplitPointController_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void SplitPointController_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void DualColorCaptureFeature_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void DualColorCaptureFeature_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void DualColorCaptureFeature_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void FullScreenPostProcessFeature_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void FullScreenPostProcessFeature_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void FullScreenPostProcessFeature_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Split_Test_Render_Feature_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Split_Test_Render_Feature_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Split_Test_Render_Feature_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ImageBlitFeature_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ImageBlitFeature_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ImageBlitFeature_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void PlaneController_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void PlaneController_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void PlaneController_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void FrontDoorTest_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void FrontDoorTest_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void FrontDoorTest_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void DuelSceneLayersController_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void DuelSceneLayersController_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void DuelSceneLayersController_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void NetworkMessagingHarnessLauncher_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void NetworkMessagingHarnessLauncher_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void NetworkMessagingHarnessLauncher_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void NetworkMessagingHarnessRequestPanel_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void NetworkMessagingHarnessRequestPanel_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void NetworkMessagingHarnessRequestPanel_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ScrollFadeTest_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ScrollFadeTest_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ScrollFadeTest_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void WrapperEnvironmentLauncher_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void WrapperEnvironmentLauncher_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void WrapperEnvironmentLauncher_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void WrapperSandbox_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void WrapperSandbox_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void WrapperSandbox_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AcceptableChoiceContainer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AcceptableChoiceContainer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AcceptableChoiceContainer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AcceptableChoiceContainer_AcceptableChoice_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AcceptableChoiceContainer_AcceptableChoice_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AcceptableChoiceContainer_AcceptableChoice_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AcceptableChoiceContainer_InnerListOfCriteria_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AcceptableChoiceContainer_InnerListOfCriteria_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AcceptableChoiceContainer_InnerListOfCriteria_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AudioEmitterBehavior_AudioAction_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AudioEmitterBehavior_AudioAction_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_AudioEmitterBehavior_AudioAction_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_BattlefieldRegionDefinition_SubRegion_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_BattlefieldRegionDefinition_SubRegion_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_BattlefieldRegionDefinition_SubRegion_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_CDCPart_FillerLinkedFace_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_CDCPart_FillerLinkedFace_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_CDCPart_FillerLinkedFace_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_cTMP_cTMP_Dropdown_DropdownEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_cTMP_cTMP_Dropdown_DropdownEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_cTMP_cTMP_Dropdown_DropdownEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_DeckSelectBlade_MetaDeckViewUEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_DeckSelectBlade_MetaDeckViewUEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_DeckSelectBlade_MetaDeckViewUEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_ListMetaCardView_Expanding_TagDisplayKeyValuePair_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_ListMetaCardView_Expanding_TagDisplayKeyValuePair_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_ListMetaCardView_Expanding_TagDisplayKeyValuePair_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_LowTimeWarning_LowTimeVisibilityChangedEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_LowTimeWarning_LowTimeVisibilityChangedEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_LowTimeWarning_LowTimeVisibilityChangedEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_MTGALocalizedString_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_MTGALocalizedString_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_MTGALocalizedString_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_NumCardsOfTypeInZoneConstraintContainer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_NumCardsOfTypeInZoneConstraintContainer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_NumCardsOfTypeInZoneConstraintContainer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_NumCardsOfTypeInZoneConstraintContainer_InnerListOfNumCardsOfTypeInZoneConstraints_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_NumCardsOfTypeInZoneConstraintContainer_InnerListOfNumCardsOfTypeInZoneConstraints_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_NumCardsOfTypeInZoneConstraintContainer_InnerListOfNumCardsOfTypeInZoneConstraints_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_NumCardsOfTypeInZoneConstraintContainer_NumCardsOfTypeInZoneConstraint_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_NumCardsOfTypeInZoneConstraintContainer_NumCardsOfTypeInZoneConstraint_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_NumCardsOfTypeInZoneConstraintContainer_NumCardsOfTypeInZoneConstraint_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_ParticleStylist_ParticleColorBlendSetting_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_ParticleStylist_ParticleColorBlendSetting_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_ParticleStylist_ParticleColorBlendSetting_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_PlayWindows_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_PlayWindows_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_PlayWindows_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_PlayWindows_PlayWindow_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_PlayWindows_PlayWindow_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_PlayWindows_PlayWindow_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_SliderToggle_ValueChangedEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_SliderToggle_ValueChangedEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_SliderToggle_ValueChangedEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Spinner_OptionSelector_SpinnerValueChangeEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Spinner_OptionSelector_SpinnerValueChangeEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Spinner_OptionSelector_SpinnerValueChangeEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_HorizontalAlignmentOptions_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_HorizontalAlignmentOptions_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_HorizontalAlignmentOptions_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TextAlignmentOptions_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TextAlignmentOptions_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TextAlignmentOptions_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Character_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Character_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Character_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Dropdown_DropdownEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Dropdown_DropdownEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Dropdown_DropdownEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Dropdown_OptionDataList_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Dropdown_OptionDataList_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Dropdown_OptionDataList_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_FontWeightPair_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_FontWeightPair_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_FontWeightPair_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_GlyphPairAdjustmentRecord_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_GlyphPairAdjustmentRecord_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_GlyphPairAdjustmentRecord_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_OnChangeEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_OnChangeEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_OnChangeEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_SelectionEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_SelectionEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_SelectionEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_SubmitEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_SubmitEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_SubmitEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_TextSelectionEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_TextSelectionEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_TextSelectionEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_TouchScreenKeyboardEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_TouchScreenKeyboardEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_InputField_TouchScreenKeyboardEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_SpriteCharacter_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_SpriteCharacter_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_SpriteCharacter_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_SpriteGlyph_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_SpriteGlyph_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_SpriteGlyph_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Style_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Style_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_TMP_Style_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_VerticalAlignmentOptions_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_VerticalAlignmentOptions_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_TMPro_VerticalAlignmentOptions_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UDateTime_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UDateTime_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UDateTime_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_AnimationCurve_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_AnimationCurve_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_AnimationCurve_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_EventSystems_EventTrigger_TriggerEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_EventSystems_EventTrigger_TriggerEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_EventSystems_EventTrigger_TriggerEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_Events_UnityEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_Events_UnityEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_Events_UnityEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_Events_UnityEventBase_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_Events_UnityEventBase_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_Events_UnityEventBase_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_ParticleSystem_MinMaxCurve_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_ParticleSystem_MinMaxCurve_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_ParticleSystem_MinMaxCurve_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_ParticleSystem_MinMaxGradient_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_ParticleSystem_MinMaxGradient_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_ParticleSystem_MinMaxGradient_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_Quaternion_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_Quaternion_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_Quaternion_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Glyph_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Glyph_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Glyph_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_GlyphMetrics_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_GlyphMetrics_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_GlyphMetrics_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_GlyphRect_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_GlyphRect_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_GlyphRect_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_LowLevel_GlyphPairAdjustmentRecord_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_LowLevel_GlyphPairAdjustmentRecord_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_LowLevel_GlyphPairAdjustmentRecord_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_Character_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_Character_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_Character_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_FontWeightPair_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_FontWeightPair_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_FontWeightPair_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_SpriteCharacter_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_SpriteCharacter_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_SpriteCharacter_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_SpriteGlyph_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_SpriteGlyph_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_SpriteGlyph_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_TextStyle_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_TextStyle_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_TextStyle_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_UnicodeLineBreakingRules_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_UnicodeLineBreakingRules_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_TextCore_Text_UnicodeLineBreakingRules_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_AnimationTriggers_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_AnimationTriggers_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_AnimationTriggers_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Button_ButtonClickedEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Button_ButtonClickedEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Button_ButtonClickedEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_ColorBlock_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_ColorBlock_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_ColorBlock_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Dropdown_DropdownEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Dropdown_DropdownEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Dropdown_DropdownEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Dropdown_OptionDataList_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Dropdown_OptionDataList_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Dropdown_OptionDataList_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_FontData_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_FontData_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_FontData_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_InputField_EndEditEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_InputField_EndEditEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_InputField_EndEditEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_InputField_OnChangeEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_InputField_OnChangeEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_InputField_OnChangeEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_InputField_SubmitEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_InputField_SubmitEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_InputField_SubmitEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_MaskableGraphic_CullStateChangedEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_MaskableGraphic_CullStateChangedEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_MaskableGraphic_CullStateChangedEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Navigation_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Navigation_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Navigation_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Scrollbar_ScrollEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Scrollbar_ScrollEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Scrollbar_ScrollEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_ScrollRect_ScrollRectEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_ScrollRect_ScrollRectEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_ScrollRect_ScrollRectEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Slider_SliderEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Slider_SliderEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Slider_SliderEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_SpriteState_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_SpriteState_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_SpriteState_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Toggle_ToggleEvent_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Toggle_ToggleEvent_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_UnityEngine_UI_Toggle_ToggleEvent_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Unity_Mathematics_bool2_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Unity_Mathematics_bool2_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Unity_Mathematics_bool2_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Unity_Mathematics_bool2x2_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Unity_Mathematics_bool2x2_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Unity_Mathematics_bool2x2_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Unity_Mathematics_quaternion_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Unity_Mathematics_quaternion_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Unity_Mathematics_quaternion_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Wotc_Mtga_Cards_RendererMaterialPairs_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Wotc_Mtga_Cards_RendererMaterialPairs_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_Generated_PropertyProviders_PropertyProvider_Wotc_Mtga_Cards_RendererMaterialPairs_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ShaderCycle_ShaderCycle_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ShaderCycle_ShaderCycle_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void ShaderCycle_ShaderCycle_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Test_Scenes_NetworkMessagingHarness_NetworkMessagingHarnessLoginPanel_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Test_Scenes_NetworkMessagingHarness_NetworkMessagingHarnessLoginPanel_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Test_Scenes_NetworkMessagingHarness_NetworkMessagingHarnessLoginPanel_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wotc_Mtga_Unity_IntentionArrowTesterBehavior_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wotc_Mtga_Unity_IntentionArrowTesterBehavior_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wotc_Mtga_Unity_IntentionArrowTesterBehavior_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wotc_Mtga_RenderPipeline_Battlefield_ECL_subclass_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wotc_Mtga_RenderPipeline_Battlefield_ECL_subclass_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wotc_Mtga_RenderPipeline_Battlefield_ECL_subclass_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wotc_Mtga_RenderPipeline_RenderBattlefieldFeatureECL_Kyle_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wotc_Mtga_RenderPipeline_RenderBattlefieldFeatureECL_Kyle_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wotc_Mtga_RenderPipeline_RenderBattlefieldFeatureECL_Kyle_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_Bootstrap_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_Bootstrap_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_Bootstrap_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_AccountManager_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_AccountManager_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_AccountManager_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_LoginMenu_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_LoginMenu_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_LoginMenu_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_OAuthLoginMenu_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_OAuthLoginMenu_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_OAuthLoginMenu_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_RegistrationMenu_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_RegistrationMenu_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_RegistrationMenu_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_AgeGateMenu_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_AgeGateMenu_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_AgeGateMenu_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ClientCredentials_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ClientCredentials_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ClientCredentials_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_CodeRedeemMenu_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_CodeRedeemMenu_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_CodeRedeemMenu_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ExamplesMainMenu_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ExamplesMainMenu_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ExamplesMainMenu_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_HasbroGoSDKManager_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_HasbroGoSDKManager_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_HasbroGoSDKManager_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SceneLoader_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SceneLoader_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SceneLoader_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ChallengeMenu_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ChallengeMenu_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ChallengeMenu_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ChangeSocialPresencePanel_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ChangeSocialPresencePanel_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_ChangeSocialPresencePanel_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_FriendChat_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_FriendChat_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_FriendChat_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_FriendTile_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_FriendTile_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_FriendTile_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_FriendTileHeader_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_FriendTileHeader_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_FriendTileHeader_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SendFriendRequestPanel_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SendFriendRequestPanel_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SendFriendRequestPanel_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialBust_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialBust_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialBust_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialManager_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialManager_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialManager_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialMenu_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialMenu_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialMenu_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialPanel_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialPanel_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SocialPanel_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_WizWandsManager_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_WizWandsManager_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_WizWandsManager_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SelectableTabNavigation_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SelectableTabNavigation_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void HasbroGo_SelectableTabNavigation_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TextContainer_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TextContainer_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TextContainer_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TextMeshPro_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TextMeshPro_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TextMeshPro_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TextMeshProUGUI_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TextMeshProUGUI_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TextMeshProUGUI_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Asset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Asset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Asset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_ColorGradient_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_ColorGradient_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_ColorGradient_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Dropdown_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Dropdown_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Dropdown_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_FontAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_FontAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_FontAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_InputField_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_InputField_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_InputField_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_InputValidator_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_InputValidator_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_InputValidator_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_PackageResourceImporterWindow_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_PackageResourceImporterWindow_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_PackageResourceImporterWindow_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_ScrollbarEventHandler_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_ScrollbarEventHandler_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_ScrollbarEventHandler_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SelectionCaret_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SelectionCaret_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SelectionCaret_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Settings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Settings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Settings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SpriteAnimator_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SpriteAnimator_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SpriteAnimator_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SpriteAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SpriteAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SpriteAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_StyleSheet_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_StyleSheet_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_StyleSheet_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SubMesh_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SubMesh_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SubMesh_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SubMeshUI_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SubMeshUI_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_SubMeshUI_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Text_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Text_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TMPro_TMP_Text_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_StateGraphAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_StateGraphAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_StateGraphAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_StateMachine_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_StateMachine_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_StateMachine_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSpriteRendererClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSpriteRendererClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSpriteRendererClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSpriteRendererTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSpriteRendererTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSpriteRendererTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenAudioSourceClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenAudioSourceClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenAudioSourceClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenAudioSourceTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenAudioSourceTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenAudioSourceTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCameraClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCameraClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCameraClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCameraTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCameraTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCameraTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLightClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLightClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLightClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLightTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLightTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLightTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLineRendererClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLineRendererClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLineRendererClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLineRendererTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLineRendererTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenLineRendererTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRendererClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRendererClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRendererClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRendererTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRendererTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRendererTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTransformClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTransformClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTransformClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTransformTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTransformTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTransformTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCanvasGroupClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCanvasGroupClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCanvasGroupClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCanvasGroupTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCanvasGroupTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenCanvasGroupTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenGraphicClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenGraphicClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenGraphicClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenGraphicTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenGraphicTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenGraphicTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenImageClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenImageClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenImageClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenImageTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenImageTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenImageTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenOutlineClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenOutlineClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenOutlineClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenOutlineTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenOutlineTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenOutlineTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRectTransformClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRectTransformClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRectTransformClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRectTransformTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRectTransformTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenRectTransformTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenShadowClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenShadowClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenShadowClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenShadowTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenShadowTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenShadowTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSliderClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSliderClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSliderClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSliderTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSliderTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenSliderTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextMeshProUGUIClip_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextMeshProUGUIClip_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextMeshProUGUIClip_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextMeshProUGUITrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextMeshProUGUITrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void AnnulusGames_TweenPlayables_TweenTextMeshProUGUITrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_ScriptGraphAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_ScriptGraphAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_ScriptGraphAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_ScriptMachine_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_ScriptMachine_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_ScriptMachine_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TimelinePreferences_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TimelinePreferences_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TimelinePreferences_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TimelineProjectSettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TimelineProjectSettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void TimelineProjectSettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_AnimatorMessageListener_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_AnimatorMessageListener_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_AnimatorMessageListener_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_LudiqBehaviour_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_LudiqBehaviour_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_LudiqBehaviour_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_LudiqScriptableObject_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_LudiqScriptableObject_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_LudiqScriptableObject_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_MacroScriptableObject_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_MacroScriptableObject_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Unity_VisualScripting_MacroScriptableObject_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Button_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Button_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Button_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Dropdown_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Dropdown_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Dropdown_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Graphic_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Graphic_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Graphic_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_GraphicRaycaster_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_GraphicRaycaster_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_GraphicRaycaster_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Image_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Image_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Image_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_InputField_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_InputField_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_InputField_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_AspectRatioFitter_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_AspectRatioFitter_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_AspectRatioFitter_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_CanvasScaler_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_CanvasScaler_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_CanvasScaler_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_ContentSizeFitter_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_ContentSizeFitter_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_ContentSizeFitter_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_GridLayoutGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_GridLayoutGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_GridLayoutGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_HorizontalLayoutGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_HorizontalLayoutGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_HorizontalLayoutGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_HorizontalOrVerticalLayoutGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_HorizontalOrVerticalLayoutGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_HorizontalOrVerticalLayoutGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_LayoutElement_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_LayoutElement_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_LayoutElement_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_LayoutGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_LayoutGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_LayoutGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_VerticalLayoutGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_VerticalLayoutGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_VerticalLayoutGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Mask_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Mask_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Mask_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_MaskableGraphic_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_MaskableGraphic_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_MaskableGraphic_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_RawImage_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_RawImage_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_RawImage_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_RectMask2D_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_RectMask2D_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_RectMask2D_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Scrollbar_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Scrollbar_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Scrollbar_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_ScrollRect_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_ScrollRect_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_ScrollRect_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Selectable_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Selectable_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Selectable_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Slider_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Slider_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Slider_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Text_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Text_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Text_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Toggle_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Toggle_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Toggle_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_ToggleGroup_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_ToggleGroup_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_ToggleGroup_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_BaseMeshEffect_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_BaseMeshEffect_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_BaseMeshEffect_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Outline_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Outline_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Outline_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_PositionAsUV1_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_PositionAsUV1_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_PositionAsUV1_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Shadow_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Shadow_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UI_Shadow_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelEventHandler_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelEventHandler_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelEventHandler_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelRaycaster_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelRaycaster_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_UIElements_PanelRaycaster_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_EventSystem_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_EventSystem_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_EventSystem_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_EventTrigger_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_EventTrigger_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_EventTrigger_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_BaseInput_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_BaseInput_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_BaseInput_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_BaseInputModule_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_BaseInputModule_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_BaseInputModule_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_PointerInputModule_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_PointerInputModule_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_PointerInputModule_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_StandaloneInputModule_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_StandaloneInputModule_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_StandaloneInputModule_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_BaseRaycaster_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_BaseRaycaster_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_BaseRaycaster_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_Physics2DRaycaster_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_Physics2DRaycaster_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_Physics2DRaycaster_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_PhysicsRaycaster_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_PhysicsRaycaster_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_PhysicsRaycaster_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_UIBehaviour_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_UIBehaviour_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_EventSystems_UIBehaviour_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_ActivationTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_ActivationTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_ActivationTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AnimationPlayableAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AnimationPlayableAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AnimationPlayableAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AnimationTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AnimationTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AnimationTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_TimelineAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_TimelineAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_TimelineAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_TrackAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_TrackAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_TrackAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AudioPlayableAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AudioPlayableAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AudioPlayableAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AudioTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AudioTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_AudioTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_ControlPlayableAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_ControlPlayableAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_ControlPlayableAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_ControlTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_ControlTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_ControlTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_Marker_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_Marker_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_Marker_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_MarkerTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_MarkerTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_MarkerTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalEmitter_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalEmitter_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalEmitter_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalReceiver_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalReceiver_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalReceiver_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_SignalTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_GroupTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_GroupTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_GroupTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_PlayableTrack_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_PlayableTrack_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Timeline_PlayableTrack_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputActionAsset_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputActionAsset_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputActionAsset_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputActionReference_op_Implicit()
	{
		InputActionReference inputActionReference = null;
		_ = (InputAction)inputActionReference;
		StaticFunctionInvoker<InputActionReference, InputAction> staticFunctionInvoker = new StaticFunctionInvoker<InputActionReference, InputAction>(null);
		staticFunctionInvoker.Invoke(null, inputActionReference);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputActionReference_op_Implicit_0()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputActionReference_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputActionReference_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputSettings_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputSettings_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_InputSettings_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_PlayerInput_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_PlayerInput_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_PlayerInput_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_PlayerInputManager_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_PlayerInputManager_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_PlayerInputManager_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_XR_TrackedPoseDriver_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_XR_TrackedPoseDriver_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_XR_TrackedPoseDriver_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_InputSystemUIInputModule_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_InputSystemUIInputModule_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_InputSystemUIInputModule_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_MultiplayerEventSystem_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_MultiplayerEventSystem_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_MultiplayerEventSystem_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_TrackedDeviceRaycaster_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_TrackedDeviceRaycaster_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_TrackedDeviceRaycaster_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_VirtualMouseInput_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_VirtualMouseInput_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_UI_VirtualMouseInput_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_OnScreen_OnScreenButton_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_OnScreen_OnScreenButton_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_OnScreen_OnScreenButton_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_OnScreen_OnScreenControl_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_OnScreen_OnScreenControl_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_OnScreen_OnScreenControl_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_OnScreen_OnScreenStick_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_OnScreen_OnScreenStick_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_OnScreen_OnScreenStick_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_EnhancedTouch_TouchSimulation_op_Implicit()
	{
		Object obj = null;
		_ = (bool)obj;
		StaticFunctionInvoker<Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_EnhancedTouch_TouchSimulation_op_Equality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj == obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_InputSystem_EnhancedTouch_TouchSimulation_op_Inequality()
	{
		Object obj = null;
		Object obj2 = null;
		_ = obj != obj2;
		StaticFunctionInvoker<Object, Object, bool> staticFunctionInvoker = new StaticFunctionInvoker<Object, Object, bool>(null);
		staticFunctionInvoker.Invoke(null, obj, obj2);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wizards_GeneralUtilities_ObjectCommunication_BeaconIdentifier_ID()
	{
		_ = ((ScriptableObjectWithIdentifier)null).ID;
		new InstancePropertyAccessor<ScriptableObjectWithIdentifier, string>(null).GetValue(null);
	}

	[Preserve]
	public static void Wizards_GeneralUtilities_ObjectCommunication_Beacon_GetObject()
	{
		string text = null;
		bool flag = false;
		Beacon.GetObject(text, flag);
		StaticFunctionInvoker<string, bool, Object[]> staticFunctionInvoker = new StaticFunctionInvoker<string, bool, Object[]>(null);
		staticFunctionInvoker.Invoke(null, text, flag);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wizards_DynamicTimelineBinding_BindingAssignment_SetTimelineAndBindings()
	{
		BindingAssignmentObject bindingAssignmentObject = null;
		((BindingAssignment)null).SetTimelineAndBindings(bindingAssignmentObject);
		InstanceActionInvoker<BindingAssignment, BindingAssignmentObject> instanceActionInvoker = new InstanceActionInvoker<BindingAssignment, BindingAssignmentObject>(null);
		instanceActionInvoker.Invoke(null, bindingAssignmentObject);
		instanceActionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Playables_PlayableDirector_Play()
	{
		((PlayableDirector)null).Play();
		InstanceActionInvoker<PlayableDirector> instanceActionInvoker = new InstanceActionInvoker<PlayableDirector>(null);
		instanceActionInvoker.Invoke(null);
		instanceActionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Component_gameObject()
	{
		_ = ((Component)null).gameObject;
		new InstancePropertyAccessor<Component, GameObject>(null).GetValue(null);
	}

	[Preserve]
	public static void UnityEngine_GameObject_SetActive()
	{
		bool flag = false;
		((GameObject)null).SetActive(flag);
		InstanceActionInvoker<GameObject, bool> instanceActionInvoker = new InstanceActionInvoker<GameObject, bool>(null);
		instanceActionInvoker.Invoke(null, flag);
		instanceActionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Playables_PlayableDirector_Stop()
	{
		((PlayableDirector)null).Stop();
		InstanceActionInvoker<PlayableDirector> instanceActionInvoker = new InstanceActionInvoker<PlayableDirector>(null);
		instanceActionInvoker.Invoke(null);
		instanceActionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Playables_PlayableDirector_state()
	{
		_ = ((PlayableDirector)null).state;
		new InstancePropertyAccessor<PlayableDirector, PlayState>(null).GetValue(null);
	}

	[Preserve]
	public static void Wizards_GeneralUtilities_GameEventDetails_EventId()
	{
		_ = default(GameEventDetails).EventId;
		new ReflectionPropertyAccessor(null).GetValue(default(GameEventDetails));
	}

	[Preserve]
	public static void Wizards_GeneralUtilities_GameEventDetails_Visible()
	{
		_ = default(GameEventDetails).Visible;
		new ReflectionPropertyAccessor(null).GetValue(default(GameEventDetails));
	}

	[Preserve]
	public static void Wizards_GeneralUtilities_GameEventId_ID()
	{
		_ = ((ScriptableObjectWithIdentifier)null).ID;
		new InstancePropertyAccessor<ScriptableObjectWithIdentifier, string>(null).GetValue(null);
	}

	[Preserve]
	public static void Wizards_GeneralUtilities_GameEventId_name()
	{
		_ = ((Object)null).name;
		((Object)null).name = null;
		InstancePropertyAccessor<Object, string> instancePropertyAccessor = new InstancePropertyAccessor<Object, string>(null);
		instancePropertyAccessor.GetValue(null);
		instancePropertyAccessor.SetValue(null, null);
	}

	[Preserve]
	public static void Wizards_GeneralUtilities_GameEventId_hideFlags()
	{
		_ = ((Object)null).hideFlags;
		((Object)null).hideFlags = HideFlags.None;
		InstancePropertyAccessor<Object, HideFlags> instancePropertyAccessor = new InstancePropertyAccessor<Object, HideFlags>(null);
		instancePropertyAccessor.GetValue(null);
		instancePropertyAccessor.SetValue(null, HideFlags.None);
	}

	[Preserve]
	public static void SceneLoader_GetSceneLoader()
	{
		SceneLoader.GetSceneLoader();
		StaticFunctionInvoker<SceneLoader> staticFunctionInvoker = new StaticFunctionInvoker<SceneLoader>(null);
		staticFunctionInvoker.Invoke(null);
		staticFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void SceneLoader_GetHomeEventBladeShown()
	{
		((SceneLoader)null).GetHomeEventBladeShown();
		InstanceFunctionInvoker<SceneLoader, bool> instanceFunctionInvoker = new InstanceFunctionInvoker<SceneLoader, bool>(null);
		instanceFunctionInvoker.Invoke(null);
		instanceFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void Wizards_Mtga_Npe_NpeProgressionFlag_GetCachedValue()
	{
		((NpeProgressionFlag)null).GetCachedValue();
		InstanceFunctionInvoker<NpeProgressionFlag, bool> instanceFunctionInvoker = new InstanceFunctionInvoker<NpeProgressionFlag, bool>(null);
		instanceFunctionInvoker.Invoke(null);
		instanceFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void UnityEngine_Application_isEditor()
	{
		_ = Application.isEditor;
		new StaticPropertyAccessor<bool>(null).GetValue(null);
	}

	[Preserve]
	public static void UnityEngine_Debug_LogError()
	{
		object obj = null;
		Object obj2 = null;
		Debug.LogError(obj, obj2);
		StaticActionInvoker<object, Object> staticActionInvoker = new StaticActionInvoker<object, Object>(null);
		staticActionInvoker.Invoke(null, obj, obj2);
		staticActionInvoker.Invoke(null);
	}

	[Preserve]
	public static void SceneLoader_GetIsHomeScreenReady()
	{
		((SceneLoader)null).GetIsHomeScreenReady();
		InstanceFunctionInvoker<SceneLoader, bool> instanceFunctionInvoker = new InstanceFunctionInvoker<SceneLoader, bool>(null);
		instanceFunctionInvoker.Invoke(null);
		instanceFunctionInvoker.Invoke(null);
	}

	[Preserve]
	public static void SceneLoader_GetRewardTreeUpgradeDeckShown()
	{
		((SceneLoader)null).GetRewardTreeUpgradeDeckShown();
		InstanceFunctionInvoker<SceneLoader, bool> instanceFunctionInvoker = new InstanceFunctionInvoker<SceneLoader, bool>(null);
		instanceFunctionInvoker.Invoke(null);
		instanceFunctionInvoker.Invoke(null);
	}
}
