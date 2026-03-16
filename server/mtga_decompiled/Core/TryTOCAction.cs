internal class TryTOCAction : ScriptedAction
{
	public uint TOC_GRPID { get; private set; }

	public TryTOCAction(uint grpid)
	{
		TOC_GRPID = grpid;
	}
}
