using System;
using System.Collections.Generic;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models.Event;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Events;

public class CourseData
{
	public Guid Id;

	public PlayerEventState CurrentEventState;

	public PlayerEventModule CurrentModule;

	public List<uint> CardPool;

	public List<CollationCardPool> CardPoolByCollation;

	public List<string> CardStyles;

	public Client_Deck CourseDeck;

	public string DraftId;

	public string MadeChoice;

	public CourseData()
	{
	}

	public CourseData(ICourseInfoWrapper courseInfoWrapper)
	{
		Update(courseInfoWrapper);
	}

	public CourseData(PlayerCourseInfoV2 course)
	{
		Update(course);
	}

	public CourseData(PlayerCourseInfo course)
	{
		Update(course);
	}

	public void Update(ICourseInfoWrapper courseInfoWrapper)
	{
		Id = courseInfoWrapper.Id;
		CurrentEventState = courseInfoWrapper.CurrentEventState;
		CurrentModule = courseInfoWrapper.CurrentEventModule;
		CardPool = courseInfoWrapper.CardPool;
		CardPoolByCollation = courseInfoWrapper.CardPoolByCollation;
		CardStyles = courseInfoWrapper.CardStyles;
		CourseDeck = courseInfoWrapper.CourseDeck;
		DraftId = courseInfoWrapper.HumanDraftId;
		MadeChoice = courseInfoWrapper.MadeChoice;
	}

	public void Update(PlayerCourseInfoV2 course)
	{
		Id = course.Id;
		CurrentEventState = course.CurrentEventState;
		CurrentModule = PlayerEventModule.None;
		CardPool = course.CardPool;
		CardPoolByCollation = course.CardPoolByCollation;
		CardStyles = course.CardStyles;
		MadeChoice = course.MadeChoice;
		Enum.TryParse<PlayerEventModule>(course.CurrentModule, ignoreCase: true, out CurrentModule);
		if (course.CourseDeck == null)
		{
			CourseDeck = new Client_Deck();
		}
		else
		{
			CourseDeck = DeckServiceWrapperHelpers.ToClientModel(course.CourseDeck.ToDeckInfo());
		}
	}

	public void Update(PlayerCourseInfo course)
	{
		Id = course.Id;
		CurrentEventState = course.CurrentEventState;
		CurrentModule = PlayerEventModule.None;
		CardPool = course.CardPool;
		CardPoolByCollation = course.CardPoolByCollation;
		CardStyles = course.CardStyles;
		MadeChoice = course.MadeChoice;
		Enum.TryParse<PlayerEventModule>(course.CurrentModule, ignoreCase: true, out CurrentModule);
		if (course.CourseDeck == null)
		{
			CourseDeck = new Client_Deck();
		}
		else
		{
			CourseDeck = DeckServiceWrapperHelpers.ToClientModel(course.CourseDeck.ToDeckInfo());
		}
	}

	public void Update(Client_Deck deckInfo)
	{
		CourseDeck = deckInfo;
		CurrentModule = ((CourseDeck == null) ? PlayerEventModule.DeckSelect : PlayerEventModule.None);
	}
}
