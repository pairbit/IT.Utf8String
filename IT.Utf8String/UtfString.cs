using System;
using System.Diagnostics;
using System.Text;

namespace IT;

[DebuggerDisplay("{ToString()}")]
internal readonly struct UtfString
{
    private readonly string _utf16;
    private readonly Utf8String _utf8;

    public string Utf16 => _utf16 ?? string.Empty;

    public Utf8String Utf8 => _utf8;

    public bool IsEmpty => _utf16 == null || _utf8.Array == null;

    public UtfString(string utf16)
    {
        _utf16 = utf16 ?? throw new ArgumentNullException(nameof(utf16));
        _utf8 = utf16.Length == 0 ? [] : Encoding.UTF8.GetBytes(utf16);
    }

    public UtfString(ReadOnlySpan<byte> utf8)
    {
        _utf16 = utf8.Length == 0 ? string.Empty : Encoding.UTF8.GetString(utf8);
        _utf8 = utf8.ToArray();
    }

    public override string ToString() => Utf16;
}