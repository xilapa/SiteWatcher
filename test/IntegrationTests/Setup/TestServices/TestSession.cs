using Microsoft.AspNetCore.Http;
using SiteWatcher.Infra.Authorization;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class TestSession : Session
{
    public TestSession(IHttpContextAccessor httpContextAccessor, DateTime currentTime) : base(httpContextAccessor)
    {
        _currentTime = currentTime;
    }

    private DateTime _currentTime;
    public override DateTime Now => _currentTime;

    public void SetNewDate(DateTime newDate) => _currentTime = newDate;
}