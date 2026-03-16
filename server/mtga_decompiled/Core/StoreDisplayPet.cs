using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using Pooling;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

public class StoreDisplayPet : StoreItemDisplay
{
	[SerializeField]
	private Transform Anchor;

	[SerializeField]
	private Image _customBacker;

	private Animator petAnimator;

	private IUnityObjectPool _objectPool;

	private static readonly int InWrapper = Animator.StringToHash("InWrapper");

	private static readonly int WrapperHover = Animator.StringToHash("Wrapper_Hover");

	private static readonly int WrapperClick = Animator.StringToHash("Wrapper_Click");

	public void Awake()
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
	}

	public void OnEnable()
	{
		if (petAnimator != null && petAnimator.ContainsParameter(InWrapper))
		{
			petAnimator.SetBool(InWrapper, value: true);
		}
	}

	public void CreatePet(string petKey, string variant, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.PetId = petKey;
		assetLookupSystem.Blackboard.PetVariantId = variant;
		PetPayload payload = assetLookupSystem.TreeLoader.LoadTree<PetPayload>().GetPayload(assetLookupSystem.Blackboard);
		if (payload != null)
		{
			GameObject gameObject = _objectPool.PopObject(payload.WrapperPrefab.RelativePath);
			petAnimator = gameObject.GetComponentInChildren<Animator>();
			if (petAnimator != null && petAnimator.ContainsParameter(InWrapper))
			{
				petAnimator.SetBool(InWrapper, value: true);
			}
			gameObject.transform.SetParent(Anchor);
			gameObject.transform.ZeroOut();
			gameObject.transform.localRotation = Quaternion.identity;
		}
		else
		{
			Debug.LogError("Unable to find asset for pet, key=" + petKey + " variant=" + variant);
		}
	}

	public override void SetBackgroundSprite(Sprite sprite)
	{
		if ((object)sprite != null)
		{
			_customBacker.gameObject.UpdateActive(sprite != null);
			_customBacker.sprite = sprite;
		}
	}

	public override void Hover(bool on)
	{
		base.Hover(on);
		if (petAnimator != null && petAnimator.ContainsParameter(WrapperHover))
		{
			if (on)
			{
				petAnimator.SetTrigger(WrapperHover);
			}
			else
			{
				petAnimator.ResetTrigger(WrapperHover);
			}
		}
	}

	public override void OnClick()
	{
		if (petAnimator != null && petAnimator.ContainsParameter(WrapperClick))
		{
			petAnimator.SetTrigger(WrapperClick);
		}
	}
}
