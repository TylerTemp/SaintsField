using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe.NotBurstCompatible;
using Unity.Serialization.Binary;
using Unity.Serialization.Json;

// ReSharper disable once CheckNamespace
namespace SaintsField.Events
{
    public static class SerializationUtil
    {
        // public static unsafe Byte[] ToBinary<T>(T obj, IReadOnlyList<IBinaryAdapter> adapters = null)
        // {
        //     var buffer = new UnsafeAppendBuffer(16, 8, Allocator.Temp);
        //     var parameters = new BinarySerializationParameters { UserDefinedAdapters = adapters?.ToList() };
        //     BinarySerialization.ToBinary(&buffer, obj, parameters);
        //
        //     var bytes = buffer.ToBytesNBC();
        //     buffer.Dispose();
        //
        //     return bytes;
        // }
        //
        // public static unsafe T FromBinary<T>(Byte[] serializedBytes, IReadOnlyList<IBinaryAdapter> adapters = null)
        // {
        //     fixed (Byte* ptr = serializedBytes)
        //     {
        //         var bufferReader = new UnsafeAppendBuffer.Reader(ptr, serializedBytes.Length);
        //         var parameters = new BinarySerializationParameters { UserDefinedAdapters = adapters?.ToList() };
        //         return BinarySerialization.FromBinary<T>(&bufferReader, parameters);
        //     }
        // }

        public static unsafe object FromBinaryType(Type type, Byte[] serializedBytes, IReadOnlyList<IBinaryAdapter> adapters = null)
        {
            fixed (Byte* ptr = serializedBytes)
            {
                UnsafeAppendBuffer.Reader bufferReader = new UnsafeAppendBuffer.Reader(ptr, serializedBytes.Length);
                BinarySerializationParameters parameters = new BinarySerializationParameters { UserDefinedAdapters = adapters?.ToList() };

                MethodInfo method = typeof(BinarySerialization)
                    .GetMethod("FromBinary", BindingFlags.Public | BindingFlags.Static);
                MethodInfo generic = method!.MakeGenericMethod(type);

                DynamicMethod dynamicMethod = new DynamicMethod(
                    "CallFromBinary",
                    typeof(object),
                    new[] { typeof(IntPtr), typeof(BinarySerializationParameters) },
                    typeof(BinarySerialization).Module,
                    skipVisibility: true);

                ILGenerator il = dynamicMethod.GetILGenerator();

                // Load pointer argument
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Conv_U); // Convert IntPtr to native int (void*)

                // Load parameters argument
                il.Emit(OpCodes.Ldarg_1);

                // Call the generic method
                il.EmitCall(OpCodes.Call, generic, null);

                // Box the result if value type
                if (type.IsValueType)
                    il.Emit(OpCodes.Box, type);

                il.Emit(OpCodes.Ret);

                Func<IntPtr, BinarySerializationParameters, object> del = (Func<IntPtr, BinarySerializationParameters, object>)dynamicMethod
                    .CreateDelegate(typeof(Func<IntPtr, BinarySerializationParameters, object>));
                return del((IntPtr)(&bufferReader), parameters);
            }
        }

        public static unsafe byte[] ToBinaryType(object obj, IReadOnlyList<IBinaryAdapter> adapters = null)
        {
            UnsafeAppendBuffer buffer = new UnsafeAppendBuffer(16, 8, Allocator.Temp);
            try
            {
                BinarySerializationParameters parameters = new BinarySerializationParameters
                    { UserDefinedAdapters = adapters?.ToList() };
                BinarySerialization.ToBinary(&buffer, obj, parameters);

                byte[] bytes = buffer.ToBytesNBC();
                buffer.Dispose();
                return bytes;
            }
            catch(Exception)
            {
                buffer.Dispose();
                throw;
            }
        }

        public static object FromJsonType(Type type, string jsonString, JsonSerializationParameters adapters = default)
        {
            return typeof(JsonSerialization)
                .GetMethod("FromJson", new[] { typeof(string), typeof(JsonSerializationParameters) })!
                .MakeGenericMethod(type)
                .Invoke(null, new object[] { jsonString, adapters });
        }


        public static string ToJsonType(object obj, JsonSerializationParameters parameters = default)
        {
            return JsonSerialization.ToJson(obj, parameters);
        }
    }
}
