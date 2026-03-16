namespace Test.Scenes.NetworkMessagingHarness;

public interface IByteSizer<T>
{
	int SizeInBytes(T value);
}
