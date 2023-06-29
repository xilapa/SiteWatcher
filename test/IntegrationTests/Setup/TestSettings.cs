namespace IntegrationTests.Setup;

public sealed class TestSettings
{
    /// <summary>
    /// Testcontainers docker host environment variable.
    /// </summary>
    public string DOCKER_HOST { get; set; }
}