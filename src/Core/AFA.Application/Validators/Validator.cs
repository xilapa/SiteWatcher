using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using FluentValidation.Results;

namespace AFA.Application.Validators;

public static class Validator
{
    private static Dictionary<Type, object> _validators;

    public static ValidationResult Validate<T>(this IValidable<T> validationTarget) where T : new()
    {
        if(_validators is null)
            _validators = new Dictionary<Type, object>();

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
            _validators.Add(validatorAbstractType, validator);
        }       

        var validationResult = (validator as AbstractValidator<T>).Validate((T)validationTarget);
        return validationResult;
    }
}