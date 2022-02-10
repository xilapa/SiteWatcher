using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace SiteWatcher.Application.DTOS.Metadata;

public class ApplicationResponse
{
    public ApplicationResponse()
    {
        _errors = new List<string>();
    }

    public ApplicationResponse(ValidationResult validationResult) : this()
    {
        if(!validationResult.IsValid)            
            _errors.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    public ApplicationResponse(IEnumerable<string> errors) : this()
    {
        _errors.AddRange(errors);
    }

    public ApplicationResponse(string message) : this()
    {
        Message = message;
    }

    public ApplicationResponse(string message, object result) : this()
    {
        Message = message;
        Result = result;
    }

    public void AddError(string error)
    {
        _errors.Add(error);
    }

    private readonly List<string> _errors;
    public IReadOnlyCollection<string> Errors => _errors;
    public bool Success => !Errors.Any();

    /// <summary>
    /// Mensagem adicionada em caso de sucesso.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Objeto passado como resultado da ação para o usuário final.
    /// </summary>
    public object Result { get; set; }
}