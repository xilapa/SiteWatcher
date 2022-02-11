using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;

namespace SiteWatcher.Application.Validators;

public static class Validator
{
    private static Dictionary<Type, object> _validators;

    public static ValidationResult Validate<T>(this IValidable<T> validationTarget) where T : new()
    {
        if(_validators is null)
            throw new Exception("Chame o método AddFluentValidators() antes de realizar uma validação.");

        var targetType = validationTarget.GetType();

        try
        {
            var validator = _validators[targetType] as AbstractValidator<T>;
            return validator.Validate((T)validationTarget);
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Não foi definida uma implementação de AbstractValidator para {targetType}.");
        }
    }

    public static void LoadFluentValidators()
    {
        var validatorsTypes = Assembly
                                .GetExecutingAssembly()
                                .GetTypes()
                                .Where(t => 
                                    t.BaseType is not null 
                                    && t.BaseType.IsGenericType
                                    && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                                .ToArray();        
        
        var capacity = validatorsTypes.Length; 
        _validators = new Dictionary<Type, object>(capacity);

        foreach(var validatorType in validatorsTypes)
        {
            var targetType = validatorType.BaseType.GenericTypeArguments[0];
            var validator = Activator.CreateInstance(validatorType);
            _validators.Add(targetType, validator);
        }
    }
}