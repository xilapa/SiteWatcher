using FluentValidation;

namespace SiteWatcher.Application.Common.Validation;

public static class Rules
{
    //TODO: trocar regex por loop, para reduzir uso de memória
    public static IRuleBuilderOptions<T, string> HasOnlyLetters<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.Matches("^[A-Za-z ]*$");
}