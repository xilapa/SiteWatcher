
using BenchmarkDotNet.Attributes;
using SiteWatcher.WebAPI.Controllers;

namespace SiteWatcher.Benchmark.Benchs;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
public class CheckMissingScopes : GoogleAuthController
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
}