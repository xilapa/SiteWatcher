using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;

namespace AFA.Application.Validators;

public static class Validator
{
    static Validator()
    {
        // deve ser maior que a quantidade de AbstractValidators no assembly para evitar resize do dicionário
        var initialCapacity = 32; 
        var concurrencyLevel = Environment.ProcessorCount;
        _validators = new ConcurrentDictionary<Type, object>(concurrencyLevel, initialCapacity);
    }

    private static ConcurrentDictionary<Type, object> _validators;

    public static ValidationResult Validate<T>(this IValidable<T> validationTarget) where T : new()
    {
        var targetType = validationTarget.GetType();
        var validatorAbstractBaseType = typeof(AbstractValidator<>);
        var validatorAbstractType = validatorAbstractBaseType.MakeGenericType(targetType);

        object validator;

        if(!_validators.TryGetValue(validatorAbstractType, out validator))
        {
            var validatorType = Assembly
                                    .GetExecutingAssembly()
                                    .GetTypes()
                                    .Single(t => t.IsAssignableTo(validatorAbstractType));

            validator = Activator.CreateInstance(validatorType);
            _validators.TryAdd(validatorAbstractType, validator);
        }       

        var validationResult = (validator as AbstractValidator<T>).Validate((T)validationTarget);
        return validationResult;
    }
}