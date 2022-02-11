namespace SiteWatcher.Application.Validators;

/// <summary>
/// Interface para marcação de objetos que podem ser validados.
/// </summary>
/// <typeparam name="T">Objeto a ser validado</typeparam>
public interface IValidable<T> where T : new()
{

}