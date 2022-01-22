using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace AFA.Application.DTOS.Metadata;

public sealed class ApplicationResponse
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

    public void AddError(string error)
    {
        _errors.Add(error);
    }

    private List<string> _errors;
    public IReadOnlyCollection<string> Errors => _errors;
    public bool Success => !Errors.Any();

    /// <summary>
    /// Mensagem adicionada em caso de sucesso.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Objeto passado como resultado da ação.
    /// </summary>
    public object Result { get; set; }
}