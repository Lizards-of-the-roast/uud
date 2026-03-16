using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Meta.LearnMore;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.LearnMore;

public class SectionObjectReferences
{
	public readonly LearnMoreSection SectionInfo;

	public readonly SectionObjectReferences[] Ancestors;

	public int Index;

	public readonly List<SectionObjectReferences> Children;

	public TableOfContentsSection TableOfContentsSection;

	public LearnToPlayContents ContentEntry;

	public readonly bool IsSelfAccessible;

	public bool IsSelfRead;

	public bool Show;

	private bool _showNewFlag;

	public string Id => SectionInfo.Id;

	public bool HasChildren => Children.Count > 0;

	public bool HasParent => Ancestors.Length != 0;

	public SectionObjectReferences Parent
	{
		get
		{
			if (!HasParent)
			{
				return null;
			}
			return Ancestors[Ancestors.Length - 1];
		}
	}

	public SectionObjectReferences Progenitor
	{
		get
		{
			if (!HasParent)
			{
				return this;
			}
			return Ancestors[0];
		}
	}

	public bool ShowNewFlag
	{
		get
		{
			return _showNewFlag;
		}
		set
		{
			bool num = TableOfContentsSection != null;
			_showNewFlag = value;
			if (num)
			{
				TableOfContentsSection.ShowNewFlag = value;
			}
		}
	}

	public string Path => ToDebugPath(AncestorsThenSelf.ToArray());

	public string[] PathTitles => ToPathTitles(AncestorsThenSelf.ToArray());

	private string AttemptedLocalizedTitle => AttemptedLocalizedSectionTitle(SectionInfo);

	private IEnumerable<SectionObjectReferences> AncestorsThenSelf => Ancestors.Append(this);

	public SectionObjectReferences(LearnMoreSection sectionInfo, SectionObjectReferences[] ancestors, bool isSelfAccessible, bool isSelfRead)
	{
		SectionInfo = sectionInfo;
		Ancestors = ancestors ?? Array.Empty<SectionObjectReferences>();
		Children = new List<SectionObjectReferences>();
		ContentEntry = null;
		IsSelfAccessible = isSelfAccessible;
		IsSelfRead = isSelfRead;
	}

	public void InitializeShowAndNew(bool showChild, bool newChild, string selfAccessibleReason)
	{
		string arg2;
		string arg;
		if (HasChildren)
		{
			Show = showChild;
			arg = (showChild ? "children: visible" : "children: none visible");
			ShowNewFlag = newChild;
			arg2 = (newChild ? "children: new" : "children: none new");
		}
		else
		{
			Show = IsSelfAccessible;
			arg = (IsSelfAccessible ? "self: visible" : "self: locked");
			arg = arg + " (" + selfAccessibleReason + ")";
			if (Show)
			{
				ShowNewFlag = !IsSelfRead;
				arg2 = (IsSelfRead ? "self: read" : "self: unread");
			}
			else
			{
				ShowNewFlag = false;
				arg2 = "self: not visible";
			}
		}
		Debug.Log($"LTP: {Path}: Show: {Show} - {arg}");
		Debug.Log($"LTP: {Path}: New: {ShowNewFlag} - {arg2}");
	}

	public override string ToString()
	{
		return SectionInfo.Title;
	}

	public static string ToDebugPath(SectionObjectReferences[] refs)
	{
		return string.Join(">", ToPathTitles(refs));
	}

	private static string[] ToPathTitles(SectionObjectReferences[] refs)
	{
		string[] array = new string[refs.Length];
		for (int i = 0; i < refs.Length; i++)
		{
			array[i] = refs[i].AttemptedLocalizedTitle;
		}
		return array;
	}

	public static string AttemptedLocalizedSectionTitle(LearnMoreSection section)
	{
		return Languages.ActiveLocProvider?.GetLocalizedText(section.Title) ?? section.Title;
	}
}
