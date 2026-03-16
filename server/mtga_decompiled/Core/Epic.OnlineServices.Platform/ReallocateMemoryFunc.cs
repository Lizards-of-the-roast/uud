using System;

namespace Epic.OnlineServices.Platform;

public delegate IntPtr ReallocateMemoryFunc(IntPtr pointer, int sizeInBytes, int alignment);
