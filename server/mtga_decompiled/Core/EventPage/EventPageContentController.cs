using System.Collections;
using System.Collections.Generic;
using AssetLookupTree.Payloads.Prefab;
using EventPage.Components;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

namespace EventPage;

public class EventPageContentController : NavContentController
{
	private readonly struct EventPage
	{
		public readonly EventComponentManager ComponentManager;

		public readonly EventPageScaffolding EventScaffolding;

		public EventPage(EventComponentManager componentManager, EventPageScaffolding eventScaffolding)
		{
			ComponentManager = componentManager;
			EventScaffolding = eventScaffolding;
		}
	}

	private SharedEventPageClasses _sharedClasses;

	private EventContext _currentEventContext;

	private EventPage? _currentEventPage;

	private Dictionary<string, EventPage> _instantiatedEventPages = new Dictionary<string, EventPage>(5);

	private EventPageComponentFactory _factory;

	private bool _readyToshow;

	public override NavContentType NavContentType => NavContentType.EventLanding;

	public override bool SkipScreen => _currentEventPage?.ComponentManager.SkipPage ?? false;

	public override bool IsReadyToShow
	{
		get
		{
			ref EventPage? currentEventPage = ref _currentEventPage;
			if (currentEventPage.HasValue && currentEventPage.GetValueOrDefault().ComponentManager.ReadyToShow)
			{
				return _readyToshow;
			}
			return false;
		}
	}

	public EventContext CurrentEventContext => _currentEventContext;

	protected virtual string _currentTemplateKey => _currentEventContext.PlayerEvent.EventInfo.InternalEventName;

	public virtual void Init(SharedEventPageClasses sharedClasses)
	{
		_sharedClasses = sharedClasses;
		_factory = new EventPageComponentFactory(sharedClasses, this);
	}

	public override void Skipped()
	{
		_currentEventPage?.ComponentManager?.Skipped();
	}

	public override void OnBeginOpen()
	{
		if (_sharedClasses.EventManager.GetEventContext(_currentTemplateKey) == null)
		{
			SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
		}
		if (!_instantiatedEventPages.TryGetValue(_currentTemplateKey, out var value))
		{
			EventPageScaffolding eventPageScaffolding = AssetLoader.Instantiate<EventPageScaffolding>(_sharedClasses.AssetLookupSystem.GetPrefabPath<EventScaffoldingPrefab, EventPageScaffolding>(), base.transform);
			eventPageScaffolding.gameObject.name = _currentTemplateKey;
			eventPageScaffolding.SetBackgroundImage(ClientEventDefinitionList.GetBackgroundImagePath(_sharedClasses.AssetLookupSystem, _currentEventContext));
			EventComponentManager componentManager = new EventComponentManager(_sharedClasses, _currentEventContext);
			_factory.CreateComponents(componentManager, eventPageScaffolding.LayoutGroups, eventPageScaffolding.safeArea);
			value = new EventPage(componentManager, eventPageScaffolding);
			_instantiatedEventPages[_currentTemplateKey] = value;
		}
		value.ComponentManager.UpdateComponents();
		_currentEventPage = value;
		_currentEventPage?.EventScaffolding.SetActive(active: true);
		StartCoroutine(Coroutine_ShowTemplate());
	}

	public override void OnBeginClose()
	{
		_currentEventPage?.EventScaffolding.SetActive(active: false);
		_currentEventPage?.ComponentManager.OnEventPageClosed();
		_currentEventPage = null;
	}

	public virtual void SetEvent(EventContext eventContext)
	{
		_currentEventContext = new EventContext(eventContext);
	}

	private IEnumerator Coroutine_ShowTemplate()
	{
		_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: true);
		_readyToshow = false;
		Promise<ICourseInfoWrapper> getCourse = _currentEventContext.PlayerEvent.GetEventCourse();
		yield return getCourse.AsCoroutine();
		_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: false);
		_readyToshow = true;
		if (!getCourse.Successful)
		{
			if (getCourse.ErrorSource != ErrorSource.Debounce)
			{
				SceneLoader.GetSceneLoader().ShowConnectionFailedMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Progress_Get_Error_Text"), allowRetry: true, exitInsteadOfLogout: true);
			}
		}
		else
		{
			_currentEventPage?.ComponentManager.OnEventPageOpen(_currentEventContext);
		}
	}

	public void ClearCachedEventPages()
	{
		_instantiatedEventPages.Clear();
	}
}
