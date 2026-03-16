using System.Collections.Generic;
using System.Linq;
using EventPage.Components.NetworkModels;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Wrapper;
using Wotc.Mtga.Wrapper.Draft;

namespace Wotc.Mtga.Events;

public class LimitedPlayerEvent : BasicPlayerEvent
{
	private class DefaultFactory : IGenericFactory<EventInfoV3, ICourseInfoWrapper, LimitedPlayerEvent>
	{
		public LimitedPlayerEvent Create(EventInfoV3 eventInfo, ICourseInfoWrapper course)
		{
			return new LimitedPlayerEvent(eventInfo, course, Pantry.Get<IEventsServiceWrapper>(), Pantry.Get<DeckDataProvider>());
		}
	}

	public new static readonly IGenericFactory<EventInfoV3, ICourseInfoWrapper, LimitedPlayerEvent> CurrentFactory = new DefaultFactory();

	public override string DefaultTemplateName => "LimitedEventTemplate";

	public IDraftPod DraftPod { get; private set; }

	public LimitedPlayerEvent(EventInfoV3 eventInfo, ICourseInfoWrapper courseInfo, IEventsServiceWrapper eventsServiceWrapper, DeckDataProvider deckDataProvider)
		: base(eventInfo, courseInfo, eventsServiceWrapper, deckDataProvider)
	{
	}

	protected override void UpdateSideboardForFormat(Client_Deck deck)
	{
	}

	public Promise<ICourseInfoWrapper> CreateBotDraft(string displayName, string localSeatAvatar)
	{
		BotDraftPod botDraftPod = new BotDraftPod(_eventsServiceWrapper);
		return _eventsServiceWrapper.CreateBotDraft(base.EventInfo.InternalEventName).IfSuccess(delegate(Promise<ICourseInfoWrapper> p)
		{
			ICourseInfoWrapper result = p.Result;
			if (result != null)
			{
				_updatePlayerCourse(result);
			}
			string draftId = _courseInfo.DraftId;
			List<uint> list = base.EventUXInfo.EventComponentData?.BoosterPacksDisplay?.CollationIds;
			CollationMapping[] boosterCollation = ((list != null) ? list.Select((uint c) => (CollationMapping)c).ToArray() : new CollationMapping[3]);
			botDraftPod.SetDraftState(_courseInfo.InternalEventName, draftId, boosterCollation, displayName, localSeatAvatar);
			DraftPod = botDraftPod;
		});
	}

	public Promise<Unit> CreateHumanDraft(bool rejoinEvent, ILogger logger, IBILogger biLogger)
	{
		HumanDraftPod humanDraftPod = new HumanDraftPod(_eventsServiceWrapper, logger, biLogger, _courseInfo.InternalEventName, _courseInfo.HumanDraftId);
		if (!rejoinEvent)
		{
			DraftPod = humanDraftPod;
			return new SimplePromise<Unit>(default(Unit));
		}
		return _eventsServiceWrapper.TryRejoinHumanDraft(base.EventInfo.InternalEventName).Then((Promise<ICourseInfoWrapper> p) => OnRejoinHumanDraft(this, humanDraftPod, p)).Convert((ICourseInfoWrapper _) => default(Unit));
	}

	public static Promise<ICourseInfoWrapper> OnRejoinHumanDraft(LimitedPlayerEvent draftEvent, HumanDraftPod humanDraftPod, Promise<ICourseInfoWrapper> previousPromise)
	{
		if (previousPromise.Successful)
		{
			draftEvent._updatePlayerCourse(previousPromise.Result);
			if (!string.IsNullOrEmpty(previousPromise.Result.HumanDraftId))
			{
				draftEvent.DraftPod = humanDraftPod;
			}
		}
		else if (previousPromise.Error.Code == 5035)
		{
			return draftEvent.CompleteDraft();
		}
		return previousPromise;
	}

	public Promise<ICourseInfoWrapper> CompleteDraft()
	{
		return _eventsServiceWrapper.CompleteDraft(base.EventInfo.InternalEventName, base.CourseData.CurrentModule == PlayerEventModule.Draft).IfSuccess(delegate(Promise<ICourseInfoWrapper> p)
		{
			_updatePlayerCourse(p.Result);
		});
	}

	public Promise<ICourseInfoWrapper> ReadyDraft(string draftId)
	{
		return _eventsServiceWrapper.ReadyDraft(base.EventInfo.InternalEventName, draftId).IfSuccess(delegate(Promise<ICourseInfoWrapper> p)
		{
			_updatePlayerCourse(p.Result);
		});
	}

	protected override void onResign()
	{
		base.onResign();
		DraftPod = null;
	}

	protected override void onDrop()
	{
		base.onDrop();
		DraftPod = null;
	}
}
