using System.ComponentModel;

namespace Wizards.Mtga.Credits;

public class CreditSectionData
{
	public string HeadingLocKey { get; set; }

	[DefaultValue(52)]
	public int HeaderTextSize { get; set; }

	public CreditRoleData[] Roles { get; set; }

	public string[] Text { get; set; }

	public string[] BulletedText { get; set; }
}
