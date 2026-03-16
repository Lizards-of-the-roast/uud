using System.Collections.Generic;

public interface IFDHistory
{
	bool NewestFirst { get; set; }

	List<HistoryEntry> HistoryEntries { get; }

	bool DoUpdate(string connectionCategory);

	void Clear();

	void ClearHistory();

	bool HasConnection();

	bool HasPreviousMatchHistoryConnection();
}
