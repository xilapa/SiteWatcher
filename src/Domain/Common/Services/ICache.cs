namespace SiteWatcher.Domain.Common.Services;

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
    Task<string?> GetAndRemoveStringAsync(string key);

    /// <summary>
    /// Get a <see cref="byte"/> array and remove it from cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<byte[]?> GetAndRemoveBytesAsync(string key);
    Task<string?> GetStringAsync(string key);
    Task<byte[]?> GetBytesAsync(string key);
    Task SaveAsync(string key, object? value, TimeSpan expiration);
    Task<T?> GetAsync<T>(string key);
    Task SaveHashAsync(string key, string fieldName, object fieldValue, TimeSpan expiration);
    Task<string?> GetHashFieldAsStringAsync(string key, string fieldName);
    Task DeleteKeyAsync(string key);

    /// <summary>
    /// Remove keys which contains the partial key.
    /// </summary>
    /// <param name="partialKey">partial key used to get the keys to be removed from cache</param>
    Task DeleteKeysWith(string partialKey);
}