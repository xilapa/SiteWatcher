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
            throw new Exception("Call AddFluentValidators() before validating an object.");

        var targetType = validationTarget.GetType();

        if(!_validators.TryGetValue(targetType, out var validator))
            return new ValidationResult();

        return (validator as AbstractValidator<T>).Validate((T)validationTarget);
    }

    public static ValidationResult Validate(object validationTarget, Type targetType)
    {
        if(_validators is null)
            throw new Exception("Call AddFluentValidators() before validating an object.");        

        if(!_validators.TryGetValue(targetType, out var validator))
            return new ValidationResult(); 

        var methodInfo = typeof(AbstractValidator<>).MakeGenericType(targetType).GetMethods()
                                            .Where(m => m.IsFinal && m.Name == "Validate")
                                            .FirstOrDefault();  

        if (methodInfo is null)
            throw new Exception("Validation method not found");

        var res = methodInfo.Invoke(validator, new object[] { validationTarget });

        return res as ValidationResult;
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