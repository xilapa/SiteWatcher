namespace Domain.DTOs.Common;

public class UpdateInfo<T>
{
    public UpdateInfo()
    { }

    public UpdateInfo(T? newValue)
    {
        NewValue = newValue;
    }

    public T? NewValue { get; set; }
}