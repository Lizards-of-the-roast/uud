using System.Collections.Generic;

public interface IGroupedCardProvider
{
	List<List<DuelScene_CDC>> GetCardGroups();
}
