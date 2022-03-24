using FluentValidation;

namespace SiteWatcher.Application.Validators;

public static class Rules
{
    public static IRuleBuilderOptions<T, string> HasOnlyLetters<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.Matches(@"^[A-Za-z ]*$");   
}