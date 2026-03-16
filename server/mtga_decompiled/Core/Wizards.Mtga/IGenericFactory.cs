namespace Wizards.Mtga;

public interface IGenericFactory<out T>
{
	T Create();
}
public interface IGenericFactory<in TParam1, out T>
{
	T Create(TParam1 param1);
}
public interface IGenericFactory<in TParam1, in TParam2, out T>
{
	T Create(TParam1 param1, TParam2 param2);
}
