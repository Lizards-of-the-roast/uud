using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Enums.QueueTips;
using Wizards.Arena.Models;

namespace Core.Shared.Code.Providers;

public class QueueTipProvider : IQueueTipProvider
{
	private List<QueueTip> _queueTips = new List<QueueTip>();

	public static QueueTipProvider Create()
	{
		return new QueueTipProvider();
	}

	public List<QueueTip> GetTipsInGroup(QueueTipGroup group)
	{
		return _queueTips.Where((QueueTip q) => q.Group.HasFlag(group)).ToList();
	}

	public void SetData(List<QueueTip> queueTips)
	{
		_queueTips = queueTips;
	}
}
