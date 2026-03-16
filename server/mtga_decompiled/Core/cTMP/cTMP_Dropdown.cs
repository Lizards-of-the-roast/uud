using System;
using System.Collections;
using System.Collections.Generic;
using Core.Code.Input;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using Wotc.Mtga.Loc;

namespace cTMP;

[AddComponentMenu("UI/TMP Dropdown", 35)]
[RequireComponent(typeof(RectTransform))]
public class cTMP_Dropdown : Selectable, IPointerClickHandler, IEventSystemHandler, IAcceptActionHandler, INavigateActionHandler, IBackActionHandler, INextActionHandler, IPreviousActionHandler, ITextActionHandler
{
	protected internal class DropdownItem : Toggle, IPointerEnterHandler, IEventSystemHandler, ICancelHandler
	{
		[SerializeField]
		private TMP_Text m_Text;

		[SerializeField]
		private Image m_Image;

		[SerializeField]
		private RectTransform m_RectTransform;

		[SerializeField]
		internal DropdownItem prev;

		internal DropdownItem next;

		public TMP_Text text
		{
			get
			{
				return m_Text;
			}
			set
			{
				m_Text = value;
			}
		}

		public Image Image
		{
			get
			{
				return m_Image;
			}
			set
			{
				m_Image = value;
			}
		}

		public RectTransform rectTransform
		{
			get
			{
				return m_RectTransform;
			}
			set
			{
				m_RectTransform = value;
			}
		}

		public event Action<DropdownItem> Submit;

		public override void OnPointerEnter(PointerEventData eventData)
		{
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}

		public virtual void OnCancel(BaseEventData eventData)
		{
			cTMP_Dropdown componentInParent = GetComponentInParent<cTMP_Dropdown>();
			if ((bool)componentInParent)
			{
				componentInParent.Hide();
			}
		}

		public override void OnSubmit(BaseEventData eventData)
		{
			base.OnSubmit(eventData);
			this.Submit?.Invoke(this);
		}

		public static DropdownItem ConvertToggleToDropdownItem(Toggle toggle)
		{
			Transition transition = toggle.transition;
			Navigation navigation = toggle.navigation;
			Graphic graphic = toggle.graphic;
			Graphic graphic2 = toggle.targetGraphic;
			ToggleTransition toggleTransition = toggle.toggleTransition;
			ColorBlock colorBlock = toggle.colors;
			GameObject obj = toggle.gameObject;
			UnityEngine.Object.DestroyImmediate(toggle);
			DropdownItem dropdownItem = obj.AddComponent<DropdownItem>();
			dropdownItem.transition = transition;
			dropdownItem.navigation = navigation;
			dropdownItem.graphic = graphic;
			dropdownItem.targetGraphic = graphic2;
			dropdownItem.colors = colorBlock;
			dropdownItem.toggleTransition = toggleTransition;
			return dropdownItem;
		}
	}

	[Serializable]
	public class OptionData
	{
		[SerializeField]
		private string m_Text;

		[SerializeField]
		private Sprite m_Image;

		public string text
		{
			get
			{
				return m_Text;
			}
			set
			{
				m_Text = value;
			}
		}

		public Sprite image
		{
			get
			{
				return m_Image;
			}
			set
			{
				m_Image = value;
			}
		}

		public OptionData()
		{
		}

		public OptionData(string text)
		{
			this.text = text;
		}

		public OptionData(Sprite image)
		{
			this.image = image;
		}

		public OptionData(string text, Sprite image)
		{
			this.text = text;
			this.image = image;
		}
	}

	[Serializable]
	public class OptionDataList
	{
		[SerializeField]
		private List<OptionData> m_Options;

		public List<OptionData> options
		{
			get
			{
				return m_Options;
			}
			set
			{
				m_Options = value;
			}
		}

		public OptionDataList()
		{
			options = new List<OptionData>();
		}
	}

	[Serializable]
	public class DropdownEvent : UnityEvent<int>
	{
	}

	private Animator _animator;

	[SerializeField]
	private Selectable selectOnUp;

	[SerializeField]
	private Selectable selectOnDown;

	public bool LocalizeText;

	[SerializeField]
	private RectTransform m_Template;

	[SerializeField]
	private TMP_Text m_CaptionText;

	[SerializeField]
	private Image m_CaptionImage;

	[Space]
	[SerializeField]
	private TMP_Text m_ItemText;

	[SerializeField]
	private Image m_ItemImage;

	[Space]
	[SerializeField]
	private int m_Value;

	[Space]
	[SerializeField]
	private OptionDataList m_Options = new OptionDataList();

	[Space]
	[SerializeField]
	private DropdownEvent m_OnValueChanged = new DropdownEvent();

	private ScrollToRect m_Dropdown;

	private GameObject m_Blocker;

	private List<DropdownItem> m_Items = new List<DropdownItem>();

	private TweenRunner<FloatTween> m_AlphaTweenRunner;

	private bool validTemplate;

	private CanvasGroup _dropDownCanvasGroup;

	private static OptionData s_NoOptionData = new OptionData();

	private IActionSystem _actions;

	private Coroutine _destroyEnumerator;

	private string _findString = "";

	private float _findTimer;

	private bool _expanded;

	private Animator Animator
	{
		get
		{
			if (_animator == null)
			{
				_animator = base.gameObject.GetComponent<Animator>();
			}
			return _animator;
		}
	}

	public RectTransform template
	{
		get
		{
			return m_Template;
		}
		set
		{
			m_Template = value;
			RefreshShownValue();
		}
	}

	public TMP_Text captionText
	{
		get
		{
			return m_CaptionText;
		}
		set
		{
			m_CaptionText = value;
			RefreshShownValue();
		}
	}

	public Image captionImage
	{
		get
		{
			return m_CaptionImage;
		}
		set
		{
			m_CaptionImage = value;
			RefreshShownValue();
		}
	}

	public TMP_Text itemText
	{
		get
		{
			return m_ItemText;
		}
		set
		{
			m_ItemText = value;
			RefreshShownValue();
		}
	}

	public Image itemImage
	{
		get
		{
			return m_ItemImage;
		}
		set
		{
			m_ItemImage = value;
			RefreshShownValue();
		}
	}

	public List<OptionData> options
	{
		get
		{
			return m_Options.options;
		}
		set
		{
			m_Options.options = value;
			RefreshShownValue();
		}
	}

	public DropdownEvent onValueChanged
	{
		get
		{
			return m_OnValueChanged;
		}
		set
		{
			m_OnValueChanged = value;
		}
	}

	public int value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (!Application.isPlaying || (value != m_Value && options.Count != 0))
			{
				m_Value = Mathf.Clamp(value, -1, options.Count - 1);
				RefreshShownValue();
				m_OnValueChanged.Invoke(m_Value);
			}
		}
	}

	public bool IsExpanded => m_Dropdown != null;

	protected cTMP_Dropdown()
	{
	}

	protected override void Awake()
	{
		m_AlphaTweenRunner = new TweenRunner<FloatTween>();
		m_AlphaTweenRunner.Init(this);
		if ((bool)m_CaptionImage)
		{
			m_CaptionImage.enabled = m_CaptionImage.sprite != null;
		}
		if ((bool)m_Template)
		{
			m_Template.gameObject.SetActive(value: false);
		}
	}

	public void Init(IActionSystem actions)
	{
		_actions = actions;
	}

	public string UnlocalizedValue()
	{
		OptionData optionData = s_NoOptionData;
		if (options.Count > 0)
		{
			optionData = options[Mathf.Clamp(m_Value, 0, options.Count - 1)];
		}
		if (optionData != null && optionData.text != null)
		{
			return optionData.text;
		}
		return "";
	}

	public void RefreshShownValue()
	{
		OptionData optionData = s_NoOptionData;
		if (options.Count > 0)
		{
			optionData = options[Mathf.Clamp(m_Value, 0, options.Count - 1)];
		}
		if ((bool)m_CaptionText)
		{
			if (optionData != null && optionData.text != null)
			{
				m_CaptionText.text = (LocalizeText ? Languages.ActiveLocProvider.GetLocalizedText(optionData.text) : optionData.text);
			}
			else
			{
				m_CaptionText.text = "";
			}
		}
		if ((bool)m_CaptionImage)
		{
			if (optionData != null)
			{
				m_CaptionImage.sprite = optionData.image;
			}
			else
			{
				m_CaptionImage.sprite = null;
			}
			m_CaptionImage.enabled = m_CaptionImage.sprite != null;
		}
	}

	public void AddOptions(List<OptionData> options)
	{
		this.options.AddRange(options);
		RefreshShownValue();
	}

	public void AddOptions(List<string> options)
	{
		for (int i = 0; i < options.Count; i++)
		{
			this.options.Add(new OptionData(options[i]));
		}
		RefreshShownValue();
	}

	public void AddOptions(List<Sprite> options)
	{
		for (int i = 0; i < options.Count; i++)
		{
			this.options.Add(new OptionData(options[i]));
		}
		RefreshShownValue();
	}

	public void ClearOptions()
	{
		options.Clear();
		RefreshShownValue();
	}

	private void SetupTemplate()
	{
		validTemplate = false;
		if (!m_Template)
		{
			Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", this);
			return;
		}
		GameObject gameObject = m_Template.gameObject;
		gameObject.SetActive(value: true);
		Toggle componentInChildren = m_Template.GetComponentInChildren<Toggle>();
		validTemplate = true;
		if (!componentInChildren || componentInChildren.transform == template)
		{
			validTemplate = false;
			Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", template);
		}
		else if (!(componentInChildren.transform.parent is RectTransform))
		{
			validTemplate = false;
			Debug.LogError("The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", template);
		}
		else if (itemText != null && !itemText.transform.IsChildOf(componentInChildren.transform))
		{
			validTemplate = false;
			Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", template);
		}
		else if (itemImage != null && !itemImage.transform.IsChildOf(componentInChildren.transform))
		{
			validTemplate = false;
			Debug.LogError("The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", template);
		}
		if (!validTemplate)
		{
			gameObject.SetActive(value: false);
			return;
		}
		DropdownItem dropdownItem = DropdownItem.ConvertToggleToDropdownItem(componentInChildren);
		componentInChildren = null;
		dropdownItem.text = m_ItemText;
		dropdownItem.Image = m_ItemImage;
		dropdownItem.rectTransform = (RectTransform)dropdownItem.transform;
		Canvas orAddComponent = GetOrAddComponent<Canvas>(gameObject);
		orAddComponent.overrideSorting = true;
		orAddComponent.sortingOrder = 30000;
		GetOrAddComponent<GraphicRaycaster>(gameObject);
		GetOrAddComponent<CanvasGroup>(gameObject);
		gameObject.SetActive(value: false);
		validTemplate = true;
	}

	private static T GetOrAddComponent<T>(GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (!val)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		Show();
	}

	public void Show()
	{
		if (!IsActive() || !IsInteractable() || m_Dropdown != null)
		{
			return;
		}
		if (!validTemplate)
		{
			SetupTemplate();
			if (!validTemplate)
			{
				return;
			}
		}
		if (_actions != null)
		{
			_actions.PushFocus(this);
		}
		List<Canvas> list = cTMP_ListPool<Canvas>.Get();
		base.gameObject.GetComponentsInParent(includeInactive: false, list);
		if (list.Count == 0)
		{
			return;
		}
		Canvas canvas = list[0];
		cTMP_ListPool<Canvas>.Release(list);
		m_Template.gameObject.SetActive(value: true);
		GameObject gameObject = CreateDropdownList(m_Template.gameObject);
		m_Dropdown = gameObject.AddComponent<ScrollToRect>();
		_dropDownCanvasGroup = m_Dropdown.GetComponent<CanvasGroup>();
		m_Dropdown.name = "Dropdown List";
		m_Dropdown.gameObject.SetActive(value: true);
		RectTransform rectTransform = m_Dropdown.transform as RectTransform;
		rectTransform.SetParent(m_Template.transform.parent, worldPositionStays: false);
		DropdownItem componentInChildren = m_Dropdown.GetComponentInChildren<DropdownItem>();
		RectTransform rectTransform2 = componentInChildren.rectTransform.parent.gameObject.transform as RectTransform;
		componentInChildren.rectTransform.gameObject.SetActive(value: true);
		Rect rect = rectTransform2.rect;
		Rect rect2 = componentInChildren.rectTransform.rect;
		Vector2 vector = rect2.min - rect.min + (Vector2)componentInChildren.rectTransform.localPosition;
		Vector2 vector2 = rect2.max - rect.max + (Vector2)componentInChildren.rectTransform.localPosition;
		Vector2 size = rect2.size;
		m_Items.Clear();
		DropdownItem dropdownItem = null;
		for (int i = 0; i < options.Count; i++)
		{
			OptionData data = options[i];
			DropdownItem item = AddItem(data, value == i, componentInChildren, m_Items);
			if (!(item == null))
			{
				item.isOn = value == i;
				item.onValueChanged.AddListener(delegate
				{
					OnSelectItem(item);
				});
				item.Submit += OnItemSubmitted;
				if (item.isOn)
				{
					item.Select();
					item.text.color = Color.white;
				}
				if (dropdownItem != null)
				{
					Navigation navigation = dropdownItem.navigation;
					Navigation navigation2 = item.navigation;
					navigation.mode = Navigation.Mode.None;
					navigation2.mode = Navigation.Mode.None;
					dropdownItem.navigation = navigation;
					item.navigation = navigation2;
					item.prev = dropdownItem;
					dropdownItem.next = item;
				}
				dropdownItem = item;
			}
		}
		Vector2 sizeDelta = rectTransform2.sizeDelta;
		sizeDelta.y = size.y * (float)m_Items.Count + vector.y - vector2.y;
		rectTransform2.sizeDelta = sizeDelta;
		float num = rectTransform.rect.height - rectTransform2.rect.height;
		if (num > 0f)
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - num);
		}
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		RectTransform rectTransform3 = canvas.transform as RectTransform;
		Rect rect3 = rectTransform3.rect;
		for (int num2 = 0; num2 < 2; num2++)
		{
			bool flag = false;
			for (int num3 = 0; num3 < 4; num3++)
			{
				Vector3 vector3 = rectTransform3.InverseTransformPoint(array[num3]);
				if (vector3[num2] < rect3.min[num2] || vector3[num2] > rect3.max[num2])
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				RectTransformUtility.FlipLayoutOnAxis(rectTransform, num2, keepPositioning: false, recursive: false);
			}
		}
		for (int num4 = 0; num4 < m_Items.Count; num4++)
		{
			RectTransform rectTransform4 = m_Items[num4].rectTransform;
			rectTransform4.anchorMin = new Vector2(rectTransform4.anchorMin.x, 0f);
			rectTransform4.anchorMax = new Vector2(rectTransform4.anchorMax.x, 0f);
			rectTransform4.anchoredPosition = new Vector2(rectTransform4.anchoredPosition.x, vector.y + size.y * (float)(m_Items.Count - 1 - num4) + size.y * rectTransform4.pivot.y);
			rectTransform4.sizeDelta = new Vector2(rectTransform4.sizeDelta.x, size.y);
		}
		AlphaFadeList(0.15f, 0f, 1f);
		m_Template.gameObject.SetActive(value: false);
		componentInChildren.gameObject.SetActive(value: false);
		m_Blocker = CreateBlocker(canvas);
	}

	protected virtual GameObject CreateBlocker(Canvas rootCanvas)
	{
		GameObject obj = new GameObject("Blocker");
		RectTransform rectTransform = obj.AddComponent<RectTransform>();
		rectTransform.SetParent(rootCanvas.transform, worldPositionStays: false);
		rectTransform.anchorMin = Vector3.zero;
		rectTransform.anchorMax = Vector3.one;
		rectTransform.sizeDelta = Vector2.zero;
		Canvas canvas = obj.AddComponent<Canvas>();
		canvas.overrideSorting = true;
		Canvas component = m_Dropdown.GetComponent<Canvas>();
		canvas.sortingLayerID = component.sortingLayerID;
		canvas.sortingOrder = component.sortingOrder - 1;
		obj.AddComponent<GraphicRaycaster>();
		obj.AddComponent<Image>().color = Color.clear;
		obj.AddComponent<Button>().onClick.AddListener(Hide);
		return obj;
	}

	protected virtual void DestroyBlocker(GameObject blocker)
	{
		UnityEngine.Object.Destroy(blocker);
	}

	protected virtual GameObject CreateDropdownList(GameObject template)
	{
		return UnityEngine.Object.Instantiate(template);
	}

	protected virtual void DestroyDropdownList(GameObject dropdownList)
	{
		UnityEngine.Object.Destroy(dropdownList);
	}

	protected virtual DropdownItem CreateItem(DropdownItem itemTemplate)
	{
		return UnityEngine.Object.Instantiate(itemTemplate);
	}

	protected virtual void DestroyItem(DropdownItem item)
	{
	}

	private DropdownItem AddItem(OptionData data, bool selected, DropdownItem itemTemplate, List<DropdownItem> items)
	{
		DropdownItem dropdownItem = CreateItem(itemTemplate);
		dropdownItem.rectTransform.SetParent(itemTemplate.rectTransform.parent, worldPositionStays: false);
		dropdownItem.gameObject.SetActive(value: true);
		dropdownItem.gameObject.name = "Item " + items.Count + ((data.text != null) ? (": " + data.text) : "");
		dropdownItem.isOn = false;
		if ((bool)dropdownItem.text)
		{
			dropdownItem.text.text = (LocalizeText ? Languages.ActiveLocProvider.GetLocalizedText(data.text) : data.text);
		}
		if ((bool)dropdownItem.Image)
		{
			dropdownItem.Image.sprite = data.image;
			dropdownItem.Image.enabled = dropdownItem.Image.sprite != null;
		}
		items.Add(dropdownItem);
		return dropdownItem;
	}

	private void AlphaFadeList(float duration, float alpha)
	{
		AlphaFadeList(duration, _dropDownCanvasGroup.alpha, alpha);
	}

	private void AlphaFadeList(float duration, float start, float end)
	{
		if (!end.Equals(start))
		{
			FloatTween info = new FloatTween
			{
				duration = duration,
				startValue = start,
				targetValue = end
			};
			info.AddOnChangedCallback(SetAlpha);
			info.ignoreTimeScale = true;
			m_AlphaTweenRunner.StartTween(info);
		}
	}

	private void SetAlpha(float alpha)
	{
		if ((bool)m_Dropdown)
		{
			_dropDownCanvasGroup.alpha = alpha;
		}
	}

	public void Hide()
	{
		if (m_Dropdown != null)
		{
			AlphaFadeList(0.15f, 0f);
			if (IsActive() && _destroyEnumerator == null)
			{
				_actions?.PopFocus(this);
				_destroyEnumerator = StartCoroutine(DelayedDestroyDropdownList(0.15f));
			}
		}
		if (m_Blocker != null)
		{
			DestroyBlocker(m_Blocker);
		}
		m_Blocker = null;
	}

	private IEnumerator DelayedDestroyDropdownList(float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		for (int i = 0; i < m_Items.Count; i++)
		{
			if (m_Items[i] != null)
			{
				DestroyItem(m_Items[i]);
			}
			m_Items.Clear();
		}
		if (m_Dropdown != null)
		{
			DestroyDropdownList(m_Dropdown.gameObject);
		}
		m_Dropdown = null;
		_destroyEnumerator = null;
	}

	private void OnSelectItem(DropdownItem toggle)
	{
		if (value >= 0 && value < m_Items.Count && m_Items[value].graphic != null)
		{
			m_Items[value].graphic.gameObject.SetActive(value: false);
		}
		if (!toggle.isOn)
		{
			toggle.isOn = true;
		}
		int num = -1;
		Transform transform = toggle.transform;
		Transform parent = transform.parent;
		for (int i = 0; i < parent.childCount; i++)
		{
			if (parent.GetChild(i) == transform)
			{
				num = i - 1;
				break;
			}
		}
		if (num >= 0)
		{
			value = num;
			Hide();
		}
	}

	private void OnItemSubmitted(DropdownItem toggle)
	{
		if (selectOnDown != null)
		{
			Hide();
			selectOnDown.Select();
		}
	}

	private void SelectGameObject(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(go);
		m_Dropdown.ScrollToSelected(go);
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (Animator != null)
		{
			base.animator.SetTrigger("Hover");
		}
		base.OnSelect(eventData);
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		if (Animator != null)
		{
			base.animator.SetTrigger("Up");
		}
		base.OnSelect(eventData);
	}

	private void Update()
	{
		if (m_Dropdown == null)
		{
			_expanded = false;
			return;
		}
		if (!_expanded)
		{
			_expanded = true;
			return;
		}
		_findTimer += Time.deltaTime;
		if (_findTimer > 1f)
		{
			_findTimer = 0f;
			_findString = "";
		}
	}

	public void OnAccept()
	{
		if (!_expanded)
		{
			Show();
		}
	}

	public void OnBack(ActionContext context)
	{
		Hide();
	}

	public void OnNext()
	{
		if (selectOnDown != null)
		{
			Hide();
			selectOnDown.Select();
		}
	}

	public void OnPrevious()
	{
		if (selectOnUp != null)
		{
			Hide();
			selectOnUp.Select();
		}
	}

	public void OnNavigate(Direction dir)
	{
		if (m_Dropdown == null || !_expanded)
		{
			return;
		}
		DropdownItem dropdownItem = (EventSystem.current.currentSelectedGameObject ? EventSystem.current.currentSelectedGameObject.GetComponent<DropdownItem>() : null);
		if (dropdownItem == null)
		{
			if (m_Items.Count > 0)
			{
				SelectGameObject(m_Items[0].gameObject);
			}
			return;
		}
		switch (dir)
		{
		case Direction.Up:
		case Direction.Left:
			if ((bool)dropdownItem.prev)
			{
				SelectGameObject(dropdownItem.prev.gameObject);
			}
			break;
		case Direction.Right:
		case Direction.Down:
			if ((bool)dropdownItem.next)
			{
				SelectGameObject(dropdownItem.next.gameObject);
			}
			break;
		}
	}

	public void OnTextInput(char inputtedCharacter)
	{
		if (inputtedCharacter < ' ' || inputtedCharacter > '~')
		{
			return;
		}
		_findString += inputtedCharacter;
		_findTimer = 0f;
		foreach (DropdownItem item in m_Items)
		{
			if (item.text.text.ToLower().StartsWith(_findString))
			{
				SelectGameObject(item.gameObject);
				break;
			}
		}
	}
}
