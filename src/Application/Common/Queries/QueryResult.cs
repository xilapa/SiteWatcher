namespace SiteWatcher.Application.Common.Queries;

public sealed class QueryResult
{
    public QueryResult(string sql, Dictionary<string, object> parameters)
    {
        Sql = sql;
        Parameters = parameters;
    }

    public string Sql { get; }
    public Dictionary<string,object> Parameters { get;  }
}