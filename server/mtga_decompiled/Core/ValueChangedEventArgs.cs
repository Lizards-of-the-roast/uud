using System;

public class ValueChangedEventArgs : EventArgs
{
	public int OldValue { get; set; }

	public int NewValue { get; set; }

	public uint AssociatedInstanceId { get; set; }
}
