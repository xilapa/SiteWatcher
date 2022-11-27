namespace SiteWatcher.Domain.Common.DTOs;

public sealed class PaginatedList<T>
{
    public PaginatedList()
    {
        Results = Enumerable.Empty<T>();
    }

    public PaginatedList(IEnumerable<T> results, int total)
    {
        Results = results;
        Total = total;
    }

    public int Total { get; set; }
    public IEnumerable<T> Results { get; set; }
}