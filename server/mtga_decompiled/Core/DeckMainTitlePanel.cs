using System.Collections;
using System.Collections.Generic;
using Core.Code.Decks;
using Core.Meta.Cards.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class DeckMainTitlePanel : MonoBehaviour
{
	[SerializeField]
	private Localize _cardCountLabel;

	[SerializeField]
	private Animator _highlightAnimator;

	[SerializeField]
	private CustomButton _button;

	[SerializeField]
	private MeshRenderer[] _deckBoxRenderers;

	[SerializeField]
	private DeckViewImages _deckViewImages;

	[SerializeField]
	private TMP_InputField _nameInput;

	[SerializeField]
	private TextMeshProUGUI _detailsText;

	[SerializeField]
	private LayoutGroup _barsLayout;

	[SerializeField]
	private TMP_Dropdown _formatDropdown;

	[SerializeField]
	private TMP_Text _selectFormatText;

	[SerializeField]
	private ParticleSystem _particleSystem;

	[SerializeField]
	private Color _cardCountWarningColor = new Color(0.87f, 0.62f, 0.078f);

	[SerializeField]
	private Color _cardCountErrorColor = new Color(0.72f, 0.07f, 0.07f);

	private Color _cardCountDefaultColor = Color.black;

	private bool _dropStateOn;

	private List<DeckFormat> _availableFormats;

	private MeshRendererReferenceLoader[] _meshRendererReferenceLoaders;

	public CustomButton Button => _button;

	private DeckBuilderContextProvider DeckBuilderContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	public void SetActive(bool active)
	{
		_button.gameObject.SetActive(active);
	}

	public void SetCardCount(int count, DeckFormat deckFormat, int modifiedMin = -1, bool alwaysDefaultColor = false)
	{
		if (modifiedMin < 0)
		{
			modifiedMin = deckFormat.MinMainDeckCards;
		}
		if (modifiedMin == 0)
		{
			alwaysDefaultColor = true;
			string key = ((count == 1) ? "MainNav/Draft/Sideboard_CardQuantity_Label_Singular" : "MainNav/Draft/Sideboard_CardQuantity_Label_Plural");
			Dictionary<string, string> parameters = new Dictionary<string, string> { 
			{
				"currentQuantity",
				count.ToString()
			} };
			_cardCountLabel.SetText(key, parameters);
		}
		else
		{
			string key2 = ((deckFormat.MinMainDeckCards == 1) ? "MainNav/Draft/MainDeck_CardQuantity_Label_Singular" : "MainNav/Draft/MainDeck_CardQuantity_Label_Plural");
			Dictionary<string, string> parameters2 = new Dictionary<string, string>
			{
				{
					"currentQuantity",
					count.ToString()
				},
				{
					"expectedQuantity",
					modifiedMin.ToString()
				}
			};
			_cardCountLabel.SetText(key2, parameters2);
		}
		TMP_Text tMP_Text = (TMP_Text)_cardCountLabel.TextTarget.serializedCmp;
		if (!alwaysDefaultColor && (count < deckFormat.MinMainDeckCards || count > deckFormat.MaxMainDeckCards))
		{
			tMP_Text.color = _cardCountErrorColor;
		}
		else if (!alwaysDefaultColor && count < modifiedMin)
		{
			tMP_Text.color = _cardCountWarningColor;
		}
		else
		{
			tMP_Text.color = _cardCountDefaultColor;
		}
	}

	public void SetDeckFormatSelector(List<DeckFormat> availableFormats, bool isAmbiguousFormat, bool isFirstEdit)
	{
		if (isAmbiguousFormat)
		{
			_formatDropdown.gameObject.UpdateActive(isFirstEdit);
			if (_particleSystem != null)
			{
				_particleSystem.gameObject.UpdateActive(isFirstEdit);
			}
			_nameInput.gameObject.UpdateActive(!isFirstEdit);
			_detailsText.gameObject.UpdateActive(!isFirstEdit);
			_barsLayout.gameObject.UpdateActive(!isFirstEdit);
			SetDropState(!isFirstEdit);
			_availableFormats = availableFormats;
			_formatDropdown.enabled = false;
			_formatDropdown.onValueChanged.RemoveListener(FormatDropdown_OnValueChanged);
			_formatDropdown.ClearOptions();
			List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
			for (int i = 0; i < availableFormats.Count; i++)
			{
				list.Add(new TMP_Dropdown.OptionData(_availableFormats[i].GetLocalizedName()));
			}
			list.Add(new TMP_Dropdown.OptionData(""));
			_formatDropdown.AddOptions(list);
			_formatDropdown.SetValueWithoutNotify(_formatDropdown.options.Count - 1);
			_formatDropdown.options.RemoveAt(_formatDropdown.options.Count - 1);
			_formatDropdown.enabled = true;
			_formatDropdown.onValueChanged.AddListener(FormatDropdown_OnValueChanged);
			_formatDropdown.captionText.gameObject.SetActive(value: false);
			_selectFormatText.gameObject.SetActive(value: true);
			_selectFormatText.text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Deckbuilder/Format/SelectFormat");
		}
		else
		{
			_formatDropdown.gameObject.UpdateActive(active: false);
			_nameInput.gameObject.UpdateActive(active: true);
			_detailsText.gameObject.UpdateActive(active: true);
			_barsLayout.gameObject.UpdateActive(active: true);
			SetDropState(on: true);
		}
	}

	private void OnDeckFormatSet(DeckFormat format)
	{
		HideDeckFormatSelector();
	}

	public void HideDeckFormatSelector()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(Coroutine_DeactivateFormatDropdown());
			return;
		}
		SetDeckFormatSelector(null, isAmbiguousFormat: false, isFirstEdit: false);
		if (_particleSystem != null)
		{
			_particleSystem.gameObject.SetActive(value: false);
		}
	}

	private void FormatDropdown_OnValueChanged(int value)
	{
		_formatDropdown.captionText.gameObject.SetActive(value: true);
		_selectFormatText.gameObject.SetActive(value: false);
		DeckBuilderContextProvider.SelectFormat(_availableFormats[value]);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
	}

	private IEnumerator Coroutine_DeactivateFormatDropdown()
	{
		yield return new WaitUntil(() => !_formatDropdown.IsExpanded);
		if (_particleSystem != null)
		{
			_particleSystem.Play();
		}
		AudioManager.PlayAudio("sfx_ui_small_magical_swish", AudioManager.Default);
		SetDeckFormatSelector(null, isAmbiguousFormat: false, isFirstEdit: false);
		if (_particleSystem != null)
		{
			yield return new WaitUntil(() => !_particleSystem.IsAlive());
			_particleSystem.gameObject.SetActive(value: false);
		}
	}

	private void Awake()
	{
		TMP_Text tMP_Text = (TMP_Text)_cardCountLabel.TextTarget.serializedCmp;
		_cardCountDefaultColor = tMP_Text.color;
		if (_formatDropdown != null)
		{
			_formatDropdown.GetComponent<CustomButton>().OnMouseover.AddListener(delegate
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
			});
			_formatDropdown.GetComponent<CustomButton>().OnClick.AddListener(delegate
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_filter_toggle, AudioManager.Default);
			});
		}
		_meshRendererReferenceLoaders = new MeshRendererReferenceLoader[_deckBoxRenderers.Length];
		for (int num = 0; num < _deckBoxRenderers.Length; num++)
		{
			_meshRendererReferenceLoaders[num] = new MeshRendererReferenceLoader(_deckBoxRenderers[num]);
		}
	}

	private void OnDestroy()
	{
		if (_meshRendererReferenceLoaders != null)
		{
			MeshRendererReferenceLoader[] meshRendererReferenceLoaders = _meshRendererReferenceLoaders;
			for (int i = 0; i < meshRendererReferenceLoaders.Length; i++)
			{
				meshRendererReferenceLoaders[i]?.Cleanup();
			}
			_meshRendererReferenceLoaders = null;
		}
	}

	public void OnEnable()
	{
		DeckBuilderContextProvider.OnDeckFormatSet += OnDeckFormatSet;
		Pantry.Get<MetaCardViewDragState>().OnDragStateChange += OnDragStateChange;
		ModelProvider.OnDeckBoxTextureChanged += SetDeckBoxTexture;
		var (artPath, crop) = DeckBuilderModelProvider.GetDeckBoxTextureInformation(Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>(), ModelProvider.Model._deckTileId, ModelProvider.Model._deckArtId);
		SetDeckBoxTexture(artPath, crop);
		VisualsUpdater.MainDeckCountVisualsUpdated += OnMainDeckCountVisualsUpdated;
		OnMainDeckCountVisualsUpdated(VisualsUpdater.MainDeckCurrentSize, VisualsUpdater.MainDeckMaxSize);
		DeckBuilderContext context = DeckBuilderContextProvider.Context;
		if (!context.IsDrafting)
		{
			List<DeckFormat> availableFormats = _availableFormats;
			List<DeckFormat> availableFormats2 = ((availableFormats != null && availableFormats.Count > 0) ? _availableFormats : DeckBuilderWidgetUtilities.GetAvailableFormats(Pantry.Get<FormatManager>().GetAllFormats(), Pantry.Get<EventManager>().EventContexts, context.Format));
			SetDeckFormatSelector(availableFormats2, context.IsAmbiguousFormat, context.IsFirstEdit);
		}
	}

	public void OnDisable()
	{
		DeckBuilderContextProvider.OnDeckFormatSet -= OnDeckFormatSet;
		Pantry.Get<MetaCardViewDragState>().OnDragStateChange -= OnDragStateChange;
		ModelProvider.OnDeckBoxTextureChanged -= SetDeckBoxTexture;
		VisualsUpdater.MainDeckCountVisualsUpdated -= OnMainDeckCountVisualsUpdated;
	}

	private void OnDragStateChange(MetaCardView draggingCard)
	{
		bool flag = draggingCard != null;
		if (_dropStateOn != flag)
		{
			SetDropState(flag);
		}
	}

	private void SetDropState(bool on)
	{
		_dropStateOn = on;
		_highlightAnimator.SetTrigger("Highlight", _dropStateOn);
	}

	public void SetDeckBoxTexture(string artPath, ArtCrop crop)
	{
		DeckBoxUtil.SetDeckBoxTexture(artPath, crop, _meshRendererReferenceLoaders, _deckViewImages.DefaultDeckTexture);
	}

	public void OnMainDeckCountVisualsUpdated(int mainCount, int mainboardMax)
	{
		SetCardCount(mainCount, DeckBuilderContextProvider.Context.Format, mainboardMax);
	}
}
