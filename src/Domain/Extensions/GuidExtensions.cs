using System.Buffers.Text;
using System.Runtime.InteropServices;
using static SiteWatcher.Domain.Utils.Utils;

namespace SiteWatcher.Domain.Extensions;

public static class GuidExtensions
{
    public static string ToBase64String(this Guid id)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        MemoryMarshal.Write(guidBytes, ref id);

        Span<byte> guidBytesBase64 = stackalloc byte[24];
        Base64.EncodeToUtf8(guidBytes, guidBytesBase64, out _, out _);

        return ConvertBase64BytesToString(guidBytesBase64);
    }
}