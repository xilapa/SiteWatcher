using Microsoft.AspNetCore.Authorization;

namespace SiteWatcher.Infra.Authorization;

public class ValidAuthData : IAuthorizationRequirement
{ }

public class ValidRegisterData : IAuthorizationRequirement
{ }