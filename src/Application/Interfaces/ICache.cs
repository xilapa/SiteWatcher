namespace SiteWatcher.Application.Interfaces;

public interface ICache
{
    /// <summary>
    /// Save a <see cref="string"/> to cache
    /// </summary>
    Task SaveStringAsync(string key, string value, TimeSpan expiration);
    Task SaveBytesAsync(string key, byte[] value, TimeSpan expiration);

    /// <summary>
    /// Get a <see cref="string"/> and remove it from cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<string> GetAndRemoveStringAsync(string key);

    /// <summary>
    /// Get a <see cref="byte"/> array and remove it from cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<byte[]?> GetAndRemoveBytesAsync(string key);
    Task<string> GetStringAsync(string key);
    Task<byte[]?> GetBytesAsync(string key);
}