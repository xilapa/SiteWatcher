using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Primitives;

namespace SiteWatcher.Benchmark.Benchs;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
public class GetAuthTokenPayload
{
    private readonly StringValues _token = "TeSteJhbGciOiJIUzIyhIsInR5cCI6IkpXVCJ9.eyJpZCI6ImI4ZDg1YTU2LTk4MWYtNDBkZi04MTIwLTE0MjkxOTlhNmQxNiIsIm5hbWUiOiJEaXJjZXUgSnVuaW9yIiwiZW1haWwiOiJkaXJjZXUuc2pyQGdtYWlsLmNvbSIsImVtYWlsLWNvbmZp4m1lZCI6InRy54UiLCJsYW5ndWFnZSI6IjEiLCJuYmYiOjE2NDg5MzMyMjYsImV1cCI6MTY0ODk2MjAyNiwiaWF0IjoxNjQ4OTMzMj22fQ.zLxmBw5TgCALner_wYo6xQA267y6jFdGXdS64hdtNSxBMTVFU";

    /// <summary>
    /// How the auth token payload was extracted
    /// </summary>
    [Benchmark]
    public string WithouSpan()
    {
        if(string.IsNullOrEmpty(_token)) 
            return null;

        var tokenWithoutBearer = _token.ToString().Replace("Bearer ","");
        var firstCharAfterFirstDotIndex = tokenWithoutBearer.IndexOf('.') + 1;
        var secondDotIndex = tokenWithoutBearer.IndexOf('.', firstCharAfterFirstDotIndex);
        var payload = tokenWithoutBearer[ firstCharAfterFirstDotIndex .. secondDotIndex ];
        return payload;
    }

    [Benchmark]
    public string WithSpan()
    {
        if(string.IsNullOrEmpty(_token))
            return string.Empty;

        var tokenString = _token.ToString();
        var tokenSpan = tokenString.AsSpan();
        var firstDotIdx = tokenSpan.IndexOf('.') + 1;
        var secondDotIdx = tokenSpan[firstDotIdx..].IndexOf('.');

        return tokenString[firstDotIdx .. secondDotIdx];
    }
}