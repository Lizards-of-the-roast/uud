using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class WrapperSandbox : MonoBehaviour
{
	[Header("Drag an accessory Wrapper prefab from the project here")]
	[Tooltip("Assign the Wrapper Prefabs")]
	[SerializeField]
	private List<GameObject> accessoryPrefabs = new List<GameObject>();

	private List<PetSelector> selectors = new List<PetSelector>();

	[Space(10f)]
	[Header("This field should not be empty. No touchy!")]
	[Tooltip("Assign the petselector Gameobject from scene")]
	[SerializeField]
	private GameObject petSelectorObject;

	private CustomButton _petHitbox;

	private int selectorIndex;

	private PetSelector selectedPetInstance;

	[Header("Scaffolding - SHOULD not be empty!")]
	[Tooltip("Main Container from Scene where pet selector objects are initialized")]
	[SerializeField]
	private RectTransform petContainer;

	[Tooltip("Scroller from Scene")]
	[SerializeField]
	private ScrollRect petScrollRect;

	[SerializeField]
	private bool cullNonVisibleItems;

	[SerializeField]
	private int visibleItemCount = 10;

	[SerializeField]
	private float selectorMoveDuration = 0.35f;

	[SerializeField]
	private Ease selectorMoveEase = Ease.OutQuint;

	[Tooltip("Assign Name of Pet object")]
	[SerializeField]
	private TextMeshProUGUI currentPetText;

	private static readonly int InWrapper = Animator.StringToHash("InWrapper");

	private static readonly int Wrapper_Hover = Animator.StringToHash("Wrapper_Hover");

	private static readonly int Wrapper_Click = Animator.StringToHash("Wrapper_Click");

	private void Awake()
	{
		if (petScrollRect != null && selectors != null)
		{
			petScrollRect.onValueChanged.AddListener(OnPetScrollRectValueChanged);
		}
	}

	public void AddFirstPet(List<GameObject> _accessory, GameObject _noPet)
	{
		_accessory.Insert(0, _noPet);
	}

	public void createPetSelectors(GameObject _petSelectorGO, List<GameObject> _accessory)
	{
		AddFirstPet(_accessory, _petSelectorGO.GetComponent<PetSelector>().noPet);
		if (_accessory.Count == 0)
		{
			return;
		}
		foreach (GameObject item in _accessory)
		{
			if (item != null)
			{
				createPetSelector(_petSelectorGO, item);
			}
		}
	}

	public void createPetSelector(GameObject _petSelectorGO, GameObject _accessory)
	{
		GameObject gameObject = Object.Instantiate(_petSelectorGO, petContainer);
		gameObject.SetActive(value: true);
		PetSelector petSelector = gameObject.GetComponent<PetSelector>();
		selectors.Add(petSelector);
		petSelector.Button.OnClick.AddListener(delegate
		{
			SelectPet(petSelector);
		});
		Object.Instantiate(_accessory, getThisPetsAnchor(gameObject));
	}

	private void OnPetPopupActive()
	{
		if (!(selectedPetInstance != null))
		{
			return;
		}
		Animator componentInChildren = selectedPetInstance.transform.GetChild(0).GetComponentInChildren<Animator>(includeInactive: true);
		if (componentInChildren != null)
		{
			if (componentInChildren.ContainsParameter(InWrapper))
			{
				componentInChildren.SetBool(InWrapper, value: true);
			}
			if (componentInChildren.ContainsParameter(Wrapper_Hover))
			{
				componentInChildren.Play(Wrapper_Hover);
			}
			_petHitbox = selectedPetInstance.GetComponentInChildren<CustomButton>();
			SetUpHover(componentInChildren);
			SetUpClick(componentInChildren);
		}
	}

	private void SetUpHover(Animator petAnimator)
	{
		_petHitbox.OnMouseover.AddListener(delegate
		{
			if (petAnimator.ContainsParameter(InWrapper))
			{
				petAnimator.SetBool(InWrapper, value: true);
			}
			if (petAnimator.ContainsParameter(Wrapper_Hover))
			{
				petAnimator.SetBool(Wrapper_Hover, value: true);
			}
		});
		_petHitbox.OnMouseoff.AddListener(delegate
		{
			if (petAnimator.ContainsParameter(InWrapper))
			{
				petAnimator.SetBool(InWrapper, value: true);
			}
			if (petAnimator.ContainsParameter(Wrapper_Hover))
			{
				petAnimator.SetBool(Wrapper_Hover, value: false);
			}
		});
	}

	private void SetUpClick(Animator petAnimator)
	{
		_petHitbox.OnClick.AddListener(delegate
		{
			if (petAnimator.ContainsParameter(InWrapper))
			{
				petAnimator.SetBool(InWrapper, value: true);
			}
			if (petAnimator.ContainsParameter(Wrapper_Click))
			{
				petAnimator.SetTrigger(Wrapper_Click);
			}
		});
	}

	private void Start()
	{
		selectors.Clear();
		createPetSelectors(petSelectorObject, accessoryPrefabs);
		if (petScrollRect != null)
		{
			float num = (float)selectorIndex / (float)(selectors.Count - 1);
			if (num < petScrollRect.horizontalScrollbar.size / 2f)
			{
				num = 0f;
			}
			else if (num > 1f - petScrollRect.horizontalScrollbar.size / 2f)
			{
				num = 1f;
			}
			petScrollRect.DOHorizontalNormalizedPos(num, selectorMoveDuration).SetEase(selectorMoveEase);
		}
		if (selectors != null)
		{
			UpdateSelectorVisibility();
		}
	}

	public Transform getThisPetsAnchor(GameObject _petSelectorGO)
	{
		Transform[] componentsInChildren = _petSelectorGO.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.gameObject.name.Equals("PetAnchor"))
			{
				return transform;
			}
		}
		Debug.Log("<color=red>Wrapper Sandbox - No object called PetAnchor Found in Prefab; please create one");
		return null;
	}

	public string GetPetName(PetSelector selector)
	{
		return getThisPetsAnchor(selector.gameObject).gameObject.transform.GetChild(0).gameObject.name.Replace("(Clone)", "");
	}

	public void SelectPet(PetSelector selector)
	{
		if (selectedPetInstance != selector)
		{
			if (selectedPetInstance != null)
			{
				selectedPetInstance.Animator.SetBool("Selected", value: false);
			}
			selectedPetInstance = selector;
			selectedPetInstance.Animator.SetBool("Selected", value: true);
			SetCurrentSelector(selectors.IndexOf(selectedPetInstance));
			currentPetText.SetText(GetPetName(selectedPetInstance));
			OnPetPopupActive();
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void SetCurrentSelector(int selectionIndex)
	{
		bool snapping = selectorIndex == selectionIndex;
		selectorIndex = selectionIndex;
		DOTween.Kill(petScrollRect);
		float num = (float)selectorIndex / (float)(selectors.Count - 1);
		if (petScrollRect != null)
		{
			if (num < petScrollRect.horizontalScrollbar.size / 2f)
			{
				num = 0f;
			}
			else if (num > 1f - petScrollRect.horizontalScrollbar.size / 2f)
			{
				num = 1f;
			}
			petScrollRect.DOHorizontalNormalizedPos(num, selectorMoveDuration, snapping).SetEase(selectorMoveEase);
		}
	}

	private void OnPetScrollRectValueChanged(Vector2 arg0)
	{
		if (selectors != null)
		{
			UpdateSelectorVisibility();
		}
	}

	private void UpdateSelectorVisibility()
	{
		int count = selectors.Count;
		float num = 0f;
		float num2 = count;
		if (cullNonVisibleItems)
		{
			float num3 = petScrollRect.normalizedPosition.x * (float)(count - 1);
			float num4 = (float)visibleItemCount / 2f;
			num = num3 - num4;
			num2 = num3 + num4;
			if (num < 0f)
			{
				num2 -= num;
			}
			else if (num2 > (float)count)
			{
				num -= num2 - (float)count;
			}
		}
	}
}
