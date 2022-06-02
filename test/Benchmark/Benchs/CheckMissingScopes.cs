
using BenchmarkDotNet.Attributes;

namespace SiteWatcher.Benchmark.Benchs;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
public class CheckMissingScopes
{
    private readonly string defaultScopes = "apple orange lemon";
    private readonly string scopesToBeChecked = "car apple pepper orange bicycle lemon";

    /// <summary>
    /// The old way scopes are checked by the google auth controller.
    /// </summary>
    [Benchmark]
    public bool CheckScopesWithLinq()
    {
        return defaultScopes.Split(" ").Any(s => !scopesToBeChecked.Contains(s));
    }

    [Benchmark]
    public bool CheckScopesWithSpan()
    {
        return IsMissingScope(defaultScopes, scopesToBeChecked);
    }

    // Same logic implemented at GoogleAuthenticationCommandValidator, but without logging.
    private bool IsMissingScope(string defaultScopes, string? scopesToBeChecked)
    {
        var scopesToBeCheckedSpan = scopesToBeChecked.AsSpan();
        var scopeSpan = defaultScopes.AsSpan();

        var crrIdx = 0;
        var sepIdx = 0;
        var sepDist = scopeSpan.IndexOf(' ');

        while (true)
        {
            var scope = scopeSpan.Slice(crrIdx, sepDist);

            if (scopesToBeCheckedSpan.IndexOf(scope) == -1)
                return true;

            if (sepIdx == -1)
                break;

            crrIdx = crrIdx + sepDist + 1;
            sepIdx = scopeSpan[crrIdx..].IndexOf(' ');
            sepDist = sepIdx == -1 ? scopeSpan.Length - crrIdx : sepIdx;
        }

        return false;
    }
}