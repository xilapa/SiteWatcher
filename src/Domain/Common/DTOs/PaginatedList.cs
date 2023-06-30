namespace SiteWatcher.Domain.Common.DTOs;

public sealed class PaginatedList<T>
{
    public PaginatedList()
    {
        Results = Array.Empty<T>();
    }

    public PaginatedList(T[] results, int total)
    {
        Results = results;
        Total = total;
    }

    public int Total { get; set; }
    public T[] Results { get; set; }
}