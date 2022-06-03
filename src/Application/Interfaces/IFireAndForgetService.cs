namespace SiteWatcher.Application.Interfaces;

public interface IFireAndForgetService
{
    void ExecuteWith<T>(Func<T, Task> func) where T: notnull;
    void ExecuteWith<T1,T2>(Func<T1, T2, Task> func) where T1: notnull where T2: notnull;
}