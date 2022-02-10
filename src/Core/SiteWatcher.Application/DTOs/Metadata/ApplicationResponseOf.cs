using FluentValidation.Results;

namespace SiteWatcher.Application.DTOS.Metadata;

public class ApplicationResponseOf<T> : ApplicationResponse
{
    public ApplicationResponseOf(string message, object result, T internalResult) : base(message, result)
    {
        InternalResult = internalResult;
    }

    public ApplicationResponseOf(string message, T internalResult) : base(message, null)
    {
        InternalResult = internalResult;
    }

    public ApplicationResponseOf(ValidationResult validationResult) : base(validationResult)
    {
        
    }

    /// <summary>
    /// Resultado interno para quem chama.
    /// </summary>
    public T InternalResult { get; set; }
}