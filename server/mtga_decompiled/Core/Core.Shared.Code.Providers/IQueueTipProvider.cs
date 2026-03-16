using System.Collections.Generic;
using Wizards.Arena.Enums.QueueTips;
using Wizards.Arena.Models;

namespace Core.Shared.Code.Providers;

public interface IQueueTipProvider
{
	List<QueueTip> GetTipsInGroup(QueueTipGroup group);

	void SetData(List<QueueTip> queueTips);
}
