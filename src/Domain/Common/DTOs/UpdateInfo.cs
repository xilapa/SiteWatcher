namespace SiteWatcher.Domain.Common.DTOs;

public sealed class UpdateInfo<T>
{
    public UpdateInfo()
    { }

    public UpdateInfo(T? newValue)
    {
        NewValue = newValue;
    }

    public T? NewValue { get; set; }
}