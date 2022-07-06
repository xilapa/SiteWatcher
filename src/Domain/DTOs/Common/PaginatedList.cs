namespace Domain.DTOs.Common;

public class PaginatedList<T>
{
    public PaginatedList()
    { }

    public PaginatedList(IEnumerable<T> results)
    {
        Results = results;
    }

    public int Total { get; set; }
    public IEnumerable<T> Results { get; set; }
}