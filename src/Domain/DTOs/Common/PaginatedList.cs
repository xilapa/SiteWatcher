namespace Domain.DTOs.Common;

public class PaginatedList<T>
{
    public PaginatedList()
    { }

    public PaginatedList(IEnumerable<T> results, int total)
    {
        Results = results;
        Total = total;
    }

    public int Total { get; set; }
    public IEnumerable<T> Results { get; set; }
}