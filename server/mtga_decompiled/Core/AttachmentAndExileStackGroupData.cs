using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

public class AttachmentAndExileStackGroupData
{
	public enum GroupType
	{
		Exile,
		Attachment
	}

	public DuelScene_CDC Parent { get; private set; }

	public GroupType Type { get; private set; }

	public List<AttachmentAndExileStackGroupData> Children { get; private set; }

	public BrowserCardHeader.BrowserCardHeaderData HeaderData { get; set; }

	public List<DuelScene_CDC> Cards { get; private set; } = new List<DuelScene_CDC>();

	public static List<AttachmentAndExileStackGroupData> GenerateGroups(MtgCardInstance instance, MtgGameState gameState, ICardViewProvider viewManager, bool setParent = true)
	{
		List<AttachmentAndExileStackGroupData> list = new List<AttachmentAndExileStackGroupData>();
		if (instance != null && instance.AttachedWithIds.Count != 0)
		{
			DuelScene_CDC cardView = viewManager.GetCardView(instance.InstanceId);
			foreach (uint attachedWithId in instance.AttachedWithIds)
			{
				if (gameState.VisibleCards.TryGetValue(attachedWithId, out var value))
				{
					AttachmentAndExileStackGroupData attachmentAndExileStackGroupData = new AttachmentAndExileStackGroupData();
					attachmentAndExileStackGroupData.Parent = (setParent ? cardView : null);
					if (viewManager.TryGetCardView(attachedWithId, out var cardView2))
					{
						attachmentAndExileStackGroupData.Cards.Add(cardView2);
						MtgZone mtgZone = value?.Zone;
						bool flag = mtgZone != null && mtgZone.Type == ZoneType.Exile;
						attachmentAndExileStackGroupData.Type = ((!flag) ? GroupType.Attachment : GroupType.Exile);
						attachmentAndExileStackGroupData.Children = GenerateGroups(value, gameState, viewManager);
						list.Add(attachmentAndExileStackGroupData);
					}
				}
			}
			CombineAndSortGroups(list);
		}
		return list;
	}

	private static void CombineAndSortGroups(List<AttachmentAndExileStackGroupData> groups)
	{
		Dictionary<GroupType, AttachmentAndExileStackGroupData> noAttachmentGroupsByType = new Dictionary<GroupType, AttachmentAndExileStackGroupData>();
		for (int num = groups.Count - 1; num >= 0; num--)
		{
			AttachmentAndExileStackGroupData attachmentAndExileStackGroupData = groups[num];
			if (attachmentAndExileStackGroupData.Children == null || attachmentAndExileStackGroupData.Children.Count <= 0)
			{
				groups.RemoveAt(num);
				AttachmentAndExileStackGroupData value = null;
				if (noAttachmentGroupsByType.TryGetValue(attachmentAndExileStackGroupData.Type, out value))
				{
					value.Cards.AddRange(attachmentAndExileStackGroupData.Cards);
				}
				else
				{
					noAttachmentGroupsByType.Add(attachmentAndExileStackGroupData.Type, attachmentAndExileStackGroupData);
				}
			}
		}
		groups.Sort(CompareGroups);
		AddToGroupsIfNoAttachmentGroupExists(GroupType.Attachment);
		AddToGroupsIfNoAttachmentGroupExists(GroupType.Exile);
		void AddToGroupsIfNoAttachmentGroupExists(GroupType type)
		{
			AttachmentAndExileStackGroupData value2 = null;
			if (noAttachmentGroupsByType.TryGetValue(type, out value2))
			{
				value2.Cards.Sort((DuelScene_CDC x, DuelScene_CDC y) => y.InstanceId.CompareTo(x.InstanceId));
				bool flag = false;
				foreach (AttachmentAndExileStackGroupData group in groups)
				{
					if (group.Type == type)
					{
						flag = true;
						value2.Cards.AddRange(group.Cards);
						group.Cards = value2.Cards;
						break;
					}
				}
				if (!flag)
				{
					groups.Insert(0, value2);
				}
			}
		}
		static int CompareGroups(AttachmentAndExileStackGroupData x, AttachmentAndExileStackGroupData y)
		{
			int num2 = x.Type.CompareTo(y.Type);
			if (num2 != 0)
			{
				return num2;
			}
			return y.Cards[y.Cards.Count - 1].InstanceId.CompareTo(x.Cards[x.Cards.Count - 1].InstanceId);
		}
	}
}
