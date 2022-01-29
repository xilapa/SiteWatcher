using System;
using System.Threading.Tasks;

namespace AFA.Domain.Interfaces;

public interface IFireForgetService
{
    void ExecuteWith<T>(Func<T, Task> func) where T: IInjectedService;
}