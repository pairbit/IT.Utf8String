#if NETSTANDARD2_0 || NETSTANDARD2_1
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

internal static class _HashCode
{
    public static void AddBytes(this ref HashCode hashCode, ReadOnlySpan<byte> value)
    {
        ref byte pos = ref MemoryMarshal.GetReference(value);
        ref byte end = ref Unsafe.Add(ref pos, value.Length);

        // Add four bytes at a time until the input has fewer than four bytes remaining.
        while ((nint)Unsafe.ByteOffset(ref pos, ref end) >= sizeof(int))
        {
            hashCode.Add(Unsafe.ReadUnaligned<int>(ref pos));
            pos = ref Unsafe.Add(ref pos, sizeof(int));
        }

        // Add the remaining bytes a single byte at a time.
        while (Unsafe.IsAddressLessThan(ref pos, ref end))
        {
            hashCode.Add((int)pos);
            pos = ref Unsafe.Add(ref pos, 1);
        }
    }
}
#endif