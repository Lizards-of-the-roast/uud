using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Core.Meta.LearnMore;
using Core.Meta.NewPlayerExperience.Graph;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Credits;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.LearnMore;
using Wotc.Mtga.Loc;

public class LearnToPlayControllerV2 : NavContentController
{
	[SerializeField]
	private LearnMoreStructure structure;

	[SerializeField]
	private GameObject learnToPlayRoot;

	[SerializeField]
	private GameObject tableOfContents;

	[SerializeField]
	private GameObject tableOfContentsTopics;

	[SerializeField]
	private GameObject contentView;

	[SerializeField]
	private CustomButton _replayTutorialButton;

	[SerializeField]
	private CustomButton _creditsButton;

	[SerializeField]
	private CreditsDisplay _creditsDisplay;

	private string _selectedBubbleSectionId;

	private readonly List<string> _activeTopicsSectionIds = new List<string>();

	private string _activeContentsSectionId;

	private LearnToPlayRunTimeHierarchy _hierarchy;

	private IBILogger _biLogger = NullBILogger.Default;

	private ILearnToPlayContentBuilder _contentBuilder = NullLearnToPlayContentBuilder.Default;

	private ITableOfContentsSectionBuilder _tableOfContentBuilder = NullTableOfContentsBuilder.Default;

	private ITutorialLauncher _tutorialLauncher = NullTutorialLauncher.Default;

	private ICreditsTextProvider _creditsTextProvider = NullCreditsTextProvider.Default;

	private bool _isReadyToShow;

	public override bool IsReadyToShow => _isReadyToShow;

	public override NavContentType NavContentType => NavContentType.LearnToPlay;

	public void Init()
	{
		_hierarchy = new LearnToPlayRunTimeHierarchy(structure, Pantry.Get<PlayerPrefsDataProvider>(), Pantry.Get<NewPlayerExperienceStrategy>());
		_biLogger = Pantry.Get<IBILogger>() ?? NullBILogger.Default;
		_contentBuilder = Pantry.Get<ILearnToPlayContentBuilder>() ?? NullLearnToPlayContentBuilder.Default;
		_tableOfContentBuilder = Pantry.Get<ITableOfContentsSectionBuilder>() ?? NullTableOfContentsBuilder.Default;
		_tutorialLauncher = Pantry.Get<ITutorialLauncher>() ?? NullTutorialLauncher.Default;
		_creditsTextProvider = Pantry.Get<ICreditsTextProvider>() ?? NullCreditsTextProvider.Default;
	}

	public override void Activate(bool active)
	{
		if (!active)
		{
			DepopulateCodex();
		}
		learnToPlayRoot.SetActive(active);
		_creditsDisplay.gameObject.UpdateActive(active: false);
		if (active)
		{
			_replayTutorialButton.gameObject.UpdateActive(_tutorialLauncher.CanLaunchTutorial());
			StartCoroutine(PopulateTableOfContents().AsCoroutine());
		}
	}

	private void Awake()
	{
		_replayTutorialButton.OnMouseover.AddListener(Button_OnHover);
		_replayTutorialButton.OnClick.AddListener(TutorialButton_OnClick);
		_creditsButton.OnMouseover.AddListener(Button_OnHover);
		_creditsButton.OnClick.AddListener(ShowCredits);
		_creditsDisplay.BackButtonClicked += HideCredits;
		Languages.LanguageChangedSignal.Listeners += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		_replayTutorialButton.OnMouseover.RemoveListener(Button_OnHover);
		_replayTutorialButton.OnClick.RemoveListener(TutorialButton_OnClick);
		_creditsButton.OnMouseover.RemoveListener(Button_OnHover);
		_creditsButton.OnClick.RemoveListener(ShowCredits);
		_creditsDisplay.BackButtonClicked -= HideCredits;
		Languages.LanguageChangedSignal.Listeners -= OnLanguageChanged;
		_biLogger = NullBILogger.Default;
		_contentBuilder = NullLearnToPlayContentBuilder.Default;
		_tableOfContentBuilder = NullTableOfContentsBuilder.Default;
		_tutorialLauncher = NullTutorialLauncher.Default;
		_creditsTextProvider = NullCreditsTextProvider.Default;
	}

	private async Task PopulateTableOfContents()
	{
		await _hierarchy.Populate();
		foreach (SectionObjectReferences item in _hierarchy.Get())
		{
			if (!item.Show)
			{
				Debug.Log("LTP: " + item.Path + " TOC not loading - hidden");
				continue;
			}
			int num = item.Ancestors.Length;
			if (num >= 2)
			{
				Debug.Log($"LTP: {item.Path} TOC not loading - defer due to depth {num}");
				continue;
			}
			Debug.Log("LTP: " + item.Path + " TOC loading");
			TableOfContentsSection tableOfContentsSection = _tableOfContentBuilder.Create(item.Ancestors.Length, LocationForNestingLevel(item));
			tableOfContentsSection.Init(item);
			tableOfContentsSection.Clicked += OnButtonClicked;
			item.TableOfContentsSection = tableOfContentsSection;
		}
		_isReadyToShow = true;
	}

	private void DepopulateCodex()
	{
		HideActiveContents();
		_activeTopicsSectionIds.Clear();
		_selectedBubbleSectionId = null;
		foreach (SectionObjectReferences item in _hierarchy.Get())
		{
			if (item.ContentEntry != null)
			{
				item.ContentEntry.BackButtonClicked -= CloseActiveContent;
				_contentBuilder.DestroyContent(item.ContentEntry);
				item.ContentEntry = null;
			}
			if (item.TableOfContentsSection != null)
			{
				item.TableOfContentsSection.Clicked -= OnButtonClicked;
				_tableOfContentBuilder.Destroy(item.TableOfContentsSection);
				item.TableOfContentsSection = null;
			}
		}
	}

	private Transform LocationForNestingLevel(SectionObjectReferences refs)
	{
		return refs.Ancestors.Length switch
		{
			0 => tableOfContents.transform, 
			1 => refs.Progenitor.TableOfContentsSection.ChildAnchor.transform, 
			2 => tableOfContentsTopics.transform, 
			_ => throw new ArgumentException("Tried to creat a table of contents section at depth greater than 3!"), 
		};
	}

	private void OnButtonClicked(string fromSectionId, LearnToPlayClickIntent intent)
	{
		if (!_hierarchy.TryGet(fromSectionId, out var refs))
		{
			SimpleLog.LogError("LTP: " + fromSectionId + ": Click: not found");
			return;
		}
		switch (intent)
		{
		case LearnToPlayClickIntent.SelectToShowChildren:
			ShowChildrenOfTableOfContentsSection(refs);
			break;
		case LearnToPlayClickIntent.CloseContent:
			HideActiveContents();
			break;
		case LearnToPlayClickIntent.OpenContent:
			OpenContent(refs);
			break;
		default:
			throw new ArgumentOutOfRangeException("intent", intent, null);
		}
	}

	private void CloseActiveContent()
	{
		if (_hierarchy.TryGet(_activeContentsSectionId, out var _))
		{
			HideActiveContents();
		}
		else
		{
			SimpleLog.LogError("LTP: " + _activeContentsSectionId + ": Click: not found");
		}
	}

	private void ShowChildrenOfTableOfContentsSection(SectionObjectReferences refs)
	{
		foreach (string activeTopicsSectionId in _activeTopicsSectionIds)
		{
			if (_hierarchy.TryGet(activeTopicsSectionId, out var refs2))
			{
				HideTableOfContentsSection(refs2);
			}
		}
		if (_selectedBubbleSectionId != null && _hierarchy.TryGet(_selectedBubbleSectionId, out var refs3))
		{
			refs3.TableOfContentsSection.SetDisplayState(on: false);
		}
		_selectedBubbleSectionId = refs.Id;
		refs.TableOfContentsSection.SetDisplayState(on: true);
		foreach (SectionObjectReferences child in refs.Children)
		{
			if (child.Show)
			{
				ShowTableOfContentsSection(child);
			}
		}
	}

	private void HideTableOfContentsSection(SectionObjectReferences reference)
	{
		GameObject gameObject = reference?.TableOfContentsSection?.gameObject;
		if (gameObject != null)
		{
			gameObject.SetActive(value: false);
		}
	}

	private void ShowTableOfContentsSection(SectionObjectReferences reference)
	{
		if (reference.TableOfContentsSection == null)
		{
			reference.TableOfContentsSection = _tableOfContentBuilder.Create(reference.Ancestors.Length, LocationForNestingLevel(reference));
			if (reference.TableOfContentsSection == null)
			{
				return;
			}
			reference.TableOfContentsSection.Init(reference);
			reference.TableOfContentsSection.Clicked += OnButtonClicked;
		}
		reference.TableOfContentsSection.gameObject.SetActive(value: true);
		_activeTopicsSectionIds.Add(reference.Id);
	}

	private void OpenContent(SectionObjectReferences sectionRefs)
	{
		if (sectionRefs.ContentEntry == null)
		{
			sectionRefs.ContentEntry = _contentBuilder.InstantiateContent(sectionRefs.Path, sectionRefs.SectionInfo.SectionPrefab, contentView.transform);
			if (sectionRefs.ContentEntry == null)
			{
				return;
			}
			sectionRefs.ContentEntry.LocalizeIndexBreadcrumbs(sectionRefs.PathTitles);
			sectionRefs.ContentEntry.BackButtonClicked += CloseActiveContent;
		}
		HideActiveContents();
		_hierarchy.SetRead(sectionRefs);
		sectionRefs.TableOfContentsSection.SetDisplayState(on: true);
		sectionRefs.ContentEntry.gameObject.SetActive(value: true);
		_activeContentsSectionId = sectionRefs.Id;
		_biLogger.Send(ClientBusinessEventType.LearnUrlButtonClicked, LearnUrlButtonClicked.Create("Learn To Play v2", sectionRefs.SectionInfo.Title));
	}

	private void HideActiveContents()
	{
		if (_hierarchy.TryGet(_activeContentsSectionId, out var refs))
		{
			refs.TableOfContentsSection.SetDisplayState(on: false);
			refs.ContentEntry.gameObject.SetActive(value: false);
			_activeContentsSectionId = null;
		}
	}

	private void Button_OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	private void TutorialButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		_tutorialLauncher.LaunchTutorial();
	}

	private void ShowCredits()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		_creditsDisplay.SetCreditsText(_creditsTextProvider.GetCreditsText(), _creditsTextProvider.GetUniversesBeyondHeaderText());
		learnToPlayRoot.SetActive(value: false);
		_creditsDisplay.gameObject.UpdateActive(active: true);
	}

	private void HideCredits()
	{
		learnToPlayRoot.SetActive(value: true);
		_creditsDisplay.gameObject.UpdateActive(active: false);
	}

	private void OnLanguageChanged()
	{
		if ((bool)_creditsDisplay && _creditsDisplay.gameObject.activeSelf)
		{
			_creditsDisplay.SetCreditsText(_creditsTextProvider.GetCreditsText(), _creditsTextProvider.GetUniversesBeyondHeaderText());
		}
		foreach (SectionObjectReferences item in _hierarchy.Get())
		{
			if (item.ContentEntry != null)
			{
				item.ContentEntry.LocalizeIndexBreadcrumbs(item.PathTitles);
			}
		}
	}
}
