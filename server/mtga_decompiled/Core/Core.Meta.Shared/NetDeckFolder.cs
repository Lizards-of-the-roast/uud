using System;

namespace Core.Meta.Shared;

public class NetDeckFolder
{
	public Guid Id { get; set; }

	public string FolderNameLocKey { get; set; }

	public string FolderDescLocKey { get; set; }

	public DateTime? StartDate { get; set; }

	public DateTime? EndDate { get; set; }
}
