using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Epic.OnlineServices;

public static class Helper
{
	private class AllocationInfo
	{
		public Type Type { get; private set; }

		public object Memory { get; private set; }

		public int? ArrayLength { get; private set; }

		public AllocationInfo(Type type, object memory, int? arrayLength = null)
		{
			Type = type;
			Memory = memory;
			ArrayLength = arrayLength;
		}
	}

	private class DelegateHolder
	{
		public Delegate Public { get; private set; }

		public Delegate Private { get; private set; }

		public DelegateHolder(Delegate publicDelegate, Delegate privateDelegate)
		{
			Public = publicDelegate;
			Private = privateDelegate;
		}
	}

	private static Dictionary<IntPtr, AllocationInfo> s_AllocationRegistry = new Dictionary<IntPtr, AllocationInfo>();

	private static Dictionary<IntPtr, DelegateHolder> s_CallDelegates = new Dictionary<IntPtr, DelegateHolder>();

	private static List<Type> s_PointerTypes = new List<Type> { typeof(string) };

	public static int GetAllocationCount()
	{
		return s_AllocationRegistry.Count;
	}

	internal static void RegisterAllocation<T>(ref IntPtr address, T memory)
	{
		ReleaseAllocation(ref address);
		if (address != IntPtr.Zero)
		{
			throw new Exception("Attempting to allocate over memory that is already externally allocated");
		}
		if (typeof(T).IsArray)
		{
			Type elementType = typeof(T).GetElementType();
			Array array = memory as Array;
			int num = 0;
			num = ((!s_PointerTypes.Contains(elementType)) ? Marshal.SizeOf(elementType) : Marshal.SizeOf(typeof(IntPtr)));
			address = Marshal.AllocHGlobal(array.Length * num);
			s_AllocationRegistry.Add(address, new AllocationInfo(typeof(T), memory, array.Length));
			for (int i = 0; i < array.Length; i++)
			{
				object value = array.GetValue(i);
				if (s_PointerTypes.Contains(elementType))
				{
					IntPtr address2 = IntPtr.Zero;
					RegisterAllocation(ref address2, value);
					Marshal.WriteIntPtr(address, i * num, address2);
				}
				else
				{
					Marshal.StructureToPtr(value, IntPtr.Add(address, i * num), fDeleteOld: false);
				}
			}
		}
		else
		{
			if (memory is string)
			{
				string s = (string)(object)memory;
				byte[] bytes = Encoding.UTF8.GetBytes(s);
				address = Marshal.AllocHGlobal(bytes.Length + 1);
				Marshal.Copy(bytes, 0, address, bytes.Length);
				Marshal.WriteByte(address, bytes.Length, 0);
			}
			else if (memory != null)
			{
				address = Marshal.AllocHGlobal(Marshal.SizeOf(memory.GetType()));
				Marshal.StructureToPtr(memory, address, fDeleteOld: false);
			}
			if (memory != null && !s_AllocationRegistry.ContainsKey(address))
			{
				s_AllocationRegistry.Add(address, new AllocationInfo(memory.GetType(), memory));
			}
		}
	}

	internal static void ReleaseAllocation(ref IntPtr address)
	{
		if (address == IntPtr.Zero || !s_AllocationRegistry.TryGetValue(address, out var value))
		{
			return;
		}
		if (value.Type.IsArray)
		{
			Type elementType = value.Type.GetElementType();
			int num = 0;
			num = ((!s_PointerTypes.Contains(elementType)) ? Marshal.SizeOf(elementType) : Marshal.SizeOf(typeof(IntPtr)));
			for (int i = 0; i < value.ArrayLength; i++)
			{
				IntPtr address2 = IntPtr.Add(address, i * num);
				if (s_PointerTypes.Contains(elementType))
				{
					address2 = Marshal.ReadIntPtr(address2);
				}
				if (GetAllocation(address2, elementType) is IDisposable disposable)
				{
					disposable.Dispose();
				}
				if (s_PointerTypes.Contains(elementType))
				{
					ReleaseAllocation(ref address2);
				}
			}
		}
		if (value.Type.IsValueType && GetAllocation(address, value.Type) is IDisposable disposable2)
		{
			disposable2.Dispose();
		}
		Marshal.FreeHGlobal(address);
		s_AllocationRegistry.Remove(address);
		address = IntPtr.Zero;
	}

	internal static T GetAllocation<T>(IntPtr startAddress, int? arrayLength = null)
	{
		if (startAddress == IntPtr.Zero)
		{
			return default(T);
		}
		if (s_AllocationRegistry.ContainsKey(startAddress) && s_AllocationRegistry[startAddress].Type == typeof(T) && s_AllocationRegistry[startAddress].ArrayLength == arrayLength)
		{
			return (T)s_AllocationRegistry[startAddress].Memory;
		}
		if (typeof(T).IsArray)
		{
			Type elementType = typeof(T).GetElementType();
			Array array = Array.CreateInstance(elementType, arrayLength.Value);
			int num = 0;
			num = ((!s_PointerTypes.Contains(elementType)) ? Marshal.SizeOf(elementType) : Marshal.SizeOf(typeof(IntPtr)));
			for (int i = 0; i < arrayLength.Value; i++)
			{
				IntPtr intPtr = IntPtr.Add(startAddress, i * num);
				if (s_PointerTypes.Contains(elementType))
				{
					intPtr = Marshal.ReadIntPtr(intPtr);
				}
				object allocation = GetAllocation(intPtr, elementType);
				array.SetValue(allocation, i);
			}
			return (T)(object)array;
		}
		return (T)GetAllocation(startAddress, typeof(T));
	}

	private static object GetAllocation(IntPtr address, Type type)
	{
		if (type == typeof(string))
		{
			return GetAllocatedString(address);
		}
		return Marshal.PtrToStructure(address, type);
	}

	private static string GetAllocatedString(IntPtr address)
	{
		if (address == IntPtr.Zero)
		{
			return null;
		}
		int i;
		for (i = 0; Marshal.ReadByte(address, i) != 0; i++)
		{
		}
		byte[] array = new byte[i];
		Marshal.Copy(address, array, 0, i);
		return Encoding.UTF8.GetString(array);
	}

	internal static void RegisterCall(ref IntPtr clientDataAddress, BoxedClientData boxedClientData, Delegate publicDelegate, Delegate privateDelegate)
	{
		RegisterAllocation(ref clientDataAddress, boxedClientData);
		s_CallDelegates.Add(clientDataAddress, new DelegateHolder(publicDelegate, privateDelegate));
	}

	private static bool CanRemoveCallDelegate(object delegateInfo)
	{
		PropertyInfo propertyInfo = (from property in delegateInfo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
			where property.PropertyType == typeof(Result)
			select property).FirstOrDefault();
		if (propertyInfo != null)
		{
			Result result = (Result)propertyInfo.GetValue(delegateInfo);
			if (result == Result.OperationWillRetry || result == Result.AuthPinGrantCode)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	internal static Delegate GetAndTryRemoveCallDelegate(IntPtr clientDataAddress, object delegateInfo)
	{
		Delegate result = s_CallDelegates[clientDataAddress].Public;
		if (CanRemoveCallDelegate(delegateInfo))
		{
			s_CallDelegates.Remove(clientDataAddress);
		}
		return result;
	}

	internal static string StringFromByteArray(byte[] bytes)
	{
		return Encoding.UTF8.GetString(bytes);
	}

	internal static byte[] StringToByteArray(string str, int sizeConst)
	{
		if (str.Length >= sizeConst - 1)
		{
			return Encoding.UTF8.GetBytes(new string(str.Take(sizeConst - 1).ToArray()));
		}
		return Encoding.UTF8.GetBytes(str.PadRight(sizeConst, '\0'));
	}

	internal static T GetDefault<T>()
	{
		return default(T);
	}

	internal static void CopyProperties(object source, object target)
	{
		if (source == null || target == null)
		{
			return;
		}
		PropertyInfo[] properties = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
		PropertyInfo[] properties2 = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
		PropertyInfo[] array = properties;
		foreach (PropertyInfo sourceProperty in array)
		{
			PropertyInfo propertyInfo = properties2.SingleOrDefault((PropertyInfo property) => property.Name == sourceProperty.Name);
			if (propertyInfo == null || propertyInfo.SetMethod == null)
			{
				continue;
			}
			if (sourceProperty.PropertyType == propertyInfo.PropertyType)
			{
				propertyInfo.SetValue(target, sourceProperty.GetValue(source));
			}
			else if (propertyInfo.PropertyType.IsArray)
			{
				if (sourceProperty.GetValue(source) is Array array2)
				{
					Array array3 = Array.CreateInstance(propertyInfo.PropertyType.GetElementType(), array2.Length);
					for (int num = 0; num < array2.Length; num++)
					{
						object value = array2.GetValue(num);
						object obj = Activator.CreateInstance(propertyInfo.PropertyType.GetElementType());
						CopyProperties(value, obj);
						array3.SetValue(obj, num);
					}
					propertyInfo.SetValue(target, array3);
				}
				else
				{
					propertyInfo.SetValue(target, null);
				}
			}
			else
			{
				object obj2 = Activator.CreateInstance(propertyInfo.PropertyType);
				CopyProperties(sourceProperty.GetValue(source), obj2);
				propertyInfo.SetValue(target, obj2);
			}
		}
	}
}
