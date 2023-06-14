namespace IntegrationTests.Setup;

public sealed class TestSettings
{
    /// <summary>
    /// Testcontainers docker host environment variable.
    /// </summary>
    public string DOCKER_HOST { get; set; }

    /// <summary>
    /// Testcontainers, file path to the Docker daemon socket that is used by Ryuk (resource reaper).
    /// </summary>
    public string TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE { get; set; }
}