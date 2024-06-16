using System.Security.Cryptography;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Primitives;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Benchmark.Benchs;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
public class GenerateStateFromRequest
{
    private readonly StringValues _userAgent = new("user-agent-mobile");
    private readonly StringValues _acceptLang = new("language-pt");

    [Benchmark]
    public string OldWayWithReplace()
    {
        var agentBytes = JsonSerializer.SerializeToUtf8Bytes(_userAgent);
        var languageBytes = JsonSerializer.SerializeToUtf8Bytes(_acceptLang);
        var timeBytes = JsonSerializer.SerializeToUtf8Bytes(DateTime.UtcNow);

        var length = agentBytes.Length + languageBytes.Length + timeBytes.Length;

        var allBytes = new byte[length];
        Buffer.BlockCopy(agentBytes, 0, allBytes, 0, agentBytes.Length);
        Buffer.BlockCopy(languageBytes, 0, allBytes, agentBytes.Length, languageBytes.Length);
        Buffer.BlockCopy(timeBytes, 0, allBytes, languageBytes.Length + agentBytes.Length, timeBytes.Length);

        var randGenerator = new Random();
        var start = randGenerator.Next(length / 4);
        var end = randGenerator.Next((length / 2) + 1, length);

        var truncatedBytes = new byte[end - start];
        Buffer.BlockCopy(allBytes, start, truncatedBytes, 0, end - start);

        var hashedbytes = SHA256.HashData(truncatedBytes);
        var base64State = Convert.ToBase64String(hashedbytes);
        var state = base64State.Replace(" ", "").Replace("+", "");

        return state;
    }

    [Benchmark]
    public static string WithStackAllocAndSpans()
    {
        return Utils.GenerateSafeRandomBase64String();
    }
}