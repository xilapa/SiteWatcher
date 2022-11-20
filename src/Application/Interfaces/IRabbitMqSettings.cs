namespace SiteWatcher.Application.Interfaces;

public interface IRabbitMqSettings
{
    public string Host { get; }
    public string VirtualHost { get; }
    public string UserName { get; }
    public string Password { get; }
    public int Port { get; }
}