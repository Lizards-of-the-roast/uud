using System;
using System.Collections.Generic;
using UnityEngine;

public class VFXPetPreview : MonoBehaviour
{
	private float lastInterval;

	private float startTime;

	private float deltaTime;

	[HideInInspector]
	public Transform transformRoot;

	[HideInInspector]
	[SerializeField]
	public GameObject petPrefab;

	private GameObject petInstance;

	private GameObject lastSavedPet;

	private Animator petAnimator;

	private List<string> animNames = new List<string> { "Drag a Pet Prefab" };

	[InspectorDropDrown("animNames")]
	public string selectAnimation = "";

	[InspectorMessageWithStringHighlight("selectAnimation", "#18DB66", "The state ", " does not exist", "")]
	public string displayAnimClipErrorMessage = "";

	[HideInInspector]
	public bool toDisableButtons;

	private bool stateWithClipNameDoesNotExists;

	[Tooltip("Key in state name of Animation State from Animator Controllers")]
	[SerializeField]
	[InspectorTextField("stateWithClipNameDoesNotExists", "You must assign a state name")]
	private string stateName = "";

	[InspectorMessageWithStringHighlight("stateName", "#18DB66", "The state ", " also does not exist", "")]
	public string displayStateAnimErrorMessage = "";

	[HideInInspector]
	public GameObject[] psGameObjects;

	[HideInInspector]
	public bool AnimButtons;

	[HideInInspector]
	public bool setLockStatus;

	[HideInInspector]
	public bool setUnlockStatus = true;

	[HideInInspector]
	public string error = "";

	protected List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

	public void getAnimList()
	{
		petAnimator = petInstance.GetComponentsInChildren<Animator>(includeInactive: true)[0];
		AnimatorOverrideController animatorOverrideController = petAnimator.runtimeAnimatorController as AnimatorOverrideController;
		overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(animatorOverrideController.overridesCount);
		animatorOverrideController.GetOverrides(overrides);
		animNames.Clear();
		for (int i = 0; i < overrides.Count; i++)
		{
			animNames.Add(overrides[i].Key.name);
		}
	}

	public void PlayAnimFunc()
	{
		Debug.Log("<color=green>[=========================================================================== Playing Animation ===========================================================================]</color>");
		lastInterval = Time.realtimeSinceStartup;
		setLockStatus = true;
		setUnlockStatus = true;
	}

	public void PlayEditorTick()
	{
		startTime = Time.realtimeSinceStartup;
		deltaTime = startTime - lastInterval;
		lastInterval = startTime;
		if (!string.IsNullOrEmpty(stateName))
		{
			petAnimator.Play(stateName);
		}
		else
		{
			petAnimator.Play(selectAnimation);
		}
		petAnimator.Update(deltaTime);
		if (HasComponent<AnimateMaterialParameter>(petInstance))
		{
			ShowMaterialAnim(GetThisComponent<AnimateMaterialParameter>(petInstance));
		}
	}

	public void StopAnimFunc()
	{
		Debug.Log("<color=red>[=========================================================================== Stopping Animation ===========================================================================]</color>");
		setLockStatus = false;
		setUnlockStatus = false;
	}

	public void StopEditorAnimation()
	{
		deltaTime = 0f;
		startTime = Time.realtimeSinceStartup;
		StopSimulatingPS();
	}

	public void NoPrefabAssigned()
	{
		if (!lastSavedPet)
		{
			lastSavedPet = petInstance;
		}
		petInstance = null;
		animNames = new List<string> { "Drag a Pet Prefab" };
		psGameObjects = new GameObject[0];
	}

	public void DeleteLastPrefab()
	{
		UnityEngine.Object.DestroyImmediate(lastSavedPet);
		lastSavedPet = null;
	}

	public void RegenerateScripts()
	{
		error = "";
		AccessoryEventVFXGroup thisComponent = GetThisComponent<AccessoryEventVFXGroup>(petInstance);
		AnimateMaterialParameter thisComponent2 = GetThisComponent<AnimateMaterialParameter>(petInstance);
		if (thisComponent != null)
		{
			ParticleSystem[] vfx = thisComponent.vfx;
			Array.Resize(ref psGameObjects, vfx.Length);
			for (int i = 0; i < vfx.Length; i++)
			{
				psGameObjects[i] = vfx[i].gameObject;
			}
		}
		else
		{
			error = "Seems like <color=#FF0055><b>" + petInstance.name + "</b></color> does not have a vfx script";
			Debug.LogWarning(error);
		}
		if (thisComponent2 != null)
		{
			thisComponent2.SetMaterials();
		}
	}

	public void ShowMaterialAnim(AnimateMaterialParameter _animateMaterialParameterScript)
	{
		if (_animateMaterialParameterScript != null)
		{
			_animateMaterialParameterScript.SetFloat();
		}
	}

	public void StopSimulatingPS()
	{
		if (petInstance != null && HasComponent<AccessoryEventVFXGroup>(petInstance))
		{
			ParticleSystem[] vfx = GetThisComponent<AccessoryEventVFXGroup>(petInstance).vfx;
			for (int i = 0; i < vfx.Length; i++)
			{
				vfx[i].Simulate(0f);
			}
		}
	}

	public T GetThisComponent<T>(GameObject obj) where T : Component
	{
		if (obj.GetComponentInChildren<T>() != null)
		{
			return obj.GetComponentInChildren<T>(includeInactive: true);
		}
		return null;
	}

	public bool HasComponent<T>(GameObject obj) where T : Component
	{
		return obj.GetComponentInChildren<T>() != null;
	}

	public bool AnimatorHasState(Animator anim, string stateName, int layer = 0)
	{
		int stateID = Animator.StringToHash(stateName);
		if (anim != null)
		{
			return anim.HasState(layer, stateID);
		}
		return false;
	}

	public void returnStringIfStateNotFound(string animName, out string stringToDisplayMessage, out bool _disaBle, out bool Checker)
	{
		stringToDisplayMessage = "";
		_disaBle = true;
		Checker = !AnimatorHasState(petAnimator, animName);
		if (Checker)
		{
			stringToDisplayMessage = animName;
		}
		else
		{
			_disaBle = false;
		}
	}

	private void OnValidate()
	{
		displayAnimClipErrorMessage = "";
		displayStateAnimErrorMessage = "";
		toDisableButtons = true;
		if (!selectAnimation.Equals("Drag a Pet Prefab") && !string.IsNullOrEmpty(selectAnimation))
		{
			returnStringIfStateNotFound(selectAnimation, out displayAnimClipErrorMessage, out toDisableButtons, out stateWithClipNameDoesNotExists);
			if (!string.IsNullOrEmpty(displayAnimClipErrorMessage))
			{
				returnStringIfStateNotFound(stateName, out displayStateAnimErrorMessage, out toDisableButtons, out var _);
			}
			else
			{
				stateName = "";
			}
		}
	}

	public void SetInstance(GameObject newInstance)
	{
		if (petInstance != null)
		{
			if (petInstance.name == petPrefab.name)
			{
				return;
			}
			lastSavedPet = petInstance;
		}
		petInstance = newInstance;
		if (petInstance != null)
		{
			petInstance.transform.parent = transformRoot.transform;
			petInstance.name = petPrefab.name;
			petInstance.transform.localPosition = Vector3.zero;
			petInstance.transform.localEulerAngles = Vector3.zero;
		}
	}
}
