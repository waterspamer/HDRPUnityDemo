using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Nettle {

public static class SerializationUtils {

    public static byte[] StructToByteArray<T>(T str) where T : struct {
        int size = Marshal.SizeOf(typeof(T));
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    public static T StructFromByteArray<T>(byte[] arr, int offset = 0) where T : struct {
        T str = new T();
        int size = Marshal.SizeOf(typeof(T));
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(arr, offset, ptr, size);
        str = (T)Marshal.PtrToStructure(ptr, str.GetType());
        Marshal.FreeHGlobal(ptr);
        return str;
    }

    public static byte[] ToByteArray<T>(T[] source) where T : struct {
        List<byte> result = new List<byte>(source.Length * Marshal.SizeOf(typeof(T)));
        for (int i = 0; i < source.Length; ++i) {
            result.AddRange(StructToByteArray(source[i]));
        }
        return result.ToArray();
    }

    public static T[] FromByteArray<T>(byte[] source) where T : struct {
        int elementSize = Marshal.SizeOf(typeof(T));
        T[] result = new T[source.Length / Marshal.SizeOf(typeof(T))];

        for (int i = 0; i < result.Length; ++i) {
            result[i] = StructFromByteArray<T>(source, i * elementSize);
        }
        return result;
    }

    public static byte[] Serializer(this object _object) {
        byte[] bytes;
        using (var _MemoryStream = new MemoryStream()) {
            IFormatter _BinaryFormatter = new BinaryFormatter();
            _BinaryFormatter.Serialize(_MemoryStream, _object);
            bytes = _MemoryStream.ToArray();
        }
        return bytes;
    }

    public static T Deserializer<T>(this byte[] _byteArray) {
        T ReturnValue;
        using (var _MemoryStream = new MemoryStream(_byteArray)) {
            IFormatter _BinaryFormatter = new BinaryFormatter();
            ReturnValue = (T)_BinaryFormatter.Deserialize(_MemoryStream);
        }
        return ReturnValue;
    }

}
}
