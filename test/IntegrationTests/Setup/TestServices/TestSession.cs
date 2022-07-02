using Microsoft.AspNetCore.Http;
using SiteWatcher.Infra.Authorization;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class TestSession : Session
{
    public TestSession(IHttpContextAccessor httpContextAccessor, DateTime currentTime) : base(httpContextAccessor)
    {
        _currentTime = currentTime;
    }

    private readonly DateTime _currentTime;
    public override DateTime Now => _currentTime;
}