using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

public class PetSelector : MonoBehaviour
{
	[SerializeField]
	public Transform CardAnchor;

	[SerializeField]
	public CustomButton Button;

	[SerializeField]
	public Animator Animator;

	[SerializeField]
	public GameObject noPet;

	private string[] petVariants;

	private string loadedInstancePath;

	private GameObject petInstance;

	private AssetLookupSystem assetLookupSystem;

	private IUnityObjectPool _objectPool;

	private bool isEnabledInUI;

	private static readonly int InWrapper = Animator.StringToHash("InWrapper");

	private static readonly int Locked = Animator.StringToHash("Locked");

	public string petKey { get; private set; }

	public bool IsSkin { get; private set; }

	public int variantIndex { get; private set; }

	public bool showLocked { get; private set; }

	public void Init(string key, string[] variants, int variantIdx, bool sl, AssetLookupSystem AssetLookupSystem)
	{
		petKey = key;
		petVariants = variants;
		variantIndex = variantIdx;
		showLocked = sl;
		assetLookupSystem = AssetLookupSystem;
		IsSkin = petVariants?.ToList().TrueForAll((string x) => x.Contains("Skin")) ?? false;
	}

	public void UpdateTier(int delta)
	{
		if (variantIndex + delta < 0)
		{
			variantIndex = petVariants.Length - 1;
		}
		else if (variantIndex + delta > petVariants.Length - 1)
		{
			variantIndex = 0;
		}
		else
		{
			variantIndex += delta;
		}
		SetPetVisuals();
	}

	private void Awake()
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
		Animator.enabled = true;
		Button.OnMouseover.AddListener(Button_OnMouseOver);
	}

	private void OnDestroy()
	{
		ClearCurrentInstance();
		Button.OnMouseover.RemoveListener(Button_OnMouseOver);
		Button.OnClick.RemoveAllListeners();
	}

	public void EnablePetUIVisuals()
	{
		if (!isEnabledInUI)
		{
			isEnabledInUI = true;
			ClearCurrentInstance();
			SetPetVisuals();
		}
	}

	public void DisablePetUIVisuals()
	{
		isEnabledInUI = false;
		ClearCurrentInstance();
	}

	private void ClearCurrentInstance()
	{
		if (petInstance != null)
		{
			Object.Destroy(petInstance);
			petInstance = null;
			loadedInstancePath = string.Empty;
		}
	}

	private void Button_OnMouseOver()
	{
	}

	private void SetPetVisuals()
	{
		if (!isEnabledInUI)
		{
			return;
		}
		if (!string.IsNullOrEmpty(petKey))
		{
			string text = petVariants[variantIndex];
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.PetId = petKey;
			assetLookupSystem.Blackboard.PetVariantId = text;
			PetPayload payload = assetLookupSystem.TreeLoader.LoadTree<PetPayload>().GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				CreatePet(payload.WrapperPrefab.RelativePath);
			}
			else
			{
				Debug.LogWarningFormat("Unable to find asset for pet, key={0} variant={1}", petKey, text);
			}
		}
		else
		{
			CreatePet(string.Empty);
		}
		Animator.SetBool(Locked, showLocked);
	}

	private void CreatePet(string prefabPath)
	{
		if (prefabPath != loadedInstancePath || petInstance == null)
		{
			ClearCurrentInstance();
			if (string.IsNullOrEmpty(prefabPath))
			{
				petInstance = _objectPool.PopObject(noPet);
			}
			else
			{
				petInstance = _objectPool.PopObject(prefabPath);
			}
			loadedInstancePath = prefabPath;
		}
		petInstance.transform.SetParent(CardAnchor);
		petInstance.transform.ZeroOut();
		petInstance.transform.localRotation = Quaternion.identity;
		Animator componentInChildren = petInstance.GetComponentInChildren<Animator>();
		if (componentInChildren != null && componentInChildren.ContainsParameter(InWrapper))
		{
			componentInChildren.SetBool(InWrapper, value: true);
		}
	}
}
